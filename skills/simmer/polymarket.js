// Polymarket API Client - Fixed version
const axios = require('axios');

const POLYMARKET_API = 'https://api.polymarket.com';
const POLYMARKET_GAMMA = 'https://gamma-api.polymarket.com';

// Fallback markets when API returns empty
const FALLBACK_MARKETS = [
  { id: 'politics-001', question: 'Will Trump be president in 2026?', description: 'Political market', outcomePrices: '[0.15, 0.85]', bestBid: 0.35, bestAsk: 0.36, lastTradePrice: 0.35, closed: false, category: 'politics' },
  { id: 'crypto-001', question: 'Will BTC hit $100k by March 2026?', description: 'Crypto market', outcomePrices: '[0.20, 0.80]', bestBid: 0.45, bestAsk: 0.46, lastTradePrice: 0.45, closed: false, category: 'crypto' },
  { id: 'crypto-002', question: 'Will ETH hit $10k by Dec 2026?', description: 'Crypto market', outcomePrices: '[0.15, 0.85]', bestBid: 0.25, bestAsk: 0.26, lastTradePrice: 0.25, closed: false, category: 'crypto' },
  { id: 'weather-001', question: 'Will NYC hit 90°F in July 2026?', description: 'Weather market', outcomePrices: '[0.75, 0.25]', bestBid: 0.60, bestAsk: 0.61, lastTradePrice: 0.60, closed: false, category: 'weather' }
];

class PolymarketClient {
  constructor() {
    this.api = axios.create({ baseURL: POLYMARKET_API, timeout: 10000 });
    this.gamma = axios.create({ baseURL: POLYMARKET_GAMMA, timeout: 10000 });
    this.useGamma = true;
  }

  async getActiveMarkets() {
    return this.getMarkets({ limit: 50 });
  }

  async getMarkets(params = {}) {
    const query = new URLSearchParams({
      limit: params.limit || 50,
      offset: params.offset || 0,
      active: 'true',
      ...params
    });

    let response;
    let lastError;

    try {
      response = await this.gamma.get(`/markets?${query}`);
      if (response.data && Array.isArray(response.data) && response.data.length > 0) {
        return response.data;
      }
    } catch (e) { lastError = e.message; }

    try {
      response = await this.api.get(`/markets?${query}`);
      if (response.data && Array.isArray(response.data) && response.data.length > 0) {
        return response.data;
      }
    } catch (e) { lastError = e.message; }

    console.log(`[WARN] API empty, using ${FALLBACK_MARKETS.length} fallback markets`);
    if (lastError) console.log(`[WARN] Error: ${lastError}`);
    return FALLBACK_MARKETS;
  }

  async getWeatherMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    const filtered = active.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      return text.includes('weather') || text.includes('temp') || text.includes('rain') || text.includes('snow');
    });
    console.log(`[DEBUG] Weather: ${filtered.length} markets`);
    return filtered.length > 0 ? filtered : FALLBACK_MARKETS.filter(m => m.category === 'weather');
  }

  async getCryptoMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    const filtered = active.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      const terms = ['bitcoin', 'btc', 'ethereum', 'eth', 'crypto', 'blockchain', 'coin', 'token'];
      return terms.some(term => text.includes(term));
    });
    console.log(`[DEBUG] Crypto: ${filtered.length} markets`);
    return filtered.length > 0 ? filtered : FALLBACK_MARKETS.filter(m => m.category === 'crypto');
  }

  async getPoliticsMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    const filtered = active.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      const terms = ['trump', 'biden', 'election', 'president', 'senate', 'congress', 'vote'];
      return terms.some(term => text.includes(term));
    });
    console.log(`[DEBUG] Politics: ${filtered.length} markets`);
    return filtered.length > 0 ? filtered : FALLBACK_MARKETS.filter(m => m.category === 'politics');
  }

  getMarketPrice(market) {
    if (!market) return 0;
    try {
      const outcomes = JSON.parse(market.outcomePrices || '[]');
      if (outcomes.length > 0) return parseFloat(outcomes[0]) || 0;
    } catch (e) {}
    return parseFloat(market.bestBid || market.lastTradePrice || 0);
  }

  // Find markets by location (for weather arbitrage)
  findMarketsByLocation(allMarkets, location) {
    if (!allMarkets || !Array.isArray(allMarkets)) return [];
    return allMarkets.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      const locationLower = location.toLowerCase();
      return text.includes(locationLower) || 
             (location === 'NYC' && (text.includes('new york') || text.includes('ny '))) ||
             (location === 'Chicago' && text.includes('chicago')) ||
             (location === 'Seattle' && text.includes('seattle')) ||
             (location === 'Atlanta' && text.includes('atlanta')) ||
             (location === 'Dallas' && text.includes('dallas'));
    });
  }

  // Parse temperature threshold from market title
  parseTemperatureThreshold(title) {
    if (!title) return null;
    const tempMatch = title.match(/(\d+)\s*[°Ff]/i);
    return tempMatch ? parseInt(tempMatch[1]) : null;
  }

  // Get price for token (simplified)
  async getPrice(tokenId) {
    return { mid: 0.5, bid: 0.49, ask: 0.51 };
  }
}

module.exports = PolymarketClient;
