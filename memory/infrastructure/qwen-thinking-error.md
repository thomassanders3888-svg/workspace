# Qwen 2.5 Coder "Thinking" Mode Error

## Error Pattern
```
400 "qwen2.5-coder:7b-q4" does not support thinking
```

## Root Cause
- **Local RTX 2080 model** (`qwen2.5-coder:7b-q4`) does not support reasoning/thinking mode
- **Thinking parameter** being passed to all model calls by default
- **Kimi K2.5** supports thinking; **Qwen** does not

## Affected Calls
```javascript
// Fails:
sessions_spawn({ task, model: "ollama/qwen2.5-coder:7b-q4" });

// Works:
sessions_spawn({ task, model: "nvidia/moonshotai/kimi-k2.5" });
```

## Fix Required
Add conditional thinking parameter based on model provider:

```javascript
const modelConfig = {
  "ollama/qwen2.5-coder:7b-q4": { thinking: false },
  "nvidia/moonshotai/kimi-k2.5": { thinking: true }
};
```

## Status
**Impact:** Medium — forces fallback to Kimi when local model preferred
**Fix priority:** Low — workaround exists (use no-thinking flag or Kimi)
