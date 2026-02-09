#!/usr/bin/env node
// Transfer USDC from Wallet 1 to Wallet 2

const { ethers } = require('ethers');

const USDC = '0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174';
const WALLET1 = '0xa9fBB45814760fF8eDDbd338AC5b228D8a9680AE';
const WALLET2 = '0xb99a3Ed7d209DdaF69FAB477B52857701e245CE9';

const MNEMONIC1 = 'because fabric avocado clever drop arctic pigeon rhythm grocery crowd uncle hello';

async function transfer() {
  console.log('╔════════════════════════════════════════════════╗');
  console.log('║    Transfer USDC: Wallet 1 → Wallet 2         ║');
  console.log('╚════════════════════════════════════════════════╝');
  console.log();
  
  const rpcs = [
    'https://rpc.ankr.com/polygon',
    'https://polygon.llamarpc.com',
    'https://polygon-rpc.com',
    'https://polygon-mainnet.public.blastapi.io'
  ];
  
  let provider;
  for (const rpc of rpcs) {
    try {
      provider = new ethers.JsonRpcProvider(rpc);
      await provider.getBlockNumber();
      console.log('✓ Connected:', rpc);
      break;
    } catch (e) {}
  }
  
  if (!provider) {
    console.log('❌ No working RPC found. Try again later.');
    process.exit(1);
  }
  
  // Restore wallet 1
  const wallet1 = ethers.Wallet.fromPhrase(MNEMONIC1, provider);
  console.log('Wallet 1:', wallet1.address);
  console.log('Wallet 2:', WALLET2);
  console.log();
  
  // Check balances
  const abi = [
    'function balanceOf(address) view returns (uint256)',
    'function decimals() view returns (uint8)',
    'function transfer(address,uint256) returns (bool)'
  ];
  
  const usdc = new ethers.Contract(USDC, abi, wallet1);
  
  const [usdcBal, dec, maticBal] = await Promise.all([
    usdc.balanceOf(wallet1.address),
    usdc.decimals(),
    provider.getBalance(wallet1.address)
  ]);
  
  const usdcFormatted = ethers.formatUnits(usdcBal, dec);
  const maticFormatted = ethers.formatEther(maticBal);
  
  console.log('Wallet 1 Balances:');
  console.log('  USDC:', usdcFormatted);
  console.log('  MATIC (gas):', maticFormatted);
  console.log();
  
  if (parseFloat(usdcFormatted) <= 0) {
    console.log('❌ No USDC to transfer');
    process.exit(1);
  }
  
  if (parseFloat(maticFormatted) < 0.01) {
    console.log('❌ Insufficient MATIC for gas (~0.01 needed)');
    console.log('Fund with MATIC first, then retry.');
    process.exit(1);
  }
  
  // Transfer all USDC minus small buffer for gas
  const amount = usdcBal;
  const amountFormatted = ethers.formatUnits(amount, dec);
  
  console.log('Transferring:', amountFormatted, 'USDC');
  console.log('To:', WALLET2);
  console.log();
  
  try {
    const tx = await usdc.transfer(WALLET2, amount);
    console.log('Transaction:', tx.hash);
    console.log('Waiting for confirmation...');
    
    await tx.wait();
    console.log('✅ Transfer complete!');
    console.log();
    
    // Verify
    const newBal = await usdc.balanceOf(wallet1.address);
    console.log('Wallet 1 USDC now:', ethers.formatUnits(newBal, dec));
    
    const w2Bal = await usdc.balanceOf(WALLET2);
    console.log('Wallet 2 USDC now:', ethers.formatUnits(w2Bal, dec));
    
  } catch (e) {
    console.log('❌ Transfer failed:', e.message);
    process.exit(1);
  }
}

transfer().catch(console.error);
