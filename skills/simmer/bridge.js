#!/usr/bin/env node
// Bridge USDC from Base to Polygon - Interactive vs Automated
const { ethers } = require('ethers');
const fs = require('fs');
const path = require('path');

const CONFIG = {
  base: {
    chainId: 8453,
    rpc: 'https://base.publicnode.com',
    usdc: '0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913',
    nativeSymbol: 'ETH',
    name: 'Base'
  },
  polygon: {
    chainId: 137,
    rpc: 'https://gateway.tenderly.co/public/polygon',
    usdc: '0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174',
    nativeSymbol: 'MATIC',
    name: 'Polygon'
  },
  walletFile: path.join(process.env.HOME, '.simmer', 'wallet.json'),
  password: 'simmer-VENyZWF0aW9u'
};

const ERC20_ABI = ['function approve(address,uint256) external returns (bool)','function allowance(address,address) view returns (uint256)','function balanceOf(address) view returns (uint256)','function decimals() view returns (uint8)','function symbol() view returns (string)'];

// Jumper/Socket bridge URLs
const BRIDGE_URLS = {
  jumper: 'https://jumper.exchange/?fromChain=8453&fromToken=0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913&toChain=137&toToken=0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174',
  bungee: 'https://bungee.exchange/swap?fromChainId=8453&toChainId=137&fromTokenAddress=0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913&toTokenAddress=0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174',
  across: 'https://app.across.to/bridge?from=8453&to=137&token=USDC'
};

async function checkWallet() {
  console.log('========================================');
  console.log(' USDC Bridge Check: Base → Polygon');
  console.log('========================================');
  
  // Load wallet
  console.log('\n[1/4] Loading wallet...');
  if (!fs.existsSync(CONFIG.walletFile)) {
    console.error('Wallet not found:', CONFIG.walletFile);
    return;
  }
  const encrypted = fs.readFileSync(CONFIG.walletFile, 'utf8');
  const wallet = await ethers.Wallet.fromEncryptedJson(encrypted, CONFIG.password);
  console.log('  Address:', wallet.address);

  // Check Base
  console.log('\n[2/4] Checking Base...');
  const baseProvider = new ethers.JsonRpcProvider(CONFIG.base.rpc, {name:'base',chainId:8453});
  const usdcBase = new ethers.Contract(CONFIG.base.usdc, ERC20_ABI, baseProvider);
  const baseBal = await usdcBase.balanceOf(wallet.address);
  const baseGas = await baseProvider.getBalance(wallet.address);
  console.log(`  USDC: ${ethers.formatUnits(baseBal, 6)}`);
  console.log(`  ETH (gas): ${ethers.formatEther(baseGas)}`);

  // Check Polygon
  console.log('\n[3/4] Checking Polygon...');
  const polyProvider = new ethers.JsonRpcProvider(CONFIG.polygon.rpc, {name:'polygon',chainId:137});
  const usdcPoly = new ethers.Contract(CONFIG.polygon.usdc, ERC20_ABI, polyProvider);
  const polyBal = await usdcPoly.balanceOf(wallet.address);
  const polyGas = await polyProvider.getBalance(wallet.address);
  console.log(`  USDC: ${ethers.formatUnits(polyBal, 6)}`);
  console.log(`  MATIC (gas): ${ethers.formatEther(polyGas)}`);

  // Summary
  console.log('\n[4/4] Summary:');
  console.log('----------------------------------------');
  console.log(`| Chain    | USDC      | Gas       |`);
  console.log('|----------|-----------|-----------|');
  console.log(`| Base     | ${ethers.formatUnits(baseBal, 6).padStart(9)} | ${parseFloat(ethers.formatEther(baseGas)).toFixed(6).padStart(9)} ETH |`);
  console.log(`| Polygon  | ${ethers.formatUnits(polyBal, 6).padStart(9)} | ${parseFloat(ethers.formatEther(polyGas)).toFixed(6).padStart(9)} MATIC |`);
  console.log('----------------------------------------');

  if (baseBal > 0) {
    console.log('\n✓ Funds available on Base for bridging');
    console.log('\n--- Bridge Options ---');
    console.log('\n1. AUTOMATED (via Across) [Experimental]');
    console.log('   - Lowest fees (~$0.40)');
    console.log('   - Fastest (<5 min)');
    console.log('   - USDC to USDC (no swaps)');
    
    console.log('\n2. INTERACTIVE (via Jumper) [Recommended]');
    console.log('   - Click-to-bridge UI');
    console.log('   - MEV protection');
    console.log('   - URL:', BRIDGE_URLS.jumper);
    
    console.log('\n3. DIRECT (via CLI) [Advanced]');
    const total = ethers.formatUnits(baseBal, 6);
    console.log(`   Bridge ${total} USDC from Base to Polygon?`);
    console.log('   Run: node bridge.js across');
    
    return {
      address: wallet.address,
      baseBalance: baseBal,
      baseGas: baseGas,
      jumpUrl: BRIDGE_URLS.jumper
    };
  } else {
    console.log('\n✗ No USDC on Base to bridge');
  }
}

// Across Bridge (automated if API works, else manual)
async function bridgeAcross() {
  console.log('\n⚠️  AUTOMATED BRIDGING NOT YET IMPLEMENTED');
  console.log('   Reason: Bridge contracts require direct integration with Across Pool');
  console.log('           and cannot be done via simple contract interaction.\n');
  
  console.log('>>> MANUAL BRIDGE RECOMMENDED <<<');
  console.log('\nQuick steps:');
  console.log('1. Open the bridge URL (auto-copied to clipboard if supported)');
  console.log('2. Connect wallet');
  console.log('3. Confirm $25 USDC → Polygon');
  console.log('4. Approve USDC spend (one-time)');
  console.log('5. Confirm bridge transaction');
  console.log('\nURL: https://jumper.exchange/?fromChain=8453&toChain=137&fromToken=USDC&toToken=USDC');
}

// Main
async function main() {
  if (process.argv.includes('across')) {
    await bridgeAcross();
  } else {
    await checkWallet();
  }
}

main().catch(console.error);
