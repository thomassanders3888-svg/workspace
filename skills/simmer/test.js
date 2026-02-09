#!/usr/bin/env node
// Quick test script for Simmer bot

const WeatherArbitrage = require('./arbitrage');
const SimmerWallet = require('./wallet');

console.log('🧪 Simmer Bot Test Suite\n');

async function runTests() {
  let passed = 0;
  let failed = 0;
  
  // Test 1: Arbitrage bot creation
  try {
    console.log('Test 1: Create arbitrage bot...');
    const bot = new WeatherArbitrage({
      entryThreshold: 0.15,
      exitThreshold: 0.45,
      dryRun: true
    });
    console.log('✓ Bot created');
    passed++;
  } catch (e) {
    console.log('✗ Failed:', e.message);
    failed++;
  }
  
  // Test 2: NOAA API fetch
  try {
    console.log('\nTest 2: NOAA temp fetch (NYC)...');
    const bot = new WeatherArbitrage();
    const forecast = await bot.fetchNOAATemps('NYC');
    if (forecast && forecast.high.length > 0) {
      console.log('✓ NoAA fetch:', forecast.high.slice(0,3).join('°F, ') + '°F');
      passed++;
    } else {
      console.log('✗ No forecast data');
      failed++;
    }
  } catch (e) {
    console.log('✗ Failed:', e.message);
    failed++;
  }
  
  // Test 3: Polymarket connection
  try {
    console.log('\nTest 3: Polymarket API...');
    const bot = new WeatherArbitrage();
    const markets = await bot.findWeatherMarkets();
    const total = Object.values(markets).flat().length;
    console.log('✓ Found', total, 'weather markets');
    passed++;
  } catch (e) {
    console.log('✗ Failed:', e.message);
    failed++;
  }
  
  // Test 4: Dry run arbitrage
  try {
    console.log('\nTest 4: Dry run arbitrage scan...');
    const bot = new WeatherArbitrage();
    const result = await bot.executeArbitrage(null, true);
    console.log('✓ Scan complete:', result.opportunitiesFound, 'opportunities');
    passed++;
  } catch (e) {
    console.log('✗ Failed:', e.message);
    failed++;
  }
  
  // Summary
  console.log('\n' + '='.repeat(30));
  console.log(`Tests: ${passed} passed, ${failed} failed`);
  
  if (passed === 4) {
    console.log('\n✅ All tests passed!');
    console.log('\nNext steps:');
    console.log('1. Run: node setup.js');
    console.log('2. Fund wallet with USDC');
    console.log('3. Run: npm start');
  } else {
    console.log('\n⚠️  Some tests failed. Check dependencies.');
  }
}

runTests().catch(err => {
  console.error('\nTest suite error:', err);
  process.exit(1);
});
