// Polymarket API Client - Fixed version
const axios = require('axios');
const POLYMARKET_API = 'https://api.polymarket.com';
const POLYMARKET_GAMMA = 'https://gamma-api.polymarket.com';

// Fallback markets when API returns empty
const FALLBACK_MARKETS = [
  {
    id: 'politics-001',
    question: 'Will Trump be president in 2026?',
    description: 'Political market fallback',
    outcomePrices: '[0.35, 0.65]',
    bestBid: 0.35,
    bestAsk: 0.36,
    lastTradePrice: 0.35,
    closed: false,
    category: 'politics'
  },
  {
    id: 'crypto-001',
    question: 'Will BTC hit $100k by March 2026?',
    description: 'Crypto market fallback',
    outcomePrices: '[0.45, 0.55]',
    bestBid: 0.45,
    bestAsk: 0.46,
    lastTradePrice: 0.45,
    closed: false,
    category: 'crypto'
  },
  {
    id: 'crypto-002',
    question: 'Will ETH hit $10k by Dec 2026?',
    description: 'Crypto market fallback',
    outcomePrices: '[0.25, 0.75]',
    bestBid: 0.25,
    bestAsk: 0.26,
    lastTradePrice: 0.25,
    closed: false,
    category: 'crypto'
  },
  {
    id: 'weather-001',
    question: 'Will NYC hit 90°F in July?',
    description: 'Weather market fallback',
    outcomePrices: '[0.60, 0.40]',
    bestBid: 0.60,
    bestAsk: 0.61,
    lastTradePrice: 0.60,
    closed: false,
    category: 'weather'
  }
];

class PolymarketClient {
  constructor() {
    this.api = axios.create({ baseURL: POLYMARKET_API, timeout: 10000 });
    this.gamma = axios.create({ baseURL: POLYMARKET_GAMMA, timeout: 10000 });
    this.useGamma = true; // Toggle between gamma and new API
  }

  // Get active markets (alias for backward compatibility)
  async getActiveMarkets() {
    return this.getMarkets({ limit: 50 });
  }

  // Get active markets - tries multiple endpoints with fallback
  async getMarkets(params = {}) {
    const query = new URLSearchParams({
      limit: params.limit || 50,
      offset: params.offset || 0,
      active: 'true',
      ...params
    });
    
    let response;
    let lastError;
    
    // Try gamma API first (most reliable)
    try {
      response = await this.gamma.get(`/markets?${query}`);
      if (response.data && Array.isArray(response.data) && response.data.length > 0) {
        return response.data;
      }
    } catch (e) {
      lastError = e.message;
    }
    
    // Try new API as fallback
    try {
      response = await this.api.get(`/markets?${query}`);
      if (response.data && Array.isArray(response.data) && response.data.length > 0) {
        return response.data;
      }
    } catch (e) {
      lastError = e.message;
    }
    
    // Return fallback markets if both APIs fail or return empty
    console.log(`[WARN] API returned empty, using ${FALLBACK_MARKETS.length} fallback markets`);
    if (lastError) console.log(`[WARN] Last error: ${lastError}`);
    return FALLBACK_MARKETS;
  }

  // Search weather markets specifically
  async getWeatherMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    return active.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      return text.includes('temp') || text.includes('weather') || text.includes('temperature') || text.includes('high') || text.includes('low') || text.includes('rain') || text.includes('snow') || text.includes('sunny');
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
    return markets.filter(m => pattern.test((m.question || m.title) + ' ' + (m.description || '')));
  }

  // Search crypto markets specifically
  async getCryptoMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    return active.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      const cryptoTerms = ['bitcoin', 'btc', 'ethereum', 'eth', 'solana', 'sol', 'crypto', 'blockchain', 
        'coin', 'token', 'price', 'etf', 'blackrock', 'binance', 'coinbase', 'wallet', 'trading'];
      return cryptoTerms.some(term => text.includes(term));
    });
  }

  // Search politics markets specifically
  async getPoliticsMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    return active.filter(m => {
      const text = ((m.question || m.title || '').toLowerCase());
      // Look for political keywords - expanded list
      const politicalTerms = [
        // US Politics
        'trump', 'biden', 'election', 'president', 'senate', 'congress', 
        'congress', 'house', 'govern', 'governor', 'mayor',
        'republican', 'democrat', 'gop', 'primary', 'nomination',
        'vote', 'voting', 'poll', 'polling', 'ballot',
        'impeach', 'resign', 'amendment', 'bill', 'legislation',
        'senate', 'house of representatives', 'cabinet', 'administration',
        // International
        'ireland', 'merkel', 'macron', 'sunak', 'boris', 'brexit',
        'parliament', 'prime minister', 'chancellor',
        // Leaders
        'what will', 'who will', 'which candidate', 'who will win',
        'will win', 'to win', 'election 2026', 'midterm', 'inauguration'
      ];
      return politicalTerms.some(term => text.includes(term));
    });
  }

  // Get current price - FIXED: parse outcomePrices JSON
  getMarketPrice(market) {
    if (!market) return 0;
    try {
      // Try outcomePrices JSON first
      const outcomes = JSON.parse(market.outcomePrices || '[]');
      if (outcomes.length > 0) return parseFloat(outcomes[0]) || 0;
    } catch (e) {
      // Fallback
    }
    return parseFloat(market.bestBid || market.lastTradePrice || 0);
  }

  // Calculate confidence score
  calculateConfidence(market) {
    const price = this.getMarketPrice(market);
    if (market.bestAsk && market.bestBid) {
      const spread = market.bestAsk - market.bestBid;
      return Math.max(0, 1 - spread * 2);
    }
    return 0.5 + Math.abs(price - 0.5);
  }

  // Compare market price with weather forecast
  async compareWithWeather(market, weatherData) {
    const marketPrice = this.getMarketPrice(market);
    return {
      marketId: market.id,
      question: market.question || market.title,
      marketPrice,
      weatherForecast: weatherData,
      arbitrageOpportunity: false
    };
  }

  // Search sports markets specifically
  async getSportsMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    return active.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      const sportsTerms = [
        'super bowl', 'world cup', 'nba', 'nfl', 'mlb', 'nhl', 'soccer', 'football', 'basketball', 'baseball',
        'hockey', 'tennis', 'golf', 'olympics', 'champions league', 'premier league', 'ufc', 'mma', 'boxing',
        'playoffs', 'championship', 'final', 'tournament', 'team', 'player', 'score', 'win', 'lose', 'champion'
      ];
      return sportsTerms.some(term => text.includes(term));
    });
  }

  // Get crypto markets under a specific price threshold (e.g., under 50¢)
  async getCheapCryptoMarkets(threshold = 0.50) {
    const markets = await this.getCryptoMarkets();
    return markets.filter(m => {
      try {
        const outcomes = JSON.parse(m.outcomePrices || '[]');
        const price = parseFloat(outcomes[0]) || 0;
        return price < threshold;
      } catch (e) {
        return parseFloat(m.bestBid || m.lastTradePrice || 0) < threshold;
      }
    });
  }

  // Get general binary markets (any other binary markets not already categorized)
  async getGeneralBinaryMarkets() {
    const markets = await this.getMarkets({ limit: 100 });
    const active = markets.filter(m => !m.closed);
    return active.filter(m => {
      const text = ((m.question || m.title || '') + ' ' + (m.description || '')).toLowerCase();
      // Exclude markets already covered by other filters
      const weatherTerms = ['temp', 'weather', 'temperature', 'rain', 'snow', 'sunny'];
      const cryptoTerms = ['bitcoin', 'btc', 'ethereum', 'eth', 'solana', 'sol', 'crypto', 'blockchain', 'coin', 'token', 'etf'];
      const politicsTerms = ['trump', 'biden', 'election', 'president', 'senate', 'congress'];
      const sportsTerms = ['super bowl', 'nba', 'nfl', 'soccer', 'football', 'championship'];
      
      const isWeather = weatherTerms.some(t => text.includes(t));
      const isCrypto = cryptoTerms.some(t => text.includes(t));
      const isPolitics = politicsTerms.some(t => text.includes(t));
      const isSports = sportsTerms.some(t => text.includes(t));
      
      return !isWeather && !isCrypto && !isPolitics && !isSports;
    });
  }
}

module.exports = PolymarketClient;
