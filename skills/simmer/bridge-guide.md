# Simmer Wallet Setup Guide

## Step 1: Create Simmer Wallet

1. Go to https://simmerx.com (Simmer SDK portal)
2. Sign up with email + password
3. **Save your recovery phrase** somewhere secure
4. Your Polygon wallet address will be displayed

## Step 2: Get Your Wallet Address

```javascript
const SimmerWallet = require('./wallet');
const wallet = new SimmerWallet();

// Create new (do this once)
const { address, mnemonic } = wallet.createWallet('your-password');
console.log('Address:', address);
console.log('Mnemonic:', mnemonic); // SAVE THIS!
```

Or load existing:
```javascript
const address = wallet.loadWallet('your-password');
console.log('Address:', address);
```

## Step 3: Bridge USDC from Coinbase to Polygon

### Option A: Direct Polygon Withdrawal (Fastest)

1. In Coinbase, go to **Send/Receive**
2. Select **USDC**
3. Choose **Polygon** network
4. Enter your Simmer wallet address
5. Send $10-50 to test

**If Coinbase doesn't show Polygon option:**

### Option B: Bridge via Jumper

1. Go to https://jumper.exchange
2. Connect MetaMask (or WalletConnect)
3. **From:** Ethereum USDC
4. **To:** Polygon USDC
5. **To Address:** Your Simmer wallet address
6. Confirm transaction (~$1-2 bridge fee)
7. Wait 5-15 minutes

### Option C: Bridge via Polygon Portal

1. Go to https://portal.polygon.technology/bridge
2. Connect wallet
3. Select USDC
4. Approve + Bridge
5. Wait for confirmation

## Step 4: Verify Balance

```javascript
const SimmerWallet = require('./wallet');
const wallet = new SimmerWallet();
wallet.loadWallet('your-password');

async function check() {
  const usdc = await wallet.getUSDCBalance();
  const matic = await wallet.getMaticBalance();
  
  console.log('USDC:', usdc.formatted);
  console.log('MATIC (gas):', matic.formatted);
}

check();
```

## Step 5: Run Arbitrage Bot

```javascript
const WeatherArbitrage = require('./arbitrage');
const SimmerWallet = require('./wallet');

const wallet = new SimmerWallet();
wallet.loadWallet('your-password');

const bot = new WeatherArbitrage({
  entryThreshold: 0.15,
  exitThreshold: 0.45,
  maxPosition: 5,
  maxTradesPerRun: 5
});

// Start scanning every 2 minutes
bot.start(wallet);

// Check status
console.log(bot.getStatus());

// Stop anytime
// bot.stop();
```

## Safety Settings

Add to your config:

```javascript
{
  dailyLossLimit: 10,     // $10 max loss per day
  dailyTradeLimit: 10,    // 10 trades max per day
  pauseOnError: true,     // Stop if API errors
  dryRun: true            // Test without real trades first!
}
```

## Gas Estimates

| Action | MATIC Cost | USD |
|--------|-----------|-----|
| Approve token | ~0.05 | $0.01 |
| Place order | ~0.1-0.2 | $0.02-0.04 |
| Cancel order | ~0.05 | $0.01 |

Keep ~$1 MATIC for gas fees.

## Quick Commands

```bash
cd /home/clawdbot267/.openclaw/workspace/skills/simmer

# Check balance
node -e "const w=new(require('./wallet')); w.loadWallet('password'); w.getUSDCBalance().then(b=>console.log(b.formatted))"

# Dry run arbitrage scan
node -e "const b=new(require('./arbitrage')); b.executeArbitrage(null, true).then(console.log)"
```

## Troubleshooting

**"Insufficient balance"**
→ You need MATIC for gas. Bridge ~$1 MATIC from Ethereum.

**"Wallet not loaded"**
→ Run wallet.loadWallet() first or wallet doesn't exist.

**"API rate limit"**
→ Wait 60 seconds, reduce scan frequency to 5 min.

**"Price null"**  
→ Market may be closed or low liquidity. Skip and move on.

## Support

- **Simmer**: https://discord.gg/simmer
- **Polymarket**: https://discord.gg/polymarket
- **Polygon**: https://discord.gg/polygon
