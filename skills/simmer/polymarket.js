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
      return text.includes('temp') || 
             text.includes('weather') || 
             text.includes('temperature') ||
             text.includes('high') ||
             text.includes('low');
    });
  }

  // Get market by ID
  async getMarket(marketId) {
    const response = await this.gamma.get(`/markets/${marketId}`);
    return response.data;
  }

  // Get order book / prices
  async getOrderBook(tokenId) {
    const response = await this.api.get(`/orderbook/${tokenId}`);
    return response.data;
  }

  // Get current price (midpoint)
  async getPrice(tokenId) {
    const book = await this.getOrderBook(tokenId);
    
    if (!book.bids?.length || !book.asks?.length) {
      return null;
    }

    const bestBid = parseFloat(book.bids[0].price);
    const bestAsk = parseFloat(book.asks[0].price);
    
    return {
      bid: bestBid,
      ask: bestAsk,
      mid: (bestBid + bestAsk) / 2,
      spread: bestAsk - bestBid
    };
  }

  // Get user's positions (requires auth)
  async getPositions(walletAddress) {
    const response = await this.api.get(`/positions`, {
      params: { user: walletAddress }
    });
    return response.data;
  }

  // Get recent trades for a market
  async getTrades(tokenId, limit = 20) {
    const response = await this.api.get(`/trades`, {
      params: { token_id: tokenId, limit }
    });
    return response.data;
  }

  // Get market history/chart data
  async getMarketHistory(marketId, resolution = 'hour') {
    const response = await this.api.get(`/markets/${marketId}/prices`, {
      params: { resolution }
    });
    return response.data;
  }

  // Helper: Find markets by location
  findMarketsByLocation(markets, location) {
    const loc = location.toLowerCase();
    return markets.filter(m => {
      const text = (m.title + ' ' + m.description).toLowerCase();
      return text.includes(loc) ||
             (loc === 'nyc' && (text.includes('new york') || text.includes('nyc'))) ||
             (loc === 'chicago' && text.includes('chicago')) ||
             (loc.includes('seattle') && text.includes('seattle')) ||
             (loc === 'atlanta' && text.includes('atlanta')) ||
             (loc === 'dallas' && text.includes('dallas'));
    });
  }

  // Helper: Parse temperature threshold from market title
  parseTemperatureThreshold(title) {
    const match = title.match(/(\d+)\s*°?\s*[CF]/i);
    return match ? parseInt(match[1]) : null;
  }

  // Filter markets by price range (for arbitrage)
  filterByPrice(markets, minPrice, maxPrice) {
    return markets.filter(m => {
      const price = parseFloat(m.best_bid) || parseFloat(m.last_trade_price) || 0.5;
      return price >= minPrice && price <= maxPrice;
    });
  }
}

module.exports = PolymarketClient;
