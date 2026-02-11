// Simmer Wallet Manager - Multi-chain support (Polygon + Ethereum)
const fs = require('fs');
const path = require('path');
const { ethers } = require('ethers');

const CONFIG_DIR = path.join(process.env.HOME, '.simmer');
const WALLET_FILE = path.join(CONFIG_DIR, 'wallet.json');

// USDC contracts
const USDC_POLYGON_NATIVE = '0x3c499c542cef5e3811e1192ce70d8cc03d5c3359'; // Native USDC on Polygon (Circle)
const USDC_POLYGON_BRIDGED = '0x2791bca1f2de4661ed88a30c99a7a9449aa84174'; // Bridged USDC on Polygon (old)
const USDC_ETHEREUM = '0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48';
const USDC_BASE = '0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913';

const CHAINS = {
  ethereum: {
    id: 1,
    name: 'Ethereum',
    usdc: USDC_ETHEREUM,
    rpcs: [
      'https://ethereum.publicnode.com',
      'https://rpc.ankr.com/eth',
      'https://cloudflare-eth.com'
    ]
  },
  base: {
    id: 8453,
    name: 'Base',
    usdc: USDC_BASE,
    rpcs: [
      'https://base.publicnode.com',
      'https://base.llamarpc.com',
      'https://1rpc.io/base',
      'https://rpc.ankr.com/base'
    ]
  },
  polygon: {
    id: 137,
    name: 'Polygon',
    usdc: USDC_POLYGON_NATIVE, // Use native Circle USDC
    rpcs: [
      'https://gateway.tenderly.co/public/polygon',
      'https://polygon.drpc.org',
      'https://polygon-rpc.com'
    ]
  }
};

class SimmerWallet {
  constructor() {
    this.wallet = null;
    this.providers = {};
    this.currentChain = 'polygon';
  }

  // Create new wallet
  createWallet(password) {
    const wallet = ethers.Wallet.createRandom();
    this.wallet = wallet;
    this.saveWallet(wallet, password);
    return { address: wallet.address, mnemonic: wallet.mnemonic.phrase };
  }

  // Import from private key
  importPrivateKey(privateKey) {
    this.wallet = new ethers.Wallet(privateKey);
    return this.wallet.address;
  }

  // Import from mnemonic
  importMnemonic(mnemonic) {
    const wallet = ethers.Wallet.fromPhrase(mnemonic);
    this.wallet = wallet;
    return this.wallet.address;
  }

  // Save encrypted wallet
  saveWallet(wallet, password) {
    if (!fs.existsSync(CONFIG_DIR)) {
      fs.mkdirSync(CONFIG_DIR, { recursive: true });
    }
    const encrypted = wallet.encryptSync(password);
    fs.writeFileSync(WALLET_FILE, encrypted);
  }

  // Load wallet
  loadWallet(password) {
    if (!fs.existsSync(WALLET_FILE)) {
      return null;
    }
    const encrypted = fs.readFileSync(WALLET_FILE, 'utf8');
    this.wallet = ethers.Wallet.fromEncryptedJsonSync(encrypted, password);
    return this.wallet.address;
  }

  // Get provider for chain
  getProvider(chainName) {
    const urls = CHAINS[chainName].rpcs;
    return new ethers.FallbackProvider(
      urls.map(url => new ethers.JsonRpcProvider(url, { name: chainName, chainId: CHAINS[chainName].id })),
      { quorum: 1 }
    );
  }

  // Get address without loading
  getAddress() {
    if (this.wallet) return this.wallet.address;
    return null;
  }

  // Get USDC balance on any chain
  async getUSDCBalance(chain = this.currentChain) {
    if (!this.wallet) {
      throw new Error('Wallet not loaded');
    }
    
    const config = CHAINS[chain];
    const address = this.wallet.address;
    
    for (const url of config.rpcs) {
      try {
        const provider = new ethers.JsonRpcProvider(url, { name: chain, chainId: config.id });
        const abi = ['function balanceOf(address) view returns (uint256)', 'function decimals() view returns (uint8)'];
        const contract = new ethers.Contract(config.usdc, abi, provider);
        
        const [balance, decimals] = await Promise.all([
          contract.balanceOf(address),
          contract.decimals()
        ]);
        
        return {
          chain: chain,
          chainId: config.id,
          raw: balance.toString(),
          formatted: ethers.formatUnits(balance, decimals),
          address: address,
          usdcContract: config.usdc,
          rpc: url
        };
      } catch (e) {
        console.log(`[Wallet] ${chain} RPC ${url} failed: ${e.message}`);
        continue;
      }
    }
    
    return {
      chain: chain,
      chainId: config.id,
      raw: '0',
      formatted: '0.0',
      address: address,
      error: 'All RPCs failed'
    };
  }

  // Check ALL chains for USDC
  async checkAllChainsUSDC() {
    const results = {};
    for (const chain of Object.keys(CHAINS)) {
      results[chain] = await this.getUSDCBalance(chain);
    }
    return results;
  }

  // Get native balance (ETH or MATIC)
  async getNativeBalance(chain = this.currentChain) {
    if (!this.wallet) {
      throw new Error('Wallet not loaded');
    }
    
    const config = CHAINS[chain];
    const address = this.wallet.address;
    
    for (const url of config.rpcs) {
      try {
        const provider = new ethers.JsonRpcProvider(url, { name: chain, chainId: config.id });
        const balance = await provider.getBalance(address);
        
        return {
          chain: chain,
          raw: balance.toString(),
          formatted: ethers.formatEther(balance),
          address: address,
          token: config.name === 'Ethereum' ? 'ETH' : 'MATIC',
          rpc: url
        };
      } catch (e) {
        continue;
      }
    }
    
    return {
      chain: chain,
      raw: '0',
      formatted: '0.0',
      token: config.name === 'Ethereum' ? 'ETH' : 'MATIC',
      error: 'All RPCs failed'
    };
  }

  // Check if wallet exists
  walletExists() {
    return fs.existsSync(WALLET_FILE);
  }

  // Switch active chain
  switchChain(chain) {
    if (CHAINS[chain]) {
      this.currentChain = chain;
      return true;
    }
    return false;
  }

  // Sign message
  async signMessage(message) {
    if (!this.wallet) throw new Error('Wallet not loaded');
    return await this.wallet.signMessage(message);
  }

  // Sign order
  async signOrder(orderData) {
    if (!this.wallet) throw new Error('Wallet not loaded');
    const message = JSON.stringify({
      marketId: orderData.marketId,
      side: orderData.side,
      size: orderData.size,
      price: orderData.price,
      timestamp: orderData.timestamp
    });
    const signature = await this.signMessage(message);
    return { ...orderData, signature, signer: this.wallet.address };
  }
}

module.exports = SimmerWallet;
