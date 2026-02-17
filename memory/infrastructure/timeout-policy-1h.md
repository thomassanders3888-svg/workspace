# Timeout Policy: 1 Hour Maximum

**Date:** Feb 17, 2026 24f7c569-fe3b-4c5c-8b9e-c2d8a6c9e4a7
**Policy:** All tasks default to 1 hour (3600s) timeout

## Rationale

- **MiniMAX M2.5 (138GB):** Cold start + complex generation needs 15-45 minutes
- **M2.5 steady state:** 10-20 min for medium tasks
- **Large/complex tasks:** Up to 1 hour for full completion
- **Prevents timeouts on:** Code generation, research, multi-file writes

## Applied To

| Service | Was | Now | Model |
|---------|-----|-----|-------|
| simmer-trader-5min | 45s | **3600s** | M2.5 |
| motivation-manager | 30s | **3600s** | M2.5 |
| trader-heal-8h | 60s | **3600s** | Kimi |
| daily-openclaw-audit | 120s | **3600s** | Kimi |
| sessions_spawn default | 180s | **3600s** | Varies |

## Guidelines

- **Simple tasks:** Complete in 5-15 min
- **Medium tasks:** 15-30 min
- **Complex tasks:** 45-60 min (now safe)
- **Always set:** timeoutSeconds explicitly

## Implementation

```javascript
// Subagents
sessions_spawn({
  task: "...",
  model: "frob/minimax-m2.5:230b-a10b-q4_K_M",
  timeoutSeconds: 3600  // 1 hour
});

// Cron jobs
{
  "timeoutSeconds": 3600,
  "model": "frob/minimax-m2.5:230b-a10b-q4_K_M"
}
```
