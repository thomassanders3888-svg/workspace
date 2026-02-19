// AI Governance Tracker - Real-time regulation monitoring

const regulations = [
    {
        id: 1,
        title: "EU AI Act - High-Risk Systems",
        region: "eu",
        category: "safety",
        status: "active",
        effective: "2026-08-01",
        description: "Requirements for high-risk AI systems including risk management, data governance, and transparency obligations.",
        impact: "high",
        complianceCost: "$50K-$500K"
    },
    {
        id: 2,
        title: "US Executive Order 14110",
        region: "us", 
        category: "safety",
        status: "active",
        effective: "2024-02-01",
        description: "Requirements for developers of large AI models including safety testing and reporting.",
        impact: "high",
        complianceCost: "$100K-$1M"
    },
    {
        id: 3,
        title: "China AI Regulations - Deep Synthesis",
        region: "china",
        category: "privacy",
        status: "active",
        effective: "2023-01-10",
        description: "Mandatory watermarking and content labeling for AI-generated media.",
        impact: "medium",
        complianceCost: "$10K-$50K"
    },
    {
        id: 4,
        title: "UK AI Safety Framework",
        region: "uk",
        category: "safety",
        status: "pending",
        effective: "2026-12-01",
        description: "Proposed requirements for frontier AI model safety assessments.",
        impact: "high",
        complianceCost: "TBD"
    },
    {
        id: 5,
        title: "NYC Local Law 144",
        region: "us",
        category: "liability",
        status: "active",
        effective: "2023-07-05",
        description: "Bias audit requirements for automated employment decision tools.",
        impact: "medium",
        complianceCost: "$5K-$30K"
    }
];

class GovernanceTracker {
    constructor() {
        this.data = regulations;
        this.init();
    }

    init() {
        this.renderTimelineChart();
        this.renderRegulations();
        this.setupFilters();
        this.updateStats();
    }

    renderTimelineChart() {
        const ctx = document.getElementById('timelineChart').getContext('2d');
        
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
        const newRegs = [3, 5, 8, 12, 15, 19];
        const updates = [8, 12, 15, 18, 22, 28];

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: months,
                datasets: [{
                    label: 'New Regulations',
                    data: newRegs,
                    borderColor: '#667eea',
                    tension: 0.4
                }, {
                    label: 'Updates/Revisions',
                    data: updates,
                    borderColor: '#fbbf24',
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position