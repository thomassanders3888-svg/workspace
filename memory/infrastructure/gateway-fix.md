# OpenClaw Gateway Token Mismatch - Analysis & Fix

## Incident Report
**Date:** Feb 17, 2026  
**Time:** 10:24 AM - 10:43 AM ET  
**Severity:** High (blocks subagent execution)  
**Error:** `gateway closed (1008): unauthorized: device token mismatch`

---

## Root Cause Analysis

### Error Pattern
```
gateway closed (1008): unauthorized: device token mismatch (rotate/reissue device token)
Gateway target: ws://127.0.0.1:18789
Source: local loopback
Config: /home/clawdbot267/.openclaw/openclaw.json
Bind: loopback
```

### Trigger Conditions
- **Affects:** All `sessions_spawn` calls
- **5 consecutive failures:** 2026-02-17 10:17-10:43
- **Not affecting:** Main session (direct tool execution works)
- **Token location:** `/home/clawdbot267/.openclaw/openclaw.json`

### Likely Causes

1. **Token Rotation Expired**  
   - Subagent tokens may have auto-rotated
   - Main session token still valid
   - Mismatch between main↔subagent auth

2. **Session State Desync**  
   - Subagent session keys generated but tokens invalid
   - `agent:main:subagent:*` sessions orphaned

3. **Gateway Service Issue**  
   - Local WebSocket (127.0.0.1:18789) may need restart
   - Token validation service out of sync

---

## Immediate Workaround (Deployed)

### Fallback Protocol: Direct Execution
When `sessions_spawn` fails with 1008:
```javascript
// SKIP: sessions_spawn(task, agentId, model)
// USE: write(file_path, content) directly

// Proven method (TerraForge night shift):
// 1. Use Kimi K2.5 via NVIDIA API for architecture
// 2. Use RTX 2080 (qwen2.5-coder) for implementation  
// 3. Commit files individually
// 4. No subagents required
```

### Parallel Execution Pattern (Validated)
```
For N parallel tasks:
├── IF sessions_spawn fails → Switch to write mode
├── Execute via write() for each path
├── Use RTX 2080 + NVIDIA APIs as needed
└── Git commit per path
```

---

## Permanent Fix Steps

### Option 1: Token Refresh (Recommended)
```bash
# 1. Stop OpenClaw service
sudo systemctl stop openclaw

# 2. Backup current config
cp ~/.openclaw/openclaw.json ~/.openclaw/openclaw.json.bak.$(date +%Y%m%d-%H%M%S)

# 3. Regenerate device token
# (Requires re-authentication via `openclaw login` or web UI)

# 4. Restart service
sudo systemctl start openclaw

# 5. Verify with test subagent
openclaw sessions-spawn --task "echo test" --model kimi-k2.5
```

### Option 2: Gateway Restart
```bash
# If token refresh fails, restart gateway
pkill -f "openclaw gateway"
sleep 2
openclaw gateway start

# Check status
openclaw gateway status
```

### Option 3: Config Restore
```bash
# Restore from backup if corruption suspected
cp ~/Desktop/config\ backups/4.txt ~/.openclaw/openclaw.json
# (Verify tokens match before restart)
```

---

## Prevention Measures

1. **Pre-Flight Check** (Recommended before multi-agent work):
   ```bash
   # Test subagent before parallel deployment
   openclaw sessions-spawn --task "ls" --model kimi-k2.5
   # IF fails → Use direct execution mode
   ```

2. **Token Expiry Monitoring**:
   - Check token age: `grep -E "lastTouchedAt|lastTouchedVersion" ~/.openclaw/openclaw.json`
   - Alert if >7 days old

3. **Dual-Path Architecture** (Implemented):
   - Always maintain direct execution fallback
   - Document in AGENTS.md: "If subagents fail → write tool"

---

## Appendix: Error Log

| Time | Session | Error |
|------|---------|-------|
| 10:24:41 | c9ab868a | 1008 unauthorized |
| 10:24:41 | 1ba3cab4 | 1008 unauthorized |
| 10:24:41 | d7210292 | 1008 unauthorized |
| 10:42:36 | 276ae0a4 | 1008 unauthorized |
| 10:47:18 | a00b4e06 | 1008 unauthorized |

**Status:** Workaround active, permanent fix pending manual intervention.
