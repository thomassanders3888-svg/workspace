#!/usr/bin/env node
// Simple bot starter

const wallet = new (require('./wallet'))();
const bot = new (require('./arbitrage'))({ dryRun: true });

const pass = 'simmer-VENyZWF0aW9u';

console.log('🤖 SIMMER BOT STARTER');
console.log('=====================\n');

try {
  const addr = wallet.loadWallet(pass);
  console.log('✓ Wallet:', addr.slice(0,10)+'...'+addr.slice(-8));
  
  bot.start(wallet);
  console.log('✓ Bot running in DRY-RUN mode');
  console.log('  - Scans weather markets');
  console.log('  - Finds opportunities');
  console.log('  - NO REAL TRADES (dryRun=true)\n');
  
  setInterval(() => {
    const s = bot.getStatus();
    console.log(`[${new Date().toLocaleTimeString()}] Trades: ${s.trades}, Running: ${s.running}`);
  }, 120000);
  
} catch(e) {
  console.log('✗ Failed:', e.message);
  process.exit(1);
}
