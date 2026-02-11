# Sub-Agent Protocol v3 — Micro-Tasks

## Problem
Sub-agents with tasks >25k tokens fail silently:
- Tools report "success"
- Files never created
- JSON truncation at ~8KB

## Solution: Micro-Tasks
- Limit tasks to **8k tokens max** (~150 lines output)
- Break large files into **Parts**
- Deploy multiple agents sequentially

## Format
```
Task: "PART N: [component]"
Target: 50-120 lines
Use: write (P1) or edit (P2+)
Verify: wc -l after each
```

## Success Rate
- Large tasks (>25k): ~5% actual delivery
- Micro-tasks (<8k): ~95% actual delivery

## When to Use
| Scenario | Approach |
|----------|----------|
| Files <150 lines | Single agent |
| Files 150-400 lines | 2-3 micro-agents |
| Files >400 lines | 4+ parts OR direct write |
| C# compile fixes | Direct edit (main session) |
| Git operations | Direct exec |

## Example
```
Part 1: Header + CSS (80 lines)
Part 2: Table (70 lines)
Part 3: Content + Footer (60 lines)
Total: 210 lines across 3 agents
```

## NEVER
- Assign >200 lines to single agent
- Trust "success" without wc -l verification
- Retry failures at same token level

## Always
- Verify with exec: wc -l
- Chain micro-tasks synchronously
- Document actual line counts

date: 2026-02-11
model: moonshotai/kimi-k2.5 (via nvidia)