// Tax Calculator Engine

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
    constructor() {
        this.year = 2024;
    }

    calculateTax(income, deductions, credits, filingStatus) {
        const taxableIncome = Math.max(0, income - deductions);
        const brackets = TAX_BRACKETS_2024[filingStatus];
        
        let tax = 0;
        let previousLimit = 0;
        
        for (const bracket of brackets) {
            if (taxableIncome > bracket.limit) {
                tax += (bracket.limit - previousLimit) * bracket.rate;
                previousLimit = bracket.limit;
            } else {
                tax += (taxableIncome - previousLimit) * bracket.rate;
                break;
            }
        }
        
        // Apply credits
        const finalTax = Math.max(0, tax - credits);
        
        return {
            grossIncome: income,
            deductions: deductions,
            taxableIncome: taxableIncome,
            federalTax: finalTax,
            effectiveRate: taxableIncome > 0 ? (finalTax / taxableIncome * 100).toFixed(2) : 0,
            takeHome: income - finalTax
        };
    }

    estimateSelfEmploymentTax(netIncome) {
        // 92.35% of net income subject to SE tax
        const taxableSE = netIncome * 0.9235;
        // 15.3% total (12.4% Social Security up to limit, 2.9% Medicare)
        const seTax = taxableSE * 0.153;
        // Deductible portion (50%)
        const deductible = seTax * 0.5;
        
        return {
            grossSE: seTax,
            deductible: deductible,
            netSE: seTax - deductible
        };
    }

    optimizeDeductions(income, itemizedDeductions, filingStatus) {
        const standard = STANDARD_DEDUCTION_2024[filingStatus];
        const optimal = Math.max(standard, itemizedDeductions);
        const savings = itemizedDeductions > standard ? 
            this.calculateTaxSavings(itemizedDeductions - standard, filingStatus) : 0;
        
        return {
            useStandard: itemizedDeductions <= standard,
            standardAmount: standard,
            itemizedAmount: itemizedDeductions,
            optimal: optimal,
            savings: savings
        };
    }

    calculateTaxSavings(additionalDeduction, filingStatus) {
        const brackets = TAX_BRACKETS_2024[filingStatus];
        // Simplified - use marginal rate
        for (const bracket of brackets) {
            if (additionalDeduction < bracket.limit) {
                return additionalDeduction * bracket.rate;
            }
        }
        return 0;
    }

    quarterlyEstimate(annualIncome, credits, filingStatus) {
        const result = this.calculateTax(annualIncome, 
            STANDARD_DEDUCTION_2024[filingStatus], credits, filingStatus);
        const quarterly = result.federalTax / 4;
        
        return {
            annual: result.federalTax,
            quarterly: quarterly,
            dueDates: ['Apr 15', 'Jun 15', 'Sep 15', 'Jan 15'],
            safeHarbor: annualIncome * 0.25 // Simplified safe harbor
        };
    }
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TaxCalculator;
}