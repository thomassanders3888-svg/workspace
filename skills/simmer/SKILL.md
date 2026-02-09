---
name: simmer
description: Polymarket trading via Simmer SDK - weather arbitrage and prediction market automation
homepage: https://polymarket.com
metadata:
  openclaw:
    emoji: 📈
    requires:
      env:
        - SIMMER_API_KEY
        - POLYGON_RPC_URL
      bins: []
---

# Simmer SDK - Polymarket Trading

Trade on Polymarket prediction markets via Simmer SDK non-custodial wallet.

## Prerequisites

1. Simmer SDK account (email signup)
2. USDC on Polygon network
3. Entry/exit thresholds configured

## Setup Steps

### 1. Create Simmer Wallet
```
Message in Telegram: "Create Simmer SDK wallet for Polymarket"
```
Records email + generates Polygon address.

### 2. Get Polygon USDC

From Coinbase (you have this):
- Buy USDC on Coinbase
- Withdraw to your Simmer Polygon address
- **OR** Bridge from Ethereum using:
  - https://portal.polygon.technology/bridge
  - https://jumper.exchange ( aggregator)

### 3. Verify Balance
```
"Check my Simmer wallet balance"
```

### 4. Configure Weather Arbitrage

Entry: < $0.15 (15% chance)
Exit: > $0.45 (45% chance)
Locations: NYC, Chicago, Seattle, Atlanta, Dallas
Scan interval: 2 minutes
Max position: $5 per market

### 5. Start Trading
```
"Start weather arbitrage trading"
```

## Commands

| Command | Action |
|---------|--------|
| `simmer balance` | Check USDC balance |
| `simmer positions` | List open positions |
| `simmer buy <market> <amount>` | Place buy order |
| `simmer sell <market> <amount>` | Place sell order |
| `simmer weather start` | Start weather arbitrage |
| `simmer weather stop` | Pause all trading |
| `simmer withdraw <amount> <address>` | Withdraw USDC |

## Safety

- Start with $10-50 USDC max
- Set daily loss limit: $10
- Polygon gas: ~$0.01-0.10 per trade
- All trades public on polygonscan.com

## Manual Bridge from Coinbase

1. Go to https://jumper.exchange
2. Select: USDC (Ethereum) → USDC (Polygon)
3. Enter your Simmer Polygon wallet address
4. Approve + confirm
5. Wait 5-15 minutes
6. Verify in Simmer

Report: https://discord.gg/simmer or https://t.me/simmer_sdk
