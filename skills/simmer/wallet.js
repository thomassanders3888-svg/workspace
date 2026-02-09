// Simmer Wallet Manager
// Polygon wallet utilities for Polymarket trading

const fs = require('fs');
const path = require('path');
const { ethers } = require('ethers');

const CONFIG_DIR = path.join(process.env.HOME, '.simmer');
const WALLET_FILE = path.join(CONFIG_DIR, 'wallet.json');
const USDC_ADDRESS = '0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174'; // Polygon USDC

class SimmerWallet {
  constructor() {
    this.wallet = null;
    this.provider = null;
    this.initProvider();
  }

  initProvider() {
    const rpcUrl = process.env.POLYGON_RPC_URL || 'https://polygon-rpc.com';
    this.provider = new ethers.JsonRpcProvider(rpcUrl);
  }

  // Create new wallet
  createWallet(password) {
    const wallet = ethers.Wallet.createRandom();
    this.wallet = wallet;
    this.saveWallet(wallet, password);
    return {
      address: wallet.address,
      mnemonic: wallet.mnemonic.phrase
    };
  }

  // Import from private key
  importPrivateKey(privateKey) {
    this.wallet = new ethers.Wallet(privateKey, this.provider);
    return this.wallet.address;
  }

  // Import from mnemonic
  importMnemonic(mnemonic) {
    this.wallet = ethers.Wallet.fromPhrase(mnemonic);
    this.wallet = this.wallet.connect(this.provider);
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
    this.wallet = this.wallet.connect(this.provider);
    return this.wallet.address;
  }

  // Get USDC balance
  async getUSDCBalance() {
    if (!this.wallet) {
      throw new Error('Wallet not loaded');
    }

    const usdcAbi = ['function balanceOf(address) view returns (uint256)', 'function decimals() view returns (uint8)'];
    const usdcContract = new ethers.Contract(USDC_ADDRESS, usdcAbi, this.provider);
    
    const balance = await usdcContract.balanceOf(this.wallet.address);
    const decimals = await usdcContract.decimals();
    
    return {
      raw: balance.toString(),
      formatted: ethers.formatUnits(balance, decimals),
      address: this.wallet.address
    };
  }

  // Get MATIC balance for gas
  async getMaticBalance() {
    if (!this.wallet) {
      throw new Error('Wallet not loaded');
    }
    
    const balance = await this.provider.getBalance(this.wallet.address);
    return {
      raw: balance.toString(),
      formatted: ethers.formatEther(balance),
      address: this.wallet.address
    };
  }

  // Check if wallet exists
  walletExists() {
    return fs.existsSync(WALLET_FILE);
  }

  // Get address without loading
  getAddress() {
    if (this.wallet) return this.wallet.address;
    return null;
  }

  // Sign message
  async signMessage(message) {
    if (!this.wallet) throw new Error('Wallet not loaded');
    return await this.wallet.signMessage(message);
  }
}

module.exports = SimmerWallet;
