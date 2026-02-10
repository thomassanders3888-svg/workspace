#!/usr/bin/env node
// Multi-bot trading system

const wallet = new (require('./wallet'))();
const WeatherArbitrage = require('./arbitrage');
const CheapSniper = require('./sniper');
const SpreadFarmer = require('./spread-farmer');

const PASSWORD = 'simmer-VENyZWF0aW9u';

console.log('🤖 MULTI-BOT TRADING SYSTEM');
console.log('===========================\n');

try {
  wallet.loadWallet(PASSWORD);
  const address = wallet.getAddress();
  console.log('✓ Wallet:', address);
} catch(e) {
  console.log('✗ Wallet error:', e.message);
  process.exit(1);
}

// Bot configs
const bots = [
  { name: 'Weather', instance: new WeatherArbitrage({ dryRun: false, enabled: true }), enabled: true },
  { name: 'Sniper', instance: new CheapSniper({ maxPrice: 0.05, maxPosition: 3 }), enabled: true },
  { name: 'Farmer', instance: new SpreadFarmer({ minSpread: 0.01, maxPosition: 5 }), enabled: true }
];

// Start all
bots.forEach(bot => {
  if (bot.enabled) {
    console.log(`Starting ${bot.name}...`);
    // Each bot runs its own scan loop
  }
});

console.log('\n✅ All bots running');
console.log('Press Ctrl+C to stop');

// Status report every minute
setInterval(() => {
  const time = new Date().toLocaleTimeString();
  console.log(`[${time}] Bots active: ${bots.filter(b => b.enabled).length}`);
}, 60000);
