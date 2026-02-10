// Agent Council - Quick Start Example
const Council = require('../scripts/council.js');

async function main() {
  // Example 1: Parallel mode for code review
  console.log('=== Example 1: Parallel Code Review ===');
  const codeReview = new Council({
    mode: 'parallel',
    agents: 3,
    model: 'ollama/qwen2.5-coder:7b'
  });

  const review = await codeReview.execute({
    task: 'Review this function for bugs and improvements',
    target: `
function calculateTotal(items) {
  let total = 0;
  for (let i = 0; i < items.length; i++) {
    total += items[i].price * items[i].qty;
  }
  return total;
}
    `,
    agentPrompts: [
      'Focus on edge cases and null safety',
      'Focus on performance optimizations',
      'Focus on code readability and maintainability'
    ]
  });

  console.log('Review complete:', review.seconds, 'seconds');

  // Example 2: Vote mode for architecture decision
  console.log('\n=== Example 2: Architecture Decision ===');
  const decision = new Council({
    mode: 'vote',
    agents: 5,
    model: 'nvidia/moonshotai/kimi-k2.5',
    requireUnanimous: false
  });

  const arch = await decision.execute({
    task: 'Choose the best database for a real-time chat app',
    options: ['PostgreSQL', 'MongoDB', 'Redis', 'Cassandra']
  });

  console.log('Decision:', arch.winner);

  // Example 3: Debate mode for trade-offs
  console.log('\n=== Example 3: Security vs Usability Debate ===');
  const debate = new Council({
    mode: 'debate',
    agents: 4,
    rounds: 2
  });

  const security = await debate.execute({
    task: 'Should we implement mandatory 2FA for all users?',
    positions: [
      'Argue FOR mandatory 2FA (security priority)',
      'Argue AGAINST mandatory 2FA (usability priority)',
      'Argue FOR conditional 2FA (balanced approach)',
      'Argue FOR alternative security measures'
    ]
  });

  console.log('Consensus:', security.consensus);
}

main().catch(console.error);
