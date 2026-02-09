# Sub-Agent Success Protocol

## Before Spawning

### ✅ Task Design Checklist
- [ ] Single, verifiable deliverable (file or deploy URL)
- [ ] Explicit file path specified
- [ ] Line count or complexity estimate
- [ ] Validation criteria ("fails if X not created")

### ❌ Bad Task (why agents fail)
"Build a website for weather trading"
→ Vague, no path, no validation

### ✅ Good Task
"Create /workspace/sites/wxtrade/index.html with 200+ lines including: header, 3 feature cards, pricing table. Validate with `wc -l` and report line count."
→ Verifiable, specific, self-checking

## Task Registry (Prevents Collisions)

| Agent | Task | Started | File Check | Status |
|-------|------|---------|------------|--------|
| terra-1 | TerrainEngine.cs | 18:00 | 712 lines | 🔄 RUNNING |
| site-1 | ToolkitPro | 17:58 | EMPTY | ❌ FAILED |

Check registry before spawning similar tasks.

## Validation Protocol

Every sub-agent MUST end with:
```bash
# Verify deliverables exist
if [ ! -f "$TARGET_FILE" ]; then
  echo "FAILED: $TARGET_FILE not created"
  exit 1
fi

# Verify content
wc -l "$TARGET_FILE"
head -20 "$TARGET_FILE"

echo "SUCCESS: Created $TARGET_FILE ($(wc -l < $TARGET_FILE) lines)"
```

## Spawn Rules

1. **Never duplicate active tasks** — check registry first
2. **Never spawn >3 concurrent** — rate limits + context collapse
3. **Always set realistic timeouts** — 2hr for 200 lines, 4hr for complex
4. **Require explicit success condition** — not "done", but "file exists with X lines"

## Retry Strategy

First failure: Re-spawn with tightened spec
Second failure: Build directly (don't use sub-agent)
Third failure: Escalate to manual/build hybrid

## TerraForge Phase Strategy

| Phase | Deliverable | Technique | Agent? |
|-------|-------------|-----------|--------|
| 1. Proto-typing | Single-file throwaway | Direct build | Me |
| 2. Architecture | Interfaces + stubs | Sub-agent #1 | Yes |
| 3. Implementation | Real logic per file | Sub-agent #2-4 | Yes, parallel |
| 4. Integration | Wiring + tests | Me + sub-agent #5 | Hybrid |
| 5. Polish | Bug fixes, perf | Me directly | No agents |

**Key insight**: Agents work best on isolated, well-scoped chunks with no dependencies on other agent work.
