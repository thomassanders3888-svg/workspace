const { ethers } = require('ethers');

// Configuration
const USDC_CONTRACT = '0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174';
const WALLET_ADDRESS = '0xb99a3Ed7d209DdaF69FAB477B52857701e245CE9';
const RPC_URLS = [
  'https://gateway.tenderly.co/public/polygon',
  'https://polygon.drpc.org'
];

// USDC ABI (minimal)
const USDC_ABI = [
  'function balanceOf(address) view returns (uint256)',
  'function decimals() view returns (uint8)'
];

const { execSync } = require('child_process');

async function getProvider() {
  for (const url of RPC_URLS) {
    try {
      const provider = new ethers.JsonRpcProvider(url);
      await provider.getBlockNumber();
      console.log(`Connected to: ${url}`);
      return provider;
    } catch (e) {
      console.log(`Failed: ${url}`);
    }
  }
  throw new Error('No RPC available');
}

function sendWhatsApp(amount) {
  const msg = `Bridge complete! $${amount.toFixed(2)} USDC on Polygon. Trader can now trade.`;
  try {
    execSync(`npx openclaw message send --target "+19402304961" --message "${msg}"`, {
      cwd: '/home/clawdbot267/.openclaw/workspace',
      encoding: 'utf8'
    });
    console.log('WhatsApp sent successfully');
    return true;
  } catch (e) {
    console.log('WhatsApp failed:', e.message);
    return false;
  }
}

async function checkBalance() {
  const provider = await getProvider();
  const usdc = new ethers.Contract(USDC_CONTRACT, USDC_ABI, provider);
  const decimals = await usdc.decimals();
  const balanceRaw = await usdc.balanceOf(WALLET_ADDRESS);
  const balance = Number(ethers.formatUnits(balanceRaw, decimals));
  return balance;
}

async function main() {
  console.log('Starting Polygon bridge monitor...');
  console.log('Wallet:', WALLET_ADDRESS);
  console.log('USDC:', USDC_CONTRACT);
  console.log('Will check every 60 seconds for 30 minutes max\n');

  let alerted = false;
  const startTime = Date.now();
  const MAX_DURATION = 30 * 60 * 1000;

  while (!alerted && (Date.now() - startTime) < MAX_DURATION) {
    try {
      const balance = await checkBalance();
      const elapsed = Math.floor((Date.now() - startTime) / 1000);
      console.log(`[${elapsed}s] Balance: $${balance.toFixed(4)} USDC`);

      if (balance >= 1.0) {
        console.log('\nBridge detected! Sending alert...');
        if (sendWhatsApp(balance)) {
          alerted = true;
          console.log('Alert sent. Stopping monitor.');
          break;
        }
      }
    } catch (e) {
      console.error('Check failed:', e.message);
    }

    await new Promise(r => setTimeout(r, 60000));
  }

  if (!alerted) {
    console.log('\nTimeout reached (30 min) or stopped. Bridge not completed.');
  }

  process.exit(0);
}

main();
