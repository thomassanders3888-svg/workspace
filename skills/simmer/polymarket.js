// Polymarket API Client
// Interacts with Polymarket prediction markets
const axios = require('axios');

const POLYMARKET_API = 'https://api.polymarket.com';
const POLYMARKET_GAMMA = 'https://gamma-api.polymarket.com';

class PolymarketClient {
  constructor() {
    this.api = axios.create({
      baseURL: POLYMARKET_API,
      timeout: 10000
    });
    this.gamma = axios.create({
      baseURL: POLYMARKET_GAMMA,
      timeout: 10000
    });
  }

  // Get active markets
  async getMarkets(params = {}) {
    const query = new URLSearchParams({
      limit: params.limit || 50,
      offset: params.offset || 0,
      active: 'true',
      ...params
    });
    const response = await this.gamma.get(`/markets?${query}`);
    return response.data;
  }

  // Search weather markets specifically
  async getWeatherMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    return markets.filter(m => {
      const text = (m.title + ' ' + m.description).toLowerCase();
      return text.includes('temp') || text.includes('weather') || text.includes('temperature') ||
             text.includes('high') || text.includes('low') || text.includes('rain') ||
             text.includes('snow') || text.includes('sunny');
    });
  }

  // Get market by ID
  async getMarket(marketId) {
    const response = await this.gamma.get(`/markets/${marketId}`);
    return response.data;
  }

  // Get markets by location
  findMarketsByLocation(markets, location) {
    const locPatterns = {
      'NYC': /new york|nyc|manhattan/i,
      'Chicago': /chicago/i,
      'Seattle': /seattle/i,
      'Atlanta': /atlanta/i,
      'Dallas': /dallas|dfw/i
    };
    const pattern = locPatterns[location];
    if (!pattern) return [];
    return markets.filter(m => pattern.test(m.title + ' ' + m.description));
  }

  // Extract temperature threshold from market title
  parseTemperatureThreshold(title) {
    const patterns = [
      /(\d+)°?\s*[Ff]/,
      /(\d+)\s+degrees/i,
      /high.*?above\s+(\d+)/i,
      /temp.*?over\s+(\d+)/i
    ];
    for (const pattern of patterns) {
      const match = title.match(pattern);
      if (match) return parseInt(match[1]);
    }
    return null;
  }

  // Get price for token
  async getPrice(tokenId) {
    try {
      const response = await this.gamma.get(`/events/${tokenId}`);
      const event = response.data;
      const bestAsk = event.bestAsk?.price || 0.5;
      const bestBid = event.bestBid?.price || 0.5;
      return {
        yes: bestAsk,
        no: bestBid,
        mid: (bestAsk + bestBid) / 2
      };
    } catch (e) {
      console.error('Price fetch error:', e.message);
      return null;
    }
  }

  // Filter markets by price range (for arbitrage)
  filterByPrice(markets, minPrice, maxPrice) {
    return markets.filter(m => {
      const price = parseFloat(m.best_bid) || parseFloat(m.last_trade_price) || 0.5;
      return price >= minPrice && price <= maxPrice;
    });
  }

  // Submit signed order to CLOB (Simulated - needs actual Polymarket SDK for production)
  async submitOrder(signedOrder) {
    console.log('[SIMULATED] Order submitted:', JSON.stringify(signedOrder, null, 2));
    return {
      success: true,
      orderId: 'sim-' + Date.now(),
      txHash: '0x' + Math.random().toString(16).substr(2, 40)
    };
  }
}

module.exports = PolymarketClient;
