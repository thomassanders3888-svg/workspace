// Cron-compatible discrete trader scan
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
    for (const market of markets.slice(0, 10)) {
      const yesPrice = market.yes || 0.5;
      const noPrice = market.no || 0.5;
      
      // Entry: buy Yes < 15¢
      if (yesPrice < 0.15 && parseFloat(balance.formatted) > 5) {
        log(`[ENTRY] ${market.slug}: Yes @ ${(yesPrice*100).toFixed(1)}¢`);
        tradesExecuted++;
      }
      // Entry: buy No < 15¢  
      if (noPrice < 0.15 && parseFloat(balance.formatted) > 5) {
        log(`[ENTRY] ${market.slug}: No @ ${(noPrice*100).toFixed(1)}¢`);
        tradesExecuted++;
      }
    }
    
    const elapsed = Date.now() - start;
    log(`Scan complete | ${tradesExecuted} trades | ${elapsed}ms`);
    
    fs.writeFileSync(STATE_FILE, JSON.stringify({
      lastScan: new Date().toISOString(),
      balance: balance.formatted,
      marketsChecked: markets.length,
      trades: tradesExecuted
    }));
    
  } catch (e) {
    log(`[ERROR] ${e.message}`);
  }
}

scan().then(() => process.exit(0));
