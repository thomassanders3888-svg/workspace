// Carbon Credit Exchange - Trading Engine

const carbonCredits = {
    'ccx-2024': { name: 'CCX-2024', price: 18.50, change: 2.3 },
    'vcs-redd': { name: 'VCS-REDD', price: 22.10, change: -0.8 },
    'gold-standard': { name: 'GOLD-STAND', price: 28.90, change: 1.1 },
    'corsia': { name: 'CORSIA', price: 35.20, change: 3.2 }
};

let userBalance = 12450.00;
let positions = [
    { type: 'ccx-2024', quantity: 2500, avgPrice: 17.80 },
    { type: 'vcs-redd', quantity: 1000, avgPrice: 21.50 }
];

let orderBook = {
    asks: [
        { price: 18.65, size: 500 },
        { price: 18.70, size: 1200 },
        { price: 18.75, size: 800 }
    ],
    bids: [
        { price: 18.45, size: 950 },
        { price: 18.40, size: 1500 },
        { price: 18.35, size: 2100 }
    ]
};

const tradeHistory = [
    { time: '2:24:15 PM', type: 'ccx-2024', side: 'buy', price: 18.50, amount: 500, total: 9250 },
    { time: '2:23:42 PM', type: 'ccx-2024', side: 'sell', price: 18.48, amount: 200, total: 3696 },
    { time: '2:22:08 PM', type: 'vcs-redd', side: 'buy', price: 22.10, amount: 1000, total: 22100 },
    { time: '2:21:33 PM', type: 'gold-standard', side: 'sell', price: 28.85, amount: 500, total: 14425 }
];

// Price chart
function initChart() {
    const ctx = document.getElementById('priceChart').getContext('2d');
    
    // Simulated price data
    const data = [];
    let price = 18.50;
    for (let i = 0; i < 24; i++) {
        price += (Math.random() - 0.48) * 0.5;
        data.push(price);
    }
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: Array(24).fill('').map((_, i) => `${i}:00`),
            datasets: [{
                label: 'CCX-2024',
                data: data,
                borderColor: '#10b981',
                tension: 0.4,
                fill: true,
                backgroundColor: 'rgba(16, 185, 129, 0.1)'
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { display: false } },
            scales: {
                y: { 
                    min: Math.min(...data) * 0.95,
                    max: Math.max(...data) * 1.05
                }
            }
        }
    });
}

// Update trade calculations
function updateTradeCalc() {
    const amount = parseFloat(document.getElementById('trade-amount').value) || 0;
    const price = parseFloat(document.getElementById('trade-price').value) || 0;
    const subtotal = amount * price;
    const fee = subtotal * 0.005;
    const total = subtotal + fee;
    
    document.getElementById('subtotal').textContent = `$${subtotal.toFixed(2)}`;
    document.getElementById('fee').textContent = `$${fee.toFixed(2)}`;
    document.getElementById('total').textContent = `$${total.toFixed(2)}`;
}

// Execute trade
function executeTrade() {
    const amount = parseFloat(document.getElementById('trade-amount').value);
    const price = parseFloat(document.getElementById('trade-price').value);
    const orderType = document.getElementById('order-type').value;
    
    if (!amount || !price) {
        alert('Please enter amount and price');
        return;
    }
    
    const total = amount * price * 1.005; // Include fee
    
    if (userBalance >= total) {
        // Update balance
        userBalance -= total;
        document.getElementById('usd-balance').textContent = userBalance.toFixed(2);
        
        // Add position
        const existing = positions.find(p => p.type === document.getElementById('credit-select').value);
        if (existing) {
            const newAvg = (existing.quantity * existing.avgPrice + amount * price) / (existing.quantity + amount);
            existing.quantity += amount;
            existing.avgPrice = newAvg;
        } else {
            positions.push({
                type: document.getElementById('credit-select').value,
                quantity: amount,
                avgPrice: price
            });
        }
        
        // Add to history
        tradeHistory.unshift({
            time: new Date().toLocaleTimeString(),
            type: document.getElementById('credit-select').value,
            side: 'buy',
            price: price,
            amount: amount,
            total: total
        });
        
        updatePositions();
        updateRecentTrades();
        alert(`Order filled: ${amount} credits at $${price.toFixed(2)}`);
    } else {
        alert('Insufficient balance');
    }
}

// Update positions display
function updatePositions() {
    const tbody = document.getElementById('positions-body');
    tbody.innerHTML = positions.map(pos => {
        const credit = carbonCredits[pos.type];
        const currentPrice = credit.price;
        const pnl = (currentPrice - pos.avgPrice) * pos.quantity;
        const pnlClass = pnl >= 0 ? 'positive' : 'negative';
        return `
            <tr>
                <td>${credit.name}</td>
                <td>${pos.quantity.toLocaleString()}</td>
                <td>$${pos.avgPrice.toFixed(2)}</td>
                <td>$${currentPrice.toFixed(2)}</td>
                <td class="${pnlClass}">${pnl >= 0 ? '+' : ''}$${pnl.toFixed(2)}</td>
                <td><button class="btn-small" onclick="sellPosition('${pos.type}')">Sell</button></td>
            </tr>
        `;
    }).join('');
}

// Update recent trades
function updateRecentTrades() {
    const list = document.getElementById('trades-list');
    list.innerHTML = tradeHistory.slice(0, 10).map(trade => `
        <div class="trade-item">
            <span class="time">${trade.time}</span>
            <span class="type">${carbonCredits[trade.type]?.name || trade.type}</span>
            <span class="side ${trade.side}">${trade.side.toUpperCase()}</span>
            <span class="price">$${trade.price.toFixed(2)}</span>
            <span class="amount">${trade.amount} CR</span>
            <span class="total">$${trade.total.toFixed(2)}</span>
        </div>
    `).join('');
}

// Sell position
function sellPosition(type) {
    const pos = positions.find(p => p.type === type);
    if (pos) {
        const credit = carbonCredits[type];
        const proceeds = pos.quantity * credit.price * 0.995; // Minus fee
        userBalance += proceeds;
        document.getElementById('usd-balance').textContent = userBalance.toFixed(2);
        
        tradeHistory.unshift({
            time: new Date().toLocaleTimeString(),
            type: type,
            side: 'sell',
            price: credit.price,
            amount: pos.quantity,
            total: proceeds
        });
        
        positions = positions.filter(p => p.type !== type);
        updatePositions();
        updateRecentTrades();
    }
}

// Event listeners
document.addEventListener('DOMContentLoaded', () => {
    initChart();
    updatePositions();
    updateRecentTrades();
    
    document.getElementById('trade-amount').addEventListener('input', updateTradeCalc);
    document.getElementById('trade-price').addEventListener('input', updateTradeCalc);
    document.getElementById('execute-trade').addEventListener('click', executeTrade);
    
    // Tab switching
    document.querySelectorAll('.tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            document.getElementById('execute-trade').textContent = 
                tab.dataset.tab === 'buy' ? 'Place Buy Order' : 'Place Sell Order';
        });
    });
});