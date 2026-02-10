---
name: agent-council
description: Coordinate multiple AI agents to collaborate, debate, and synthesize solutions on complex tasks. Use when you need parallel agent execution, multi-perspective analysis, consensus building, or distributed problem-solving. Triggers on requests like "get multiple opinions", "council of agents", "collaborative analysis", "debate approaches", "parallel agents", or multi-agent workflows.
---

# Agent Council

A multi-agent coordination system for parallel, sequential, and consensus-based agent workflows.

## Quick Start

```javascript
const Council = require('./scripts/council.js');

const council = new Council({
  mode: 'parallel',      // 'parallel', 'sequential', 'vote', 'debate'
  agents: 3,             // Number of agent perspectives
  model: 'ollama/qwen2.5-coder:7b'
});

const result = await council.execute({
  task: 'Design a database schema for user authentication',
  perspectives: ['security', 'performance', 'scalability']
});
```

## Modes

### Parallel (parallel)
Spawn N agents simultaneously with same task, different seeds. Fastest option for multi-perspective analysis.

### Sequential (sequential)
Agents run in order, each building on previous output. Use for iterative refinement.

### Vote (vote)
Agents propose solutions, then vote on best. Use when clear winner needed from options.

### Debate (debate)
Agents argue opposing positions, then synthesize consensus. Use for complex trade-offs.

## Advanced Usage

```javascript
// Custom coordination
const council = new Council({
  mode: 'vote',
  agents: 5,
  model: 'nvidia/moonshotai/kimi-k2.5',
  requireUnanimous: false,    // Allow majority (3/5)
  timeoutSeconds: 120,
  synthesisModel: 'ollama/qwen2.5-coder:7b'  // Cheaper for final synthesis
});

// With custom prompts per agent
await council.execute({
  task: 'Refactor this function',
  target: 'function code here',
  agentPrompts: [
    'Focus on readability',
    'Focus on performance',
    'Focus on type safety'
  ]
});
```

## Output Format

```json
{
  "consensus": "final synthesized solution",
  "agents": [
    {"id": 1, "output": "...", "votes": 2},
    {"id": 2, "output": "...", "votes": 1}
  ],
  "seconds": 45.2,
  "mode": "vote"
}
```

## When to Use

Use Agent Council when:
- Task has trade-offs (speed vs. security vs. simplicity)
- You want multiple creative approaches to choose from
- You need confidence through consensus
- Single agent might miss edge cases
- Problem benefits from domain specialization

Avoid when:
- Task is straightforward (adds overhead)
- Speed is critical (parallel helps, but still overhead)
- Cost-sensitive (runs multiple agents)
