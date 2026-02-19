#!/bin/bash
pkill -f trader-fixed 2>/dev/null
export WALLET_PASS=simmer-VENyZWF0aW9u
export ENTRY_THRESHOLD=0.15
export EXIT_THRESHOLD=0.70
node trader-fixed.js LIVE >> trades.log 2>&1 &
echo $! > trader.pid
sleep 2
cat trader.pid
