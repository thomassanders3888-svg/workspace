#!/usr/bin/env node
// Run the weather arbitrage bot

const fs = require('fs');
const path = require('path');

// Load .env
const ENV_FILE = path.join(process.env.HOME, '.simmer', '.env');
if (fs.existsSync(ENV_FILE)) {
  const env = fs.readFileSync(ENV_FILE, 'utf8');
  env.split('\n').forEach(line => {
    const match = line.match(/^([A-Z_]+)=(.+)$/);
    if (match) {
      process.env[match[1]] = match[2].replace(/^["']|["']$/g, '');
    }
  });
}

const SimmerWallet = require('./wallet');
const WeatherArbitrage = require('./arbitrage');

console.log('╔════════════════════════════════════════════════╗');
console.log('║     Simmer Weather Arbitrage Bot v1.0          ║');
console.log('╚════════════════════════════════════════════════╝');
console.log();

// Load wallet
const wallet = new SimmerWallet();
const password = process.env.WALLET_PASS || 'demo';
let address;

try {
  if (!wallet.walletExists()) {
    console.log('❌ No wallet found!');
    console.log('Run: node setup.js');
    process.exit(1);
  }
  
  address = wallet.loadWallet(password);
  console.log('✓ Wallet loaded');
  console.log('  Address:', address.slice(0, 10) + '...' + address.slice(-8));
} catch (e) {
  console.log('❌ Wallet unlock failed:', e.message);
  process.exit(1);
}

// Check balances
console.log('\n📊 Balances:');
Promise.all([
  wallet.getUSDCBalance().catch(() => ({ formatted: '0' })),
  wallet.getNativeBalance().catch(() => ({ formatted: '0' }))
]).then(([usdc, matic]) => {
  console.log('  USDC:', usdc.formatted);
  console.log('  MATIC (gas):', matic.formatted);
  
  const usdcBalance = parseFloat(usdc.formatted);
  const maticBalance = parseFloat(matic.formatted);
  
  if (usdcBalance < 5) {
    console.log('\n⚠️  LOW USDC BALANCE');
    console.log('   Fund your wallet before trading');
    console.log('   Guide: bridge-guide.md');
    console.log();
  }
  
  if (maticBalance < 0.1) {
    console.log('⚠️  LOW MATIC (need ~0.5 for gas)');
    console.log();
  }
  
}).then(() => {
  // Setup arbitrage bot
  const isDryRun = (process.env.dryRun || 'true').toLowerCase() === 'true';
  
  console.log('⚙️  Configuration:');
  console.log('  Entry threshold: $' + (process.env.ENTRY_THRESHOLD || 0.15));
  console.log('  Exit threshold: $' + (process.env.EXIT_THRESHOLD || 0.45));
  console.log('  Mode:', isDryRun ? 'DRY RUN (no trades)' : 'LIVE TRADING ⚠️');
  console.log();
  
  if (!isDryRun) {
    console.log('╔════════════════════════════════════════════════╗');
    console.log('║  ⚠️  LIVE TRADING ENABLED ⚠️                   ║');
    console.log('║  Real money will be spent!                     ║');
    console.log('╚════════════════════════════════════════════════╝');
    console.log();
  }
  
  const bot = new WeatherArbitrage({
    entryThreshold: parseFloat(process.env.ENTRY_THRESHOLD || 0.15),
    exitThreshold: parseFloat(process.env.EXIT_THRESHOLD || 0.45),
    enabled: true,
    dryRun: isDryRun,
    scanInterval: parseInt(process.env.SCAN_INTERVAL || 120000)
  });
  
  // Initial scan
  console.log('🔍 Running initial scan...');
  bot.executeArbitrage(wallet, isDryRun).then(result => {
    console.log('\n📈 Results:');
    console.log('  Opportunities found:', result.opportunitiesFound);
    console.log('  Trades:', result.trades.length);
    
    if (result.trades.length > 0) {
      console.log('\n  Trade details:');
      result.trades.forEach((t, i) => {
        console.log(`    ${i+1}. ${t.action} ${t.marketId.slice(0, 16)}... @ $${t.price.toFixed(3)}`);
      });
    }
    
    console.log();
  }).then(() => {
    // Start continuous scanning
    if (process.argv.includes('--once')) {
      console.log('Single scan complete.');
      process.exit(0);
    }
    
    console.log('🤖 Starting continuous monitoring (⌃C to stop)...');
    console.log(`   Scan interval: ${process.env.SCAN_INTERVAL || 120000}ms`);
    console.log();
    
    bot.start(wallet);
    
    // Status report every 5 minutes
    setInterval(() => {
      const status = bot.getStatus();
      console.log(`[${new Date().toLocaleTimeString()}] Trades today: ${status.tradesToday}, Running: ${status.running}`);
    }, 300000);
  });
  
  // Graceful shutdown
  process.on('SIGINT', () => {
    console.log('\n\n👋 Stopping bot...');
    bot.stop();
    console.log('✓ Bot stopped');
    process.exit(0);
  });
  
}).catch(err => {
  console.error('Error:', err.message);
  process.exit(1);
});
