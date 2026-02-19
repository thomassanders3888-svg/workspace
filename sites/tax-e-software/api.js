// Tax-E-Software API Layer
// Simulates backend endpoints for tax calculations

const TaxEAPI = {
    // Base URL for API calls
    baseUrl: '/api/v1',
    
    // Calculate federal tax
    async calculateFederal(data) {
        // Simulate API latency
        await new Promise(resolve => setTimeout(resolve, 500));
        
        const calc = new TaxCalculator();
        const result = calc.calculateFederalTax(
            data.income,
            data.deductions || 0,
            data.credits || 0,
            data.filingStatus || 'single'
        );
        
        return {
            status: 'success',
            data: result,
            timestamp: new Date().toISOString()
        };
    },
    
    // Calculate self-employment tax
    async calculateSETax(netIncome) {
        await new Promise(resolve => setTimeout(resolve, 300));
        
        const calc = new TaxCalculator();
        const result = calc.calculateSelfEmploymentTax(netIncome);
        
        return {
            status: 'success',
            data: result
        };
    },
    
    // Get quarterly estimates
    async getQuarterlyEstimates(income, filingStatus) {
        const calc = new TaxCalculator();
        const result = calc.estimateQuarterlyTaxes(income, filingStatus);
        
        return {
            status: 'success',
            data: result
        };
    },
    
    // Save calculation to user history
    async saveCalculation(calculation) {
        const history = JSON.parse(localStorage.getItem('taxCalculations') || '[]');
        history.push({
            ...calculation,
            id: Date.now(),
            timestamp: new Date().toISOString()
        });
        localStorage.setItem('taxCalculations', JSON.stringify(history));
        
        return { status: 'success', message: 'Saved to history' };
    },
    
    // Get calculation history
    async getHistory() {
        const history = JSON.parse(localStorage.getItem('taxCalculations') || '[]');
        return { status: 'success', data: history };
    },
    
    // Export calculation as PDF (simulated)
    async exportPDF(data) {
        console.log('Exporting to PDF:', data);
        return { status: 'success', downloadUrl: '#' };
    },
    
    // Get tax brackets for year
    async getTaxBrackets(year = 2024) {
        return {
            status: 'success',
            data: TAX_BRACKETS_2024
        };
    },
    
    // Compare scenarios
    async compareScenarios(scenarios) {
        const calc = new TaxCalculator();
        const results = scenarios.map(s => ({
            name: s.name,
            result: calc.calculateFederalTax(s.income, s.deductions, s.credits, s.filingStatus)
        }));
        
        return { status: 'success', data: results };
    }
};

// Payment Processing Integration
const PaymentAPI = {
    // Initialize PayPal payment
    async initPayPal(container, orderData) {
        if (typeof paypal === 'undefined') {
            console.error('PayPal SDK not loaded');
            return;
        }
        
        paypal.Buttons({
            createOrder: (