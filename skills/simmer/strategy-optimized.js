// Enhanced Weather Arbitrage Strategy
// Optimized for profitability with risk controls

module.exports = {
  // Core thresholds (tightened for better edge)
  thresholds: {
    entry: {
      aggressive: 0.12,  // High confidence buy <12%
      standard: 0.15,    // Normal entry <15%
      conservative: 0.18 // Lower confidence cut-off
    },
    exit: {
      takeProfit1: 0.40,  // 40%+ (2.6x return)
      takeProfit2: 0.55,  // 55%+ (3.7x return)
      moon: 0.70          // 70%+ (5.8x return - close position)
    },
    stopLoss: 0.05  // Max 5¢ loss from entry
  },

  // Position sizing based on confidence
  positionSizing: {
    highConfidence: 8,     // >30% edge
    mediumConfidence: 5,   // 15-30% edge
    lowConfidence: 2       // <15% edge
  },

  // Trading rules
  rules: {
    // Only trade when NOAA probability >65% or <35%
    minForecastConfidence: 0.65,
    
    // Require min spread (liquidity check)
    minSpread: 0.02,  // 2 cent spread
    
    // Never hold >24hrs (weather resolves)
    maxHoldTime: 24 * 60 * 60 * 1000,
    
    // Cooldown between same-market trades
    marketCooldown: 30 * 60 * 1000,  // 30 min
    
    // Volatility filter - avoid choppy markets
    minPriceHistory: 5,  // Need 5 data points
    maxVolatility: 0.15 // Max 15% hourly variance
  },

  // Time windows (weather markets have specific resolution times)
  scanning: {
    activeHours: [6, 22],  // Scan 6AM-10PM EST (weather active)
    resolutionWindow: {
      before: 4 * 60 * 60 * 1000,   // 4hrs before resolution - higher variance
      after: 2 * 60 * 60 * 1000      // After resolution - close positions
    }
  },

  // Profit taking tiers
  profitTiers: [
    { threshold: 0.70, action: 'close_full', urgency: 'immediate' },  // 400%+ profit
    { threshold: 0.55, action: 'close_75', urgency: 'high' },        // 250%+
    { threshold: 0.45, action: 'close_half', urgency: 'medium' },    // 180%+
    { threshold: 0.38, action: 'close_25', urgency: 'low' }          // 140%+
  ],

  // Risk management
  risk: {
    dailyLossLimit: 15,       // $15 max daily loss
    positionLimit: 3,         // Max 3 concurrent positions
    maxDownsidePerTrade: 3,   // $3 max loss per trade
    correlationLimit: 2,       // Max 2 positions same region
    
    // Circuit breaker
    circuitBreaker: {
      consecutiveLosses: 3,   // Pause after 3 losses
      lossThreshold: 10,      // $10 in 1 hour
      cooldownTime: 60 * 60 * 1000  // 1 hour pause
    }
  },

  // Location-specific weights (based on predictability)
  locationWeights: {
    'NYC': 1.2,     // Good NOAA coverage
    'Chicago': 1.1,  // Stable patterns
    'Seattle': 1.0,  // Moderate
    'Atlanta': 0.9,  // Less predictable
    'Dallas': 0.9    // Variable
  },

  // Advanced filters
  filters: {
    // Skip markets with <10 trades (illiquid)
    minVolume: 10,
    
    // Skip if NOAA vs market differs <15%
    minEdge: 0.15,
    
    // Avoid extreme weather days (high variance)
    rejectForecastChange: 20,  // Reject if temp swing >20°F expected
    
    // Only trade clear patterns
    requireDirectionAlignment: true  // NOAA trend must match market direction
  },

  // Execution settings
  execution: {
    slippageTolerance: 0.005,  // 0.5% max slippage
    partialFillAcceptance: 0.5, // Accept 50% fills
    retryAttempts: 3,
    retryDelay: 2000  // 2 sec
  },

  // Monitoring & alerts
  alerts: {
    onEntry: true,
    onExit: true,
    onStopLoss: true,
    onCircuitBreaker: true,
    onLargePosition: 5,  // Alert on $5+ positions
    pnlUpdateInterval: 15 * 60 * 1000  // P&L report every 15 min
  }
};

// Derived config with all settings merged
module.exports.fullConfig = {
  ...module.exports,
  
  // Locations to scan
  locations: ['NYC', 'Chicago', 'Seattle', 'Atlanta', 'Dallas'],
  
  // Scan interval (ms) - vary by time of day
  scanInterval: (hour) => {
    if (hour >= 7 && hour <= 9) return 60 * 1000;    // Morning rush: 1 min
    if (hour >= 14 && hour <= 16) return 60 * 1000;  // Afternoon: 1 min
    return 2 * 60 * 1000;  // Default: 2 min
  },
  
  // Dynamic thresholds based on time-to-resolution
  dynamicThresholds: (hoursToResolution) => {
    if (hoursToResolution < 4) {
      // Close to resolution - tighten for certainty
      return { entry: 0.10, exit: 0.35 };
    }
    if (hoursToResolution > 48) {
      // Far out - can be more aggressive
      return { entry: 0.18, exit: 0.50 };
    }
    // Normal
    return module.exports.thresholds;
  },
  
  // Expected value calculation
  expectedValue: (entryPrice, exitPrice, probability) => {
    const upside = (exitPrice - entryPrice) * probability;
    const downside = entryPrice * (1 - probability);
    return upside - downside;
  }
};
