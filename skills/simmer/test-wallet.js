const WalletManager = require('./wallet');
const PASSWORD = 'simmer-VENyZWF0aW9u';

async function test() {
  console.log('=== Wallet Balance Test ===\n');
  
  const wallet = new WalletManager();
  
  // Test RPCs first
  console.log('Testing RPC endpoints...');
  const rpcResult = await wallet.testRPCs();
  console.log('RPC Result:', rpcResult.ok ? `✓ ${rpcResult.url} (block ${rpcResult.blockNumber})` : `✗ ${rpcResult.error}`);
  
  // Load wallet
  console.log('\nLoading wallet...');
  const address = wallet.loadWallet(PASSWORD);
  console.log('Address:', address);
  
  // Get balance
  console.log('\nFetching USDC balance...');
  try {
    const balance = await wallet.getUSDCBalance();
    console.log('✓ Balance:', balance.formatted, 'USDC');
    console.log('  Raw:', balance.raw);
    console.log('  RPC:', balance.rpc);
  } catch (e) {
    console.log('✗ Balance error:', e.message);
  }
  
  // Get MATIC
  console.log('\nFetching MATIC balance...');
  try {
    const matic = await wallet.getMaticBalance();
    console.log('✓ MATIC:', matic.formatted);
  } catch (e) {
    console.log('✗ MATIC error:', e.message);
  }
}

test().catch(console.error);
