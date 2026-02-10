# Agent Council

A multi-agent coordination system for OpenClaw that enables parallel, sequential, and consensus-based AI workflows.

## Installation

Copy the `agent-council/` folder to your OpenClaw skills directory:
```bash
~/.openclaw/workspace/skills/agent-council/
```

## Quick Start

```javascript
const Council = require('./skills/agent-council/scripts/council.js');

const council = new Council({
  mode: 'parallel',
  agents: 3,
  model: 'ollama/qwen2.5-coder:7b'
});

const result = await council.execute({
  task: 'Design a database schema',
  perspectives: ['security', 'performance', 'scalability']
});
```

## Modes

- **parallel**: All agents work simultaneously (fastest)
- **sequential**: Agents build on previous output (iterative)
- **vote**: Agents propose, then vote on best solution
- **debate**: Agents argue positions, synthesize consensus

## Documentation

- `SKILL.md` - Complete usage guide
- `references/modes.md` - Mode selection and patterns
- `examples/quickstart.js` - Working examples

## Requirements

- OpenClaw with sessions_spawn support
- Valid model configuration (Ollama, NVIDIA, etc.)

## License

MIT
