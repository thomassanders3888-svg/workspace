// Weather Arbitrage Bot v2 - Enhanced Strategy
const fs = require('fs');
const path = require('path');
const axios = require('axios');
const PolymarketClient = require('./polymarket');

class WeatherArbitrageV2 {
  constructor() {
    // Load env
    const envPath = path.join(process.env.HOME, '.simmer', '.env');
    if (fs.existsSync(envPath)) {
      const env = fs.readFileSync(envPath, 'utf8');
      env.split('\n').forEach(line => {
        const match = line.match(/^([A-Z_]+)=(.+)$/);
        if (match) process.env[match[1]] = match[2].replace(/^["']|["']$/g, '');
      });
    }

    // v2 Config
    this.config = {
      entry: {
        aggressive: parseFloat(process.env.ENTRY_AGGRESSIVE || 0.12),
        standard: parseFloat(process.env.ENTRY_STANDARD || 0.15),
        conservative: parseFloat(process.env.ENTRY_CONSERVATIVE || 0.18)
      },
      exit: {
        tier1: parseFloat(process.env.EXIT_TIER1 || 0.38),
        tier2: parseFloat(process.env.EXIT_TIER2 || 0.45),
        tier3: parseFloat(process.env.EXIT_TIER3 || 0.55),
        moon: parseFloat(process.env.EXIT_MOON || 0.70)
      },
      position: {
        high: parseInt(process.env.POSITION_HIGH || 8),
        med: parseInt(process.env.POSITION_MED || 5),
        low: parseInt(process.env.POSITION_LOW || 2)
      },
      limits: {
        maxPosition: parseInt(process.env.MAX_POSITION || 8),
        maxTrades: parseInt(process.env.MAX_TRADES_PER_DAY || 15),
        dailyLoss: parseInt(process.env.DAILY_LOSS_LIMIT || 15),
        maxPositions: parseInt(process.env.POSITION_COUNT_LIMIT || 3)
      },
      circuit: {
        losses: parseInt(process.env.CIRCUIT_LOSSES || 3),
        lossAmount: parseInt(process.env.CIRCUIT_LOSS_AMOUNT || 10)
      },
      scan: {
        normal: parseInt(process.env.SCAN_INTERVAL || 120000),
        rush: parseInt(process.env.SCAN_INTERVAL_RUSH || 60000),
        start: parseInt(process.env.ACTIVE_HOURS_START || 6),
        end: parseInt(process.env.ACTIVE_HOURS_END || 22)
      },
      filters: {
        minForecast: parseFloat(process.env.MIN_FORECAST_CONF || 0.65),
        minEdge: parseFloat(process.env.MIN_EDGE || 0.15)
      }
    };

    this.polymarket = new PolymarketClient();
    this.state = {
      positions: new Map(),
      trades: 0,
      pnl: 0,
      losses: 0,
      lastLoss: null,
      circuitBroken: false
    };
    this.intervalId = null;
  }

  getConfidenceLevel(forecastProb, edge) {
    if (edge > 0.30) return { level: 'high', size: this.config.position.high };
    if (edge > 0.15) return { level: 'medium', size: this.config.position.med };
    return { level: 'low', size: this.config.position.low };
  }

  getExitAction(currentPrice, entryPrice) {
    const gain = currentPrice - entryPrice;
    const gainPct = gain / entryPrice;

    if (currentPrice >= this.config.exit.moon) return { action: 'close_full', reason: 'Moon target hit' };
    if (currentPrice >= this.config.exit.tier3) return { action: 'close_75', reason: 'Tier 3 profit' };
    if (currentPrice >= this.config.exit.tier2) return { action: 'close_50', reason: 'Tier 2 profit' };
    if (currentPrice >= this.config.exit.tier1) return { action: 'close_25', reason: 'Tier 1 profit' };
    if (gainPct < -0.05) return { action: 'stop_loss', reason: '5% stop hit' };
    
    return { action: 'hold', reason: 'Within range' };
  }

  checkCircuitBreaker() {
    if (this.state.losses >= this.config.circuit.losses) {
      this.state.circuitBroken = true;
      setTimeout(() => {
        this.state.circuitBroken = false;
        this.state.losses = 0;
      }, 60 * 60 * 1000); // 1 hour
      return true;
    }
    return false;
  }

  getScanInterval() {
    const hour = new Date().getHours();
    return (hour >= this.config.scan.start && hour <= this.config.scan.end) 
      ? this.config.scan.rush 
      : this.config.scan.normal;
  }
}

module.exports = WeatherArbitrageV2;
