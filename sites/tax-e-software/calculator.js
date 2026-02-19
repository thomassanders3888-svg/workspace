// Tax Calculator Engine - 2024 Tax Year

const TAX_BRACKETS_2024 = {
    single: [
        { limit: 11600, rate: 0.10 },
        { limit: 47150, rate: 0.12 },
        { limit: 100525, rate: 0.22 },
        { limit: 191950, rate: 0.24 },
        { limit: 243725, rate: 0.32 },
        { limit: 609350, rate: 0.35 },
        { limit: Infinity, rate: 0.37 }
    ],
    married: [
        { limit: 23200, rate: 0.10 },
        { limit: 94300, rate: 0.12 },
        { limit: 201050, rate: 0.22 },
        { limit: 383900, rate: 0.24 },
        { limit: 487450, rate: 0.32 },
        { limit: 731200, rate: 0.35 },
        { limit: Infinity, rate: 0.37 }
    ],
    head: [
        { limit: 16550, rate: 0.10 },
        { limit: 63100, rate: 0.12 },
        { limit: 100500, rate: 0.22 },
        { limit: 191950, rate: 0.24 },
        { limit: 243700, rate: 0.32 },
        { limit: 609350, rate: 0.35 },
        { limit: Infinity, rate: 0.37 }
    ]
};

const STANDARD_DEDUCTION_2024 = {
    single: 14600,
    married: 29200,
    head: 21900
};

class TaxCalculator {
    calculateFederalTax(income, deductions, credits, filingStatus) {
        const standardDeduction = STANDARD_DEDUCTION_2024[filingStatus] || STANDARD_DEDUCTION_2024.single;
        const totalDeductions = Math.max(deductions, standardDeduction);
        const taxableIncome = Math.max(0, income - totalDeductions);
        
        const brackets = TAX_BRACKETS_2024[filingStatus] || TAX_BRACKETS_2024.single;
        let tax = 0;
        let previousLimit = 0;
        
        for (const bracket of brackets) {
            if (taxableIncome > bracket.limit) {
                tax += (bracket.limit - previousLimit) * bracket.rate;
                previousLimit = bracket.limit;
            } else {
                tax += Math.max(0, taxableIncome - previousLimit) * bracket.rate;
                break;
            }
        }
        
        const finalTax = Math.max(0, tax - (credits || 0));
        
        return {
            grossIncome: income,
            deductions: totalDeductions,
            taxableIncome: taxableIncome,
            federalTax: finalTax,
            effectiveRate: taxableIncome > 0 ? (finalTax / taxableIncome * 100).toFixed(2) : 0,
            takeHome: income - finalTax
        };
    }
    
    calculateSelfEmploymentTax(netIncome) {
        const seTaxable = netIncome * 0.9235;
        const socialSecurityCap = 168600;
        const socialSecurityRate = 0.124;
        const medicareRate = 0.029;
        
        let ssTax = Math.min(seTaxable, socialSecurityCap) * socialSecurityRate;
        let medicareTax = seTaxable * medicareRate;
        
        if (netIncome > 200000) {
            medicareTax += (netIncome - 200000) * 0.009;
        }
        
        const totalSETax = ssTax + medicareTax;
        const deductible = totalSETax * 0.5;
        
        return {
            total: totalSETax.toFixed(2),
            deductible: deductible.toFixed(2),
            net: (totalSETax - deductible).toFixed(2)
        };
    }
    
    estimateQuarterlyTaxes(annualIncome, filingStatus) {
        const result = this.calculateFederalTax(annualIncome, 0, 0, filingStatus);
        const quarterly = result.federalTax / 4;
        
        return {
            annual: result.federalTax.toFixed(2),
            quarterly: quarterly.toFixed(2),
            dueDates: ['April 15', 'June 15', 'September 15', 'January 15'],
            safeHarbor: (annualIncome * 0.25).toFixed(2)
        };
    }
    
    calculateStateTax(income, state) {
        const stateRates = {
            CA: { rate: 0.093, name: 'California' },
            NY: { rate: 0.0685, name: 'New York' },
            TX: { rate: 0, name: 'Texas (No income tax)' },
            FL: { rate: 0, name: 'Florida (No income tax)' },
            WA: { rate: 0, name: 'Washington (No income tax)' },
            default: { rate: 0.05, name: 'Average State' }
        };
        
        const stateInfo = stateRates[state] || stateRates.default;
        const tax = income * stateInfo.rate;
        
        return {
            state: stateInfo.name,
            rate: (stateInfo.rate * 100).toFixed(2),
            tax: tax.toFixed(2)
        };
    }
}

// Export for browser and Node.js
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TaxCalculator;
}

// Initialize on page load
if (typeof window !== 'undefined') {
    window.TaxCalculator = TaxCalculator;
}