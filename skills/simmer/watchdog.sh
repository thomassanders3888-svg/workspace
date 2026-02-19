#!/bin/bash
# Trader watchdog - auto-restart if process dies

LOG_FILE="/home/clawdbot267/.openclaw/workspace/skills/simmer/watchdog.log"

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" >> "$LOG_FILE"
}

log "Watchdog started"

cd /home/clawdbot267/.openclaw/workspace/skills/simmer || exit 1

while true; do
    if ! pgrep -f "trader-fixed.js LIVE" > /dev/null; then
        log "Trader died, restarting..."
        export WALLET_PASS=simmer-VENyZWF0aW9u
        nohup node trader-fixed.js LIVE >> trades.log 2>&1 &
        log "Trader restarted with PID: $!"
    fi
    sleep 30
done
