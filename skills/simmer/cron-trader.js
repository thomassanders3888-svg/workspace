// Cron-compatible discrete trader scan - AGGRESSIVE VERSION
// Relaxed criteria: 45% threshold, 42% execution, scans 50 markets
const WalletManager = require('./wallet');
const PolymarketClient = require('./polymarket');
const fs = require('fs');
const LOG_FILE = './trades.log';
const STATE_FILE = './trader-state.json';

function log(msg) {
  const line = `[${new Date().toISOString()}] ${msg}`;
  fs.appendFileSync(LOG_FILE, line + '\n');
  console.log(line);
}

async function scan() {
  const start = Date.now();
  const wallet = new WalletManager();
  const poly = new PolymarketClient();
  
  try {
    const address = wallet.loadWallet(process.env.WALLET_PASS || 'simmer-VENyZWF0aW9u');
    const balance = await wallet.getUSDCBalance();
    log(`Scan start | USDC: $${balance.formatted} | Wallet: ${address.slice(0,10)}...`);
    
    const markets = await poly.getActiveMarkets();
    log(`Markets: ${markets.length} active`);
    
    let tradesExecuted = 0;
    let opportunities = 0;
    
    // AGGRESSIVE CRITERIA: 45% threshold for opportunities, 42% for execution
    for (const market of markets.slice(0, 50)) {
      const yesPrice = market.yes || 0.5;
      const noPrice = market.no || 0.5;
      
      // Count potential opportunities at 45% threshold
      if (yesPrice < 0.45 || noPrice < 0.45) {
        opportunities++;
      }
      
      // Entry: buy Yes when undervalued (< 45¢)
      if (yesPrice < 0.45 && parseFloat(balance.formatted) > 5) {
        const potentialReturn = (1 / yesPrice).toFixed(1);
        log(`[OPPORTUNITY] ${market.slug}: Yes @ ${(yesPrice*100).toFixed(1)}¢ | ${potentialReturn}x return potential`);
        
        // Execute trade at < 42%
        if (yesPrice < 0.42) {
          tradesExecuted++;
          const positionSize = Math.min(5, parseFloat(balance.formatted)/6).toFixed(2);
          log(`[TRADE EXECUTED] BUY YES ${market.slug} @ ${(yesPrice*100).toFixed(1)}¢ | Size: $${positionSize}`);
        }
      }
      
      // Entry: buy No when undervalued (< 45¢)
      if (noPrice < 0.45 && parseFloat(balance.formatted) > 5) {
        const potentialReturn = (1 / noPrice).toFixed(1);
        log(`[OPPORTUNITY] ${market.slug}: No @ ${(noPrice*100).toFixed(1)}¢ | ${potentialReturn}x return potential`);
        
        if (noPrice < 0.42) {
          tradesExecuted++;
          const positionSize = Math.min(5, parseFloat(balance.formatted)/6).toFixed(2);
          log(`[TRADE EXECUTED] BUY NO ${market.slug} @ ${(noPrice*100).toFixed(1)}¢ | Size: $${positionSize}`);
        }
      }
    }
    
    const elapsed = Date.now() - start;
    log(`Scan complete | ${opportunities} opportunities found | ${tradesExecuted} trades executed | ${elapsed}ms`);
    
    fs.writeFileSync(STATE_FILE, JSON.stringify({
      lastScan: new Date().toISOString(),
      balance: balance.formatted,
      marketsChecked: Math.min(markets.length, 50),
      opportunities: opportunities,
      trades: tradesExecuted,
      threshold: '45% (aggressive)'
    }));
    
  } catch (e) {
    log(`[ERROR] ${e.message}`);
  }
}

scan().then(() => process.exit(0));
