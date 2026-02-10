#!/bin/bash
# Hourly Content Engine - Auto-generated
# Runs every hour to generate and post content for Thomas

cd /home/clawdbot267/.openclaw/workspace || exit 1

# Log file
LOG_FILE="$HOME/.openclaw/workspace/content-engine/hourly.log"
exec >> "$LOG_FILE" 2>&1

echo "=== Content Engine Run: $(date) ==="

# Call OpenClaw to spawn the content generation agent
curl -s -X POST http://127.0.0.1:18789/spawn \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENCLAW_TOKEN" \
  -d "{
    \"task\": \"You are the Hourly Content Engine for Thomas (@deathangel267). Read MEMORY.md and memory files for his voice. Pick a topic from: nuclear energy, space exploration, AI & education, free speech, energy policy, or population trends. Research with web_search. Write ONE punchy X post (100-280 chars) + 6-12 post thread (800-2000 words total). Save to content-engine/output-\$(date +%Y-%m-%d-%H%M).json. Send content via WhatsApp to +19402304961 with note that X posting needs configuration. Report what topic you chose.\",
    \"model\": \"nvidia/moonshotai/kimi-k2.5\",
    \"thinking\": \"medium\",
    \"timeoutSeconds\": 300,
    \"cleanup\": \"keep\"
  }"

echo "=== Complete: $(date) ==="
