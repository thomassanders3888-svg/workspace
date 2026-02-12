#!/usr/bin/env node
// Multi-chain trader with Ethereum + Polygon support
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
const WalletManager = require('./wallet');
const PolymarketClient = require('./polymarket');
const PASSWORD = 'simmer-VENyZWF0aW9u';
const LOG_FILE = path.join(__dirname, 'trades.log');

let firstTradeNotified = false;

class FixedTrader {
  constructor() {
    this.wallet = new WalletManager();
    this.polymarket = new PolymarketClient();
    this.config = {
      entryThreshold: 0.15,
      exitThreshold: 0.45,
      sniperThreshold: 0.05,
      minSpread: 0.02,
      maxPosition: 5,
      maxDailyLoss: 15,
      maxTradesPerDay: 15,
      dryRun: false,
      chain: 'polygon'
    };
    this.tradeCount = 0;
    this.dailyLoss = 0;
    this.positions = new Map();
    this.running = false;
    this.walletBalance = 0;
    this.chainBalances = {};
  }

  log(msg) {
    const ts = new Date().toISOString();
    const line = `[${ts}] ${msg}`;
    console.log(line);
    fs.appendFileSync(LOG_FILE, line + '\n');
  }

  notify(side, size, price, marketId) {
    try {
      const cmd = `openclaw message send --target "+19402304961" --message "🚀 Trade executed! ${side} $${size} @ $${price.toFixed(2)} — Market: ${marketId}"`;
      execSync(cmd, { stdio: 'ignore' });
    } catch (e) {
      // Silent fail for notifications
    }
  }

  async init() {
    try {
      this.wallet.loadWallet(PASSWORD);
      const address = this.wallet.getAddress();
      this.log(`[INIT] Wallet: ${address}`);
      this.log(`[INIT] Checking all chains for USDC...`);
      const allBalances = await this.wallet.checkAllChainsUSDC();
      for (const [chain, bal] of Object.entries(allBalances)) {
        const formatted = parseFloat(bal.formatted).toFixed(2);
        this.chainBalances[chain] = parseFloat(bal.formatted) || 0;
        this.log(`[INIT] ${chain.toUpperCase()}: $${formatted} USDC`);
      }
      const totalUSDC = Object.values(this.chainBalances).reduce((a, b) => a + b, 0);
      this.walletBalance = this.chainBalances.polygon;
      this.log(`[INIT] Total across all chains: $${totalUSDC.toFixed(2)}`);
      this.log(`[INIT] Tradeable (Polygon): $${this.walletBalance.toFixed(2)}`);
      for (const chain of ['ethereum', 'polygon']) {
        const native = await this.wallet.getNativeBalance(chain);
        this.log(`[INIT] ${chain.toUpperCase()} gas (${native.token}): ${parseFloat(native.formatted).toFixed(4)}`);
      }
      if (this.chainBalances.ethereum > 0 && this.chainBalances.polygon === 0) {
        this.log(`[WARN] USD FOUND ON ETHEREUM! You have $${this.chainBalances.ethereum.toFixed(2)} on Ethereum`);
        this.log(`[WARN] Need to bridge to Polygon for Polymarket trading`);
      }
      this.log(`[INIT] Chain: ${this.config.chain.toUpperCase()}`);
      this.log(`[INIT] Dry Run: ${this.config.dryRun}`);
      this.log(`[INIT] ${this.config.dryRun ? 'SIMULATED' : 'LIVE'} MODE`);
      this.log('='.repeat(50));
      return true;
    } catch (e) {
      this.log(`[INIT ERROR] ${e.message}`);
      return false;
    }
  }

  async executeTrade(marketId, side, size, price) {
    if (side === 'BUY' && this.walletBalance < size) {
      this.log(`[SKIP] Insufficient funds: $${this.walletBalance.toFixed(2)} < $${size}`);
      return { success: false, error: 'insufficient funds' };
    }
    if (this.config.dryRun) {
      this.log(`[DRY RUN] ${side} $${size} @ $${price.toFixed(2)}`);
      return { success: true, dryRun: true };
    }
    try {
      this.log(`[EXEC] ${side} $${size} @ $${price.toFixed(2)} on ${marketId}`);
      const tradeData = { marketId, side, size, price, timestamp: Date.now() };
      const order = await this.wallet.signOrder(tradeData);
      this.log(`[SIGN] Order signed: ${order.signature?.substring(0, 30)}...`);
      this.tradeCount++;
      if (side === 'BUY') this.walletBalance -= size;
      this.positions.set(marketId, { side, size, price });
      if (!firstTradeNotified) {
        this.notify(side, size, price, marketId);
        firstTradeNotified = true;
      }
      this.log(`[EXEC] Trade complete. Balance: $${this.walletBalance.toFixed(2)}`);
      return { success: true };
    } catch (e) {
      this.log(`[EXEC ERROR] ${e.message}`);
      return { success: false, error: e.message };
    }
  }

  async runOnce() {
    if (!this.running) return;
    this.log(`--- SCAN #${this.tradeCount + 1} ---`);
    this.log(`Balance: $${this.walletBalance.toFixed(2)} USDC`);
    if (this.walletBalance < 1) {
      this.log(`[WAIT] Insufficient funds. Need $1+ to trade.`);
      return;
    }

    // Fetch ALL market types: Weather, Crypto, Politics
    let allMarkets = [];
    let marketStats = { weather: 0, crypto: 0, politics: 0, other: 0 };
    
    try {
      const [weatherMarkets, cryptoMarkets, politicsMarkets] = await Promise.all([
        this.polymarket.getWeatherMarkets(),
        this.polymarket.getCryptoMarkets(),
        this.polymarket.getPoliticsMarkets()
      ]);
      
      marketStats.weather = weatherMarkets.length;
      marketStats.crypto = cryptoMarkets.length;
      marketStats.politics = politicsMarkets.length;
      
      // Combine all markets and deduplicate by ID
      const seenIds = new Set();
      for (const m of [...weatherMarkets, ...cryptoMarkets, ...politicsMarkets]) {
        if (!seenIds.has(m.id)) {
          seenIds.add(m.id);
          allMarkets.push(m);
        }
      }
      
      marketStats.other = allMarkets.length - (weatherMarkets.length + cryptoMarkets.length + politicsMarkets.length);
      this.log(`Markets: ${allMarkets.length} total | Weather: ${marketStats.weather} | Crypto: ${marketStats.crypto} | Politics: ${marketStats.politics}`);
    } catch (e) {
      this.log(`[API ERROR] ${e.message}`);
      allMarkets = [];
    }
    this.log(`Markets: ${allMarkets.length}`);
    for (const m of allMarkets) {
      if (m.closed) continue; // Skip closed markets

      // Extract price: try outcomePrices JSON, then bestBid, then lastTradePrice
      let price = 0;
      try {
        const outcomes = JSON.parse(m.outcomePrices);
        price = parseFloat(outcomes[0]) || 0; // YES price
      } catch (e) {
        price = parseFloat(m.bestBid || m.lastTradePrice || 0);
      }

      if (price === 0) continue;

      if (price < this.config.entryThreshold) {
        this.log(`[ENTRY] ${m.question || m.id}: Yes @ ${(price * 100).toFixed(1)}¢`);
        await this.executeTrade(m.id, 'BUY', 1, price);
      }
      if (price > this.config.exitThreshold && this.positions.has(m.id)) {
        this.log(`[EXIT] ${m.question || m.id}: Yes @ ${(price * 100).toFixed(1)}¢`);
        await this.executeTrade(m.id, 'SELL', 1, price);
        this.positions.delete(m.id);
      }
    }
    this.log(`Status: ${this.tradeCount} trades | ${this.positions.size} positions`);
  }

  start() {
    if (this.running) return;
    this.running = true;
    this.log(`🚀 TRADER STARTED`);
    this.runOnce();
    this.interval = setInterval(() => this.runOnce(), 2 * 60 * 1000);
  }

  stop() {
    this.running = false;
    if (this.interval) clearInterval(this.interval);
    this.log(`🛑 TRADER STOPPED`);
  }
}

const trader = new FixedTrader();
trader.init().then(ok => {
  if (ok) trader.start();
  else process.exit(1);
});

process.on('SIGINT', () => {
  trader.stop();
  process.exit(0);
});
