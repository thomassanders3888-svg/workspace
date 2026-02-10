# Agent Council Modes Reference

## Mode Selection Guide

| Mode | Speed | Cost | Best For | Trade-off |
|------|-------|------|----------|-----------|
| **Parallel** | Fastest (N× parallel) | High (N agents) | Multiple perspectives | No iteration |
| **Sequential** | Slow (N× serial) | High | Iterative refinement | Slowest |
| **Vote** | Medium | Very High (2N agents) | Clear decisions | Expensive |
| **Debate** | Slow | Very High | Complex trade-offs | May not converge |

## Parallel Mode

**When to use:**
- Need multiple approaches quickly
- Want diverse creative solutions
- Problem has no single "right" answer
- Time-sensitive decision

**Example:**
```javascript
const council = new Council({ mode: 'parallel', agents: 3 });
await council.execute({
  task: 'Generate 3 different solutions',
  agentPrompts: [
    'Focus on minimal code',
    'Focus on maximum performance', 
    'Focus on readability'
  ]
});
```

## Sequential Mode

**When to use:**
- Solution needs refinement
- Each step builds on previous
- Validation at each stage
- Complex multi-step problems

**Example:**
```javascript
const council = new Council({ mode: 'sequential', agents: 4 });
await council.execute({
  task: 'Design a system',
  agentPrompts: [
    'Design the core architecture',
    'Add error handling and edge cases',
    'Optimize for performance',
    'Add monitoring and observability'
  ]
});
```

## Vote Mode

**When to use:**
- Clear best option needed
- Multiple valid approaches
- Want confidence through consensus
- Democratic decision making

**Voting Thresholds:**
- `requireUnanimous: true` — All agents must agree
- `requireUnanimous: false` — Simple majority (50%+)

**Example:**
```javascript
const council = new Council({ 
  mode: 'vote', 
  agents: 5,
  requireUnanimous: false  // 3/5 wins
});
```

## Debate Mode

**When to use:**
- Complex trade-offs (security vs usability)
- Opposing valid viewpoints
- Need thorough exploration of edges
- Stakes are high

**Configuration:**
- `rounds`: Number of debate rounds (default: 2)
- `positions`: Force specific stances per agent

**Example:**
```javascript
const council = new Council({ 
  mode: 'debate', 
  agents: 4,
  rounds: 3
});
await council.execute({
  task: 'Should we use blockchain?',
  positions: [
    'Argue FOR blockchain (decentralization)',
    'Argue AGAINST blockchain (complexity)',
    'Argue FOR hybrid approach',
    'Argue for specific use-case only'
  ]
});
```

## Cost Optimization

**Reduce costs while keeping Council benefits:**

1. **Cheaper synthesis model:**
```javascript
const council = new Council({
  model: 'ollama/qwen2.5-coder:7b',      // For agents
  synthesisModel: 'ollama/qwen2.5-coder:7b' // Same or cheaper for final
});
```

2. **Fewer agents:**
- 2 agents: Good for binary decisions (for/against)
- 3 agents: Good for general multi-perspective
- 5+ agents: Only for high-stakes decisions

3. **Hybrid mode:**
- Start with parallel (3 agents)
- Take top 2 outputs
- Run debate between them

## Common Patterns

### Pattern: Review +Vote
```javascript
// First: Parallel review
const review = await council.execute({ mode: 'parallel', task: 'Review code' });

// Then: Vote on best fixes
const fixes = await council.execute({ 
  mode: 'vote', 
  task: 'Which fixes should we apply?',
  proposals: review.agents.map(a => a.output)
});
```

### Pattern: Explore → Refine
```javascript
// First: Broad exploration
const ideas = await council.execute({ 
  mode: 'parallel', 
  agents: 5,
  task: 'Generate ideas'
});

// Take best idea, sequential refinement
const refined = await council.execute({
  mode: 'sequential',
  agents: 3,
  task: `Refine: ${ideas.best}`,
  agentPrompts: ['Add structure', 'Add error handling', 'Optimize']
});
```
