#!/bin/bash
cd /home/clawdbot267/.openclaw/workspace/skills/simmer
nohup node run.js > bot.log 2>&1 &
echo "Bot started with PID $!"
sleep 2
tail -30 bot.log
