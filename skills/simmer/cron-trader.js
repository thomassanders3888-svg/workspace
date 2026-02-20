// Weather Arbitrage Bot - LIVE VERSION with Real Wallet Integration
// Fetches real Polymarket data and executes trades using actual wallet
const axios = require('axios');
const PolymarketClient = require('./polymarket');
const SimmerWallet = require('./wallet');
const fs = require('fs');
const path = require('path');

const LOG_FILE = path.join(__dirname, 'trades.log');
const STATE_FILE = path.join(__dirname, 'trader-state.json');

class WeatherArbitrage {
  constructor(config = {}) {
    this.config = {
      entryThreshold: 0.15,      // Buy YES below 15¢ (6.7x upside minimum)
      exitThreshold: 0.60,       // Sell above 60¢
      maxPosition: 5,            // Max $5 per trade
      maxDailyTrades: 5,         // Conservative limit
      minBalance: 1,             // Leave $1 buffer
      ...config
    };
    this.polymarket = new PolymarketClient();
    this.wallet = new SimmerWallet();
    this.balance = 0;
    this.todayTrades = 0;
    this.loadState();
  }

  loadState() {
    try {
      if (fs.existsSync(STATE_FILE)) {
        const state = JSON.parse(fs.readFileSync(STATE_FILE, 'utf8'));
        this.todayTrades = state.todayTrades || 0;
        this.lastDate = state.lastDate || new Date().toDateString();
        // Reset daily counter if new day
        if (this.lastDate !== new Date().toDateString()) {
          this.todayTrades = 0;
        }
      }
    } catch (e) {
      this.todayTrades = 0;
    }
  }

  saveState() {
    fs.writeFileSync(STATE_FILE, JSON.stringify({
      todayTrades: this.todayTrades,
      lastDate: new Date().toDateString(),
      lastScan: new Date().toISOString()
    }));
  }

  log(message) {
    const line = `[${new Date().toISOString()}] ${message}`;
    fs.appendFileSync(LOG_FILE, line + '\n');
    console.log(line);
  }

  async initialize() {
    // Load wallet
    try {
      this.wallet.loadWallet('simmer-VENyZWF0aW9u');
      this.address = this.wallet.getAddress();
    } catch (e) {
      throw new Error(`Wallet failed to load: ${e.message}`);
    }

    // Get REAL balance from blockchain
    const bal = await this.wallet.getUSDCBalance('polygon');
    this.balance = parseFloat(bal.formatted);
    
    this.log(`[INIT] Wallet: ${this.address}`);
    this.log(`[INIT] USDC: $${this.balance.toFixed(2)}`);
    this.log(`[INIT] Today: ${this.todayTrades}/${this.config.maxDailyTrades} trades used`);
  }

  async scanAndTrade() {
    await this.initialize();
    
    if (this.balance < this.config.minBalance) {
      this.log(`[SKIP] Insufficient funds ($${this.balance.toFixed(2)} < $${this.config.minBalance})`);
      return { status: 'NO_FUNDS', balance: this.balance };
    }

    const startTime = Date.now();
    const markets = await this.polymarket.getActiveMarkets();
    const candidates = [];
    const executed = [];

    // Scan for opportunities across ALL categories (not just weather)
    for (const market of markets.slice(0, 30)) {
      const yesPrice = this.parseYesPrice(market);
      
      // Skip if price is null or invalid
      if (!yesPrice || yesPrice <= 0 || yesPrice >= 1) continue;
      
      // Skip already expensive positions
      if (yesPrice >= this.config.entryThreshold) continue;

      candidates.push({
        id: market.id || market.slug,
        question: market.question || market.title,
        price: yesPrice,
        upside: (1 / yesPrice).toFixed(1),
        category: this.categorize(market)
      });
    }

    // Sort by best upside (lowest price)
    candidates.sort((a, b) => a.price - b.price);

    // Log scan summary
    this.log(`[SCAN] Found ${candidates.length} opportunities under ${(this.config.entryThreshold * 100).toFixed(0)}¢`);

    // Execute top opportunities (if we have budget and daily limit)
    for (const opp of candidates.slice(0, this.config.maxDailyTrades - this.todayTrades)) {
      if (this.balance < this.config.minBalance + this.config.maxPosition) {
        this.log(`[HALT] Low balance ($${this.balance.toFixed(2)}), stopping`);
        break;
      }

      const tradeSize = Math.min(this.config.maxPosition, this.balance - this.config.minBalance);
      
      // ACTUAL TRADE EXECUTION
      const result = await this.executeTrade(opp, tradeSize);
      
      if (result.success) {
        executed.push(result);
        this.balance -= tradeSize;
        this.todayTrades++;
        this.log(`[TRADE] ✅ Executed: $${tradeSize.toFixed(2)} @ ${(opp.price * 100).toFixed(1)}¢ | Upside: ${opp.upside}x`);
      } else {
        this.log(`[TRADE] ❌ Failed: ${result.error}`);
      }
    }

    this.saveState();

    return {
      status: executed.length > 0 ? 'TRADED' : 'SCANNED',
      scanned: markets.length,
      opportunities: candidates.length,
      executed: executed.length,
      balance: this.balance,
      trades: executed
    };
  }

  parseYesPrice(market) {
    try {
      // Try outcomePrices first
      if (market.outcomePrices) {
        const prices = JSON.parse(market.outcomePrices);
        if (Array.isArray(prices) && prices.length >= 2) {
          return parseFloat(prices[0]);
        }
      }
      // Fallback to bestBid
      if (market.bestBid) return parseFloat(market.bestBid);
      if (market.yes) return parseFloat(market.yes);
    } catch (e) {}
    return null;
  }

  categorize(market) {
    const text = ((market.question || market.title || '') + ' ' + (market.description || '')).toLowerCase();
    if (text.includes('weather') || text.includes('temp') || text.includes('rain')) return 'weather';
    if (text.includes('btc') || text.includes('eth') || text.includes('crypto') || text.includes('bitcoin')) return 'crypto';
    if (text.includes('trump') || text.includes('biden') || text.includes('election') || text.includes('president')) return 'politics';
    return 'other';
  }

  async executeTrade(opportunity, size) {
    // CRITICAL: This is where real Polymarket order execution would happen
    // Currently placeholder - requires Simmer SDK or PolyMarket CTF integration
    
    // For now, simulate successful execution but log clearly
    this.log(`[ORDER] Would place: BUY YES $${size} on "${opportunity.question?.substring(0, 40)}..."`);
    
    // TODO: Implement actual order via:
    // 1. Simmer SDK: simmer.placeOrder(marketId, side, size)
    // 2. Or direct CTF: interacting with ConditionalTokens contract
    // 3. Or API: POST to /orders endpoint with signed order data
    
    // Check if we have actual trading capability
    const hasTradingCapability = false; // Set to true when ready
    
    if (!hasTradingCapability) {
      return {
        success: false,
        error: 'SIMULATION_MODE: Trading requires Simmer SDK integration or manual order signing',
        opportunity,
        size
      };
    }

    // Real execution would go here...
    return { success: true, opportunity, size, txHash: null };
  }
}

// Main execution
async function main() {
  const trader = new WeatherArbitrage({
    entryThreshold: 0.15,    // 15¢ = 6.7x upside
    maxPosition: 5,         // $5 per trade
    maxDailyTrades: 3       // Conservative
  });

  try {
    const result = await trader.scanAndTrade();
    
    const now = new Date().toLocaleTimeString('en-US', { hour12: false, hour: '2-digit', minute: '2-digit' });
    
    if (result.status === 'NO_FUNDS') {
      console.log(`[${now}] 💸 No funds | $${result.balance.toFixed(2)} USDC`);
    } else if (result.executed > 0) {
      console.log(`[${now}] ✅ ${result.executed} trade(s) | $${result.balance.toFixed(2)} remaining`);
    } else if (result.opportunities > 0) {
      console.log(`[${now}] 👀 ${result.opportunities} opportunities | $${result.balance.toFixed(2)} | No trades`);
    } else {
      console.log(`[${now}] ⏸️