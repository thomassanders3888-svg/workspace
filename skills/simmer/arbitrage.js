// Weather Arbitrage Bot
// Buys < $0.15, sells > $0.45 based on NOAA vs Polymarket comparison

const axios = require('axios');
const PolymarketClient = require('./polymarket');

class WeatherArbitrage {
  constructor(config = {}) {
    this.config = {
      entryThreshold: 0.15,
      exitThreshold: 0.45,
      locations: ['NYC', 'Chicago', 'Seattle', 'Atlanta', 'Dallas'],
      maxPosition: 5,
      maxTradesPerRun: 5,
      enabled: true,
      trendDetection: true,
      scanInterval: 2 * 60 * 1000,
      ...config
    };
    
    this.polymarket = new PolymarketClient();
    this.positions = new Map();
    this.tradeCount = 0;
    this.intervalId = null;
    this.lastPrices = new Map();
  }

  // NOAA API: Get temperature forecast
  async fetchNOAATemps(location) {
    // Map locations to NOAA station IDs or lat/lon
    const locationMap = {
      'NYC': { lat: 40.7128, lon: -74.0060 },
      'Chicago': { lat: 41.8781, lon: -87.6298 },
      'Seattle': { lat: 47.6062, lon: -122.3321 },
      'Atlanta': { lat: 33.7490, lon: -84.3880 },
      'Dallas': { lat: 32.7767, lon: -96.7970 }
    };

    const coords = locationMap[location];
    if (!coords) return null;

    try {
      // NOAA API endpoint for grid forecast
      const response = await axios.get(
        `https://api.weather.gov/points/${coords.lat},${coords.lon}`,
        { timeout: 10000 }
      );
      
      const forecastUrl = response.data.properties.forecast;
      const forecastResponse = await axios.get(forecastUrl, { timeout: 10000 });
      
      const periods = forecastResponse.data.properties.periods.slice(0, 7);
      
      return {
        location,
        high: periods.map(p => p.temperature),
        low: periods.map(p => p.temperature),
        timestamps: periods.map(p => p.startTime),
        source: 'NOAA'
      };
    } catch (e) {
      console.error(`NOAA fetch failed for ${location}:`, e.message);
      return null;
    }
  }

  // Find all weather markets
  async findWeatherMarkets() {
    const allMarkets = await this.polymarket.getWeatherMarkets();
    const locationMarkets = {};

    for (const location of this.config.locations) {
      locationMarkets[location] = this.polymarket.findMarketsByLocation(
        allMarkets, location
      );
    }

    return locationMarkets;
  }

  // Get current Polymarket price for a market
  async getMarketPrice(market) {
    const tokenId = market.clob_token_ids?.[0];
    if (!tokenId) return null;
    
    return await this.polymarket.getPrice(tokenId);
  }

  // Compare NOAA forecast vs market probability
  compareForecastWithMarket(forecast, market, price) {
    const threshold = this.polymarket.parseTemperatureThreshold(market.title);
    if (!threshold) return null;

    // Calculate probability based on forecast
    const likely = forecast.high.some(t => t >= threshold);
    const probability = likely ? 0.7 : 0.3;

    return {
      marketId: market.id,
      title: market.title,
      marketPrice: price?.mid || 0.5,
      forecastProbability: probability,
      delta: probability - (price?.mid || 0.5),
      threshold,
      recommendation: this.getRecommendation(price?.mid, probability)
    };
  }

  // Determine buy/sell/hold
  getRecommendation(marketPrice, forecastProb) {
    if (!marketPrice) return 'HOLD';
    
    // Buy when market under-values the outcome
    if (marketPrice < this.config.entryThreshold && forecastProb > 0.5) {
      return 'BUY';
    }
    
    // Sell when market over-values
    if (marketPrice > this.config.exitThreshold && forecastProb < 0.5) {
      return 'SELL';
    }

    return 'HOLD';
  }

  // Find arbitrage opportunities
  async findOpportunities() {
    const opportunities = [];
    const marketMap = await this.findWeatherMarkets();

    for (const location of this.config.locations) {
      const forecast = await this.fetchNOAATemps(location);
      if (!forecast) continue;

      const markets = marketMap[location] || [];
      
      for (const market of markets) {
        try {
          const price = await this.getMarketPrice(market);
          const comparison = this.compareForecastWithMarket(forecast, market, price);
          
          if (comparison && comparison.recommendation !== 'HOLD') {
            opportunities.push(comparison);
          }
        } catch (e) {
          console.error(`Error analyzing ${market.id}:`, e.message);
        }
      }
    }

    // Sort by best delta
    return opportunities.sort((a, b) => Math.abs(b.delta) - Math.abs(a.delta));
  }

  // Execute arbitrage trades
  async executeArbitrage(wallet, dryRun = true) {
    if (!this.config.enabled) return { status: 'disabled' };

    const opportunities = await this.findOpportunities();
    const trades = [];

    for (const opp of opportunities.slice(0, this.config.maxTradesPerRun)) {
      if (this.tradeCount >= this.config.maxTradesPerRun) break;

      // Trend detection: only trade if delta is significant
      if (this.config.trendDetection && Math.abs(opp.delta) < 0.2) continue;

      const trade = {
        marketId: opp.marketId,
        action: opp.recommendation,
        price: opp.marketPrice,
        amount: this.config.maxPosition,
        timestamp: new Date().toISOString(),
        executed: !dryRun
      };

      if (!dryRun) {
        // Actual execution would go here
        // await this.placeTrade(wallet, opp.marketId, opp.recommendation);
        this.tradeCount++;
        this.positions.set(opp.marketId, trade);
      }

      trades.push(trade);
    }

    return {
      timestamp: new Date().toISOString(),
      opportunitiesFound: opportunities.length,
      trades: trades,
      dryRun
    };
  }

  // Start automated scanning
  start(wallet) {
    if (this.intervalId) return false;

    console.log(`Starting weather arbitrage bot (scan interval: ${this.config.scanInterval/1000}s)`);

    this.intervalId = setInterval(async () => {
      try {
        const result = await this.executeArbitrage(wallet, true);
        console.log(`Scan complete: ${result.opportunitiesFound} opportunities`);
        
        if (result.trades.length > 0) {
          this.notify(result);
        }
      } catch (e) {
        console.error('Scan error:', e);
      }
    }, this.config.scanInterval);

    return true;
  }

  // Stop scanning
  stop() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
      return true;
    }
    return false;
  }

  // Get status
  getStatus() {
    return {
      running: !!this.intervalId,
      tradesToday: this.tradeCount,
      positions: Array.from(this.positions.entries()),
      config: this.config
    };
  }

  // Mock notification (replace with actual alert system)
  notify(result) {
    console.log('\n=== ARBITRAGE OPPORTUNITY ===');
    console.log(JSON.stringify(result, null, 2));
    console.log('============================\n');
  }

  // Reset daily counters
  resetDaily() {
    this.tradeCount = 0;
  }
}

module.exports = WeatherArbitrage;
