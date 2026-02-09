// Spread Farmer - Buy bid, sell ask
const PolymarketClient = require('./polymarket');

class SpreadFarmer {
  constructor(config = {}) {
    this.config = {
      minSpread: config.minSpread || 0.01,
      maxPosition: config.maxPosition || 5,
      maxFlips: config.maxFlips || 100,
      fee: config.fee || 0.002,
      ...config
    };
    
    this.polymarket = new PolymarketClient();
    this.flips = 0;
    this.profit = 0;
    this.attempts = 0;
  }

  async findSpreads() {
    const markets = await this.polymarket.getMarkets({ limit: 100 });
    const spreads = [];
    
    for (const market of markets) {
      if (!market.active || market.closed) continue;
      
      const outcomes = market.outcomes || [];
      for (const outcome of outcomes) {
        try {
          const orderbook = await this.polymarket.getOrderBook(outcome.id);
          if (!orderbook || !orderbook.bids || !orderbook.asks) continue;
          
          const bestBid = parseFloat(orderbook.bids[0]?.price || 0);
          const bestAsk = parseFloat(orderbook.asks[0]?.price || 0);
          
          if (bestBid <= 0 || bestAsk <= 0) continue;
          
          const spread = bestAsk - bestBid;
          const spreadPct = spread / bestBid;
          
          if (spread >= this.config.minSpread) {
            spreads.push({
              marketId: market.id,
              outcomeId: outcome.id,
              title: market.title,
              outcome: outcome.name,
              bid: bestBid,
              ask: bestAsk,
              spread,
              spreadPct,
              potentialProfit: (this.config.maxPosition * (spread - 2 * this.config.fee))
            });
          }
        } catch (e) {
          continue;
        }
      }
    }
    
    return spreads.sort((a, b) => b.spread - a.spread);
  }

  async executeFlip(wallet, spread) {
    try {
      console.log(`Flip ${spread.title}: Buy $${spread.bid.toFixed(3)} → Sell $${spread.ask.toFixed(3)}`);
      
      // Buy at bid
      const buyPrice = spread.bid;
      console.log(`Buying ${spread.outcome} @ $${buyPrice}`);
      
      // Sell at ask (simulated for dry-run)
      const sellPrice = spread.ask;
      const gross = sellPrice - buyPrice;
      const fees = (buyPrice + sellPrice) * this.config.fee;
      const netProfit = (gross - fees) * this.config.maxPosition;
      
      this.flips++;
      this.profit += netProfit;
      
      return {
        success: true,
        market: spread.title,
        outcome: spread.outcome,
        buy: buyPrice,
        sell: sellPrice,
        profit: netProfit
      };
    } catch (e) {
      return { success: false, error: e.message };
    }
  }

  async batchFlips(wallet, spreads) {
    const results = [];
    for (const spread of spreads.slice(0, this.config.maxFlips)) {
      if (this.flips >= this.config.maxFlips) break;
      
      const result = await this.executeFlip(wallet, spread);
      results.push(result);
      
      if (result.success) {
        await new Promise(r => setTimeout(r, 500)); // Rate limit
      }
    }
    return results;
  }

  trackPnL() {
    return {
      flips: this.flips,
      profit: this.profit,
      avgProfit: this.flips > 0 ? this.profit / this.flips : 0,
      attempts: this.attempts
    };
  }
}

module.exports = SpreadFarmer;
