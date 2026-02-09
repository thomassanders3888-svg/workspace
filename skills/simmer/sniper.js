// Cheap Sniper Bot - Buy < $0.05 outcomes
const PolymarketClient = require('./polymarket');

class CheapSniper {
  constructor(config = {}) {
    this.config = {
      maxPrice: config.maxPrice || 0.05,
      maxPosition: config.maxPosition || 3,
      maxMarkets: config.maxMarkets || 50,
      sellThreshold: config.sellThreshold || 0.15,
      dailyLimit: config.dailyLimit || 10,
      ...config
    };
    
    this.polymarket = new PolymarketClient();
    this.positions = new Map();
    this.trades = 0;
    this.dailyPnL = 0;
  }

  async scanAllMarkets() {
    try {
      const markets = await this.polymarket.getMarkets({ limit: 200 });
      return Array.isArray(markets) ? markets.filter(m => m.active !== false && !m.closed) : [];
    } catch (e) {
      console.error('Scan error:', e.message);
      return [];
    }
  }

  filterCheap(markets) {
    const cheap = [];
    for (const market of markets) {
      const outcomes = market.outcomes || [];
      for (const outcome of outcomes) {
        const price = parseFloat(outcome.price || 0);
        if (price > 0 && price <= this.config.maxPrice) {
          cheap.push({
            marketId: market.id,
            outcomeId: outcome.id,
            title: market.title,
            outcome: outcome.name,
            price: price,
            potentialReturn: (1 / price)
          });
        }
      }
    }
    return cheap.sort((a, b) => a.price - b.price).slice(0, this.config.maxMarkets);
  }

  async buyCheapOutcomes(wallet, opportunities) {
    const buys = [];
    for (const opp of opportunities) {
      if (this.trades >= this.config.dailyLimit) break;
      if (this.positions.has(opp.outcomeId)) continue;
      
      try {
        console.log(`Buying ${opp.outcome} @ $${opp.price.toFixed(3)}`);
        const position = {
          id: opp.outcomeId,
          entryPrice: opp.price,
          amount: this.config.maxPosition,
          market: opp.title,
          outcome: opp.outcome,
          timestamp: Date.now()
        };
        
        this.positions.set(opp.outcomeId, position);
        this.trades++;
        buys.push(position);
      } catch (e) {
        console.error('Buy failed:', e.message);
      }
    }
    return buys;
  }

  async monitorPositions() {
    const sells = [];
    for (const [id, pos] of this.positions) {
      try {
        const current = await this.polymarket.getPrice(id);
        if (!current || !current.mid) continue;
        
        const profit = (current.mid - pos.entryPrice) * pos.amount;
        
        if (current.mid >= this.config.sellThreshold) {
          console.log(`Selling ${pos.outcome} @ $${current.mid.toFixed(3)} (profit: $${profit.toFixed(2)})`);
          this.dailyPnL += profit;
          sells.push({ ...pos, exitPrice: current.mid, profit });
          this.positions.delete(id);
        }
      } catch (e) {
        console.error('Monitor error:', e.message);
      }
    }
    return sells;
  }

  getStatus() {
    return {
      positions: this.positions.size,
      trades: this.trades,
      dailyPnL: this.dailyPnL,
      cheapFound: 0
    };
  }
}

module.exports = CheapSniper;
