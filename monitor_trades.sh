#!/bin/bash
# Trade Monitor Script
# Checks trades.log every 60s for first trade execution

LOG_FILE="/home/clawdbot267/.openclaw/workspace/trades.log"
TIMEOUT_SECONDS=86400  # 24 hours
START_TIME=$(date +%s)
ALERT_SENT=0

echo "[$(date '+%Y-%m-%d %H:%M:%S')] Trade monitoring started. Watching for first trade..."

while [ $ALERT_SENT -eq 0 ]; do
    CURRENT_TIME=$(date +%s)
    ELAPSED=$((CURRENT_TIME - START_TIME))
    
    # Check timeout
    if [ $ELAPSED -ge $TIMEOUT_SECONDS ]; then
        echo "[$(date '+%Y-%m-%d %H:%M:%S')] Timeout reached after 24 hours. No trade detected."
        exit 0
    fi
    
    # Check if log file exists
    if [ -f "$LOG_FILE" ]; then
        # Look for ENTRY, EXEC, or SIGN patterns
        FIRST_TRADE=$(grep -m1 -E '\[(ENTRY|EXEC|SIGN)\]' "$LOG_FILE" 2>/dev/null)
        
        if [ -n "$FIRST_TRADE" ]; then
            echo "[$(date '+%Y-%m-%d %H:%M:%S')] First trade detected: $FIRST_TRADE"
            
            # Extract fields (adapt regex based on actual log format)
            # Assuming format like: [ENTRY] Market: BTC-USD Side: BUY Size: 1.5 Price: 45000
            MARKET_ID=$(echo "$FIRST_TRADE" | grep -oP 'Market[:]?\s*\K[A-Z0-9-]+' || echo "Unknown")
            SIDE=$(echo "$FIRST_TRADE" | grep -oP 'Side[:]?\s*\K[A-Z]+' || echo "Unknown")
            SIZE=$(echo "$FIRST_TRADE" | grep -oP 'Size[:]?\s*\K[0-9.]+' || echo "0")
            PRICE=$(echo "$FIRST_TRADE" | grep -oP 'Price[:]?\s*\K[0-9.]+' || echo "0")
            
            echo "[$(date '+%Y-%m-%d %H:%M:%S')] Extracted: Market=$MARKET_ID, Side=$SIDE, Size=$SIZE, Price=$PRICE"
            
            # Write alert file for WhatsApp sender
            cat > /tmp/trade_alert.txt << EOF
🚀 FIRST TRADE! $SIDE $$SIZE @$PRICE — Market: $MARKET_ID
EOF
            
            ALERT_SENT=1
            exit 0
        fi
    fi
    
    sleep 60
done
