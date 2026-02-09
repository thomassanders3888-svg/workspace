#!/usr/bin/env node
// Simmer Setup - Creates .env and wallet

const fs = require('fs');
const path = require('path');
const readline = require('readline');

const CONFIG_DIR = path.join(process.env.HOME, '.simmer');
const ENV_FILE = path.join(CONFIG_DIR, '.env');

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

console.log('=== Simmer Weather Arbitrage Bot Setup ===\n');
console.log('This will create your configuration file.');
console.log('Your wallet seed phrase will be shown ONCE. Save it!\n');

async function ask(q) {
  return new Promise(resolve => rl.question(q, resolve));
}

async function setup() {
  // Create config directory
  if (!fs.existsSync(CONFIG_DIR)) {
    fs.mkdirSync(CONFIG_DIR, { recursive: true });
    console.log('Created:', CONFIG_DIR);
  }

  // Ask for wallet password
  const password = await ask('Set wallet password: ');
  const confirm = await ask('Confirm password: ');
  
  if (password !== confirm) {
    console.log('Passwords do not match!');
    process.exit(1);
  }

  // Check/create wallet
  const SimmerWallet = require('./wallet');
  const wallet = new SimmerWallet();
  
  let address;
  if (wallet.walletExists()) {
    console.log('\nExisting wallet found!');
    address = wallet.loadWallet(password);
    console.log('Loaded wallet:', address);
  } else {
    console.log('\nCreating new wallet...');
    const { address: newAddr, mnemonic } = wallet.createWallet(password);
    address = newAddr;
    
    console.log('\n' + '='.repeat(50));
    console.log('!!! SAVE THIS RECOVERY PHRASE !!!');
    console.log('='.repeat(50));
    console.log(mnemonic);
    console.log('='.repeat(50));
    console.log('\nThis is your ONLY backup. Lose it = lose funds.\n');
    
    await ask('Press Enter after saving your phrase...');
  }

  // Get Telegram bot token (optional)
  console.log('\n--- Notifications (optional) ---');
  const useTelegram = await ask('Set up Telegram alerts? (y/n): ');
  let telegramToken = '', telegramChat = '';
  
  if (useTelegram.toLowerCase() === 'y') {
    telegramToken = await ask('Telegram Bot Token: ');
    telegramChat = await ask('Telegram Chat ID: ');
  }

  // Create .env file
  const envContent = `# Simmer Weather Trader Config
WALLET_PASS="${password}"
POLYGON_RPC="https://polygon-rpc.com"
POLYGON_WS="wss://polygon-mainnet.g.alchemy.com/v2/demo"

# Thresholds (price in cents, 0.15 = 15%)
ENTRY_THRESHOLD=0.15
EXIT_THRESHOLD=0.45
MAX_POSITION=5
MAX_TRADES_PER_DAY=20
DAILY_LOSS_LIMIT=10

# Scan interval (ms)
SCAN_INTERVAL=120000

# Notifications
TELEGRAM_BOT="${telegramToken}"
TELEGRAM_CHAT="${telegramChat}"

# Safety
dryRun=true
`;

  fs.writeFileSync(ENV_FILE, envContent);
  console.log('\nConfig saved to:', ENV_FILE);

  // Create user config
  const userConfig = {
    wallet: { address },
    arbitrage: {
      entryThreshold: 0.15,
      exitThreshold: 0.45,
      locations: ['NYC', 'Chicago', 'Seattle', 'Atlanta', 'Dallas'],
      maxPosition: 5,
      maxTradesPerRun: 5,
      maxTradesPerDay: 20,
      dailyLossLimit: 10
    },
    notifications: {
      telegram: useTelegram.toLowerCase() === 'y',
      telegramChat
    }
  };

  fs.writeFileSync(
    path.join(CONFIG_DIR, 'config.json'),
    JSON.stringify(userConfig, null, 2)
  );

  console.log('\n=== Setup Complete ===');
  console.log('Wallet address:', address);
  console.log('\nNext steps:');
  console.log('1. Fund your wallet with USDC on Polygon');
  console.log('2. Run: npm run balance');
  console.log('3. Run: npm run scan (dry run)');
  console.log('4. Edit .env: set dryRun=false to enable live trading');
  console.log('\nBridge guide: bridge-guide.md');

  rl.close();
}

setup().catch(err => {
  console.error('Setup error:', err);
  rl.close();
  process.exit(1);
});
