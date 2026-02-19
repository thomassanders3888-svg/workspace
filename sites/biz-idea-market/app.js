// Business Idea Marketplace

const ideas = [
    {
        id: 1,
        title: "AI Customer Support Agent",
        category: "ai",
        price: 2500,
        status: "for-sale",
        validation: "MRR $500",
        description: "AI-powered chatbot trained on company docs. Reduces support tickets by 60%.",
        metrics: {
            traction: "15 beta users",
            market: "$15B support software market",
            mvp: "Complete"
        },
        founder: "Looking for: Sales co-founder",
        tags: ["SaaS", "B2B", "AI"]
    },
    {
        id: 2,
        title: "Freelancer Payment Protection",
        category: "saas",
        price: 5000,
        status: "for-sale",
        validation: "50 signups waitlist",
        description: "Escrow + milestone tracking for freelancers. Like Safe for independent work.",
        metrics: {
            traction: "50 waitlist",
            market: "$455B freelance economy",
            mvp: "Prototype"
        },
        founder: "Looking for: Tech co-founder",
        tags: ["Fintech", "Marketplace"]
    },
    {
        id: 3,
        title: "Carbon Credit Exchange",
        category: "marketplace",
        price: 15000,
        status: "for-sale",
        validation: "2 LOIs",
        description: "P2P marketplace for verified carbon credits. Blockchain-verified.",
        metrics: {
            traction: "2 LOIs signed",
            market: "$50B carbon market",
            mvp: "In development"
        },
        founder: "Looking for: Funding",
        tags: ["Climate", "Fintech", "Web3"]
    },
    {
        id: 4,
        title: "Micro-SaaS Boilerplate",
        category: "saas",
        price: 299,
        status: "for-sale",
        validation: "127 sales",
        description: "Next.js boilerplate with auth, billing, DB. Save 40 hours per project.",
        metrics: {
            traction: "127 sales",
            market: "Dev tools market",
            mvp: "Complete"
        },
        founder: "Selling: Complete product",
        tags: ["Developer Tools", "Starter"]
    },
    {
        id: 5,
        title: "Restaurant Inventory AI",
        category: "ai",
        price: 8000,
        status: "for-sale",
        validation: "3 pilot restaurants",
        description: "Computer vision for tracking inventory. Reduces food waste by 30%.",
        metrics: {
            traction: "3 pilots",
            market: "$900B restaurant industry",
            mvp: "Beta"
        },
        founder: "Looking for: Growth lead",
        tags: ["AI", "Food", "B2B"]
    },
    {
        id: 6,
        title: "Solo Founder Community",
        category: "marketplace",
        price: 0,
        status: "cofounder-wanted",
        validation: "Not started",
        description: "Community platform for solo founders to find accountability partners.",
        metrics: {
            traction: "Not started",
            market: "1M+ indie hackers",
            mvp: "Concept"
        },
        founder: "Looking for: Technical co-founder",
        tags: ["Community", "Networking"]
    }
];

function renderIdeas(filter = 'all') {
    const container = document.getElementById('ideas-container');
    const filtered = filter === 'all' ? ideas : ideas.filter(i => i.category === filter);
    
    container.innerHTML = filtered.map(idea => `
        <div class="idea-card" data-id="${idea.id}">
            <div class="idea-header">
                <span class="category-badge">${idea.category.toUpperCase()}</span>
                <span class="status-badge status-${idea.status}">${formatStatus(idea.status)}</span>
            </div>
            <h3>${idea.title}</h3>
            <p class="idea-desc">${idea.description}</p>
            <div class="metrics">
                <div class="metric">
                    <span class="label">Traction</span>
                    <span class="value">${idea.metrics.traction}</span>
                </div>
                <div class="metric">
                    <span class="label">Market</span>
                    <span class="value">${idea.metrics.market}</span>
                </div>
            </div>
            <div class="tags">
                ${idea.tags.map(t => `<span class="tag">${t}</span>`).join('')}
            </