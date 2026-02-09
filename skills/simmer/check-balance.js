#!/usr/bin/env node
// Check USDC balance for wallet 1

const { ethers } = require('ethers');

const USDC = '0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174';
const WALLET = '0xa9fBB45814760fF8eDDbd338AC5b228D8a9680AE';

const rpcs = [
  'https://rpc.ankr.com/polygon',
  'https://polygon.llamarpc.com',
  'https://polygon-rpc.com'
];

const abi = ['function balanceOf(address) view returns (uint256)'];

async function check() {
  for (const rpc of rpcs) {
    try {
      const provider = new ethers.JsonRpcProvider(rpc);
      const usdc = new ethers.Contract(USDC, abi, provider);
      const bal = await usdc.balanceOf(WALLET);
      const formatted = ethers.formatUnits(bal, 6);
      
      console.log('Wallet 1:', WALLET);
      console.log('USDC:', formatted, '($' + parseFloat(formatted).toFixed(2) + ')');
      
      if (parseFloat(formatted) > 0) {
        console.log('\n✅ FUNDS FOUND IN WALLET 1!');
        console.log('Run: node transfer-to-wallet2.js to move funds');
      }
      return;
    } catch (e) {
      console.log('Trying', rpc, '- failed');
    }
  }
  console.log('All RPCs failed. Check manually: https://polygonscan.com/address/' + WALLET);
}

check();
