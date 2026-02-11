#!/bin/bash

LOG_FILE="/home/clawdbot267/.openclaw/workspace/trades.log"
ALERT_MARKER="/tmp/trade_alert_sent"
CHECK_COUNT=0
MAX_CHECKS=1440  # 24 hours worth of 60s checks

echo "Watching for first trade in trades.log..."

while [ $CHECK_COUNT -lt $MAX_CHECKS ]; do
    CHECK_COUNT=$((CHECK_COUNT + 1))
    
    if [ -f "$LOG_FILE" ]; then
        # Check for any trade-related markers
        FIRST_TRADE=$(grep -m1 -E '\[(ENTRY|EXEC|SIGN)\]' "$LOG_FILE" 2>/dev/null)
        
        if [ -n "$FIRST_TRADE" ] && [ ! -f "$ALERT_MARKER" ]; then
            echo "$FIRST_TRADE" > "$ALERT_MARKER"
            cat "$ALERT_MARKER"
            exit 0
        fi
    fi
    
    sleep 60
done

echo "TIMEOUT: No trade detected within 24 hours"
