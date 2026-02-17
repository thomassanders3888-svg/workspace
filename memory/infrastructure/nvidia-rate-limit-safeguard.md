# NVIDIA Kimi K2.5 Free Tier Rate Limit Safeguard

## Problem
- **NVIDIA API free tier** has rate limits (429 errors)
- **Impact:** Subagents stall when rate limited
- **Frequency:** During parallel execution (5+ concurrent requests)

## Safeguard Implementation

### Method 1: Config-Based Circuit Breaker
Add to `agents.defaults` in config:

```json
{
  "agents": {
    "defaults": {
      "timeoutSeconds": 120,
      "maxRetries": 3,
      "retryDelayMs": 5000,
      "fallbackModels": ["ollama/qwen2.5-coder:7b-q4"],
      "rateLimitHandling": {
        "429": {
          "backoffType": "exponential",
          "baseDelayMs": 5000,
          "maxDelayMs": 60000,
          "fallbackAfterRetries": true
        }
      }
    }
  }
}
```

### Method 2: Subagent Spawn Wrapper
```javascript
async function spawnWithSafeguard(task, options = {}) {
  const models = ['nvidia/moonshotai/kimi-k2.5', 'ollama/qwen2.5-coder:7b-q4'];
  const maxRetries = options.maxRetries || 3;
  
  for (let attempt = 0; attempt < maxRetries; attempt++) {
    for (const model of models) {
      try {
        return await sessions_spawn({
          task,
          model,
          timeoutSeconds: 120,
          ...options
        });
      } catch (e) {
        if (e.message?.includes('429') || e.statusCode === 429) {
          console.log(`Rate limited on ${model}, retrying...`);
          await sleep(5000 * Math.pow(2, attempt)); // Exponential backoff
          continue;
        }
        if (model === models[models.length - 1]) throw e;
      }
    }
  }
}
```

### Method 3: Rate Limit Monitor (Active)
Track and throttle requests:

```javascript
class RateLimitGuard {
  constructor() {
    this.requests = [];
    this.windowMs = 60000; // 1 minute window
    this.maxRequests = 5; // Conservative for free tier
  }
  
  async checkLimit() {
    const now = Date.now();
    this.requests = this.requests.filter(t => now - t < this.windowMs);
    
    if (this.requests.length >= this.maxRequests) {
      const oldest = this.requests[0];
      const waitMs = this.windowMs - (now - oldest);
      console.log(`Rate limit guard: waiting ${waitMs}ms`);
      await sleep(waitMs);
    }
    
    this.requests.push(now);
  }
}
```

### Method 4: Hybrid Model Strategy
```javascript
const MODEL_TIERS = {
  primary: 'nvidia/moonshotai/kimi-k2.5',      // Use first
  fallback: 'ollama/qwen2.5-coder:7b-q4',       // 429 → fallback
  emergency: 'ollama/kimi-k2.5:agent'           // Both fail → local
};

async function smartSpawn(task) {
  // Try primary with rate limit check
  if (await canUseNvidia()) {
    try {
      return await sessions_spawn({ task, model: MODEL_TIERS.primary });
    } catch (e) {
      markNvidiaRateLimited();
    }
  }
  
  // Fallback to local
  return await sessions_spawn({ 
    task, 
    model: MODEL_TIERS.fallback,
    thinking: false  // Qwen doesn't support thinking
  });
}
```

## Recommended Configuration

Add to `MEMORY.md` or `AGENTS.md`:

```markdown
## NVIDIA Rate Limit Handling

### Parallel Execution Limits
- Max simultaneous Kimi calls: **3**
- Retry delay: **5s exponential**
- Fallback: **Local RTX 2080 (qwen)**

### Error Codes
| Code | Action |
|------|--------|
| 429 | Wait 5s, retry, fallback if persists |
| 503 | Immediate fallback to local |
| 500 | Retry once, then fallback |

### Usage Pattern
Use Kimi for:
- Architecture decisions
- Complex integration
- Planning phases

Use Qwen (RTX 2080) for:
- Code generation (< 300 lines)
- Parallel batch tasks
- Fallback when Kimi rate limited
```

## Monitoring

Track in `memory/heartbeat-state.json`:
```json
{
  "lastMinuteRequests": {
    "nvidia": 3,
    "local": 5
  },
  "rateLimitHits": 2,
  "fallbackActivations": 2
}
```

## Immediate Application

When spawning subagents now:

```javascript
// Stagger launches by 2 seconds to avoid burst
const tasks = [task1, task2, task3, task4, task5];
for (let i = 0; i < tasks.length; i++) {
  setTimeout(() => {
    sessions_spawn({
      task: tasks[i],
      model: i < 3 ? 'nvidia/moonshotai/kimi-k2.5' : 'ollama/qwen2.5-coder:7b-q4'
    });
  }, i * 2000);
}
```

---

**Last Updated:** Feb 17, 2026
**Status:** Active - Apply to all multi-subagent operations
