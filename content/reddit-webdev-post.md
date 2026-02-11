# I built 8 websites in 30 days as a side experiment — here's what happened

**TL;DR:** Navy nuclear instructor with no web dev background built 8 sites in 30 days using vanilla JS. Traffic is modest but growing. Here's my honest build log.

---

## Who I am

I'm a Navy nuclear instructor (yes, actual nuclear, not startup-speak). Background in Mechanical & Energy Engineering. I joined the Navy after undergrad because I wanted something more applied while keeping technical.

I've always been curious about web development but never had time to dive deep. So I gave myself a 30-day challenge: build and ship 8 complete websites using **only vanilla JS** — no frameworks, no build tools, just `index.html`, `styles.css`, and `script.js`.

---

## The stack (kept it simple)

- **Frontend:** Vanilla HTML/CSS/JS (no React, no Vue, no build step)
- **Hosting:** GitHub Pages (free)
- **CDN/Security:** Cloudflare (free tier)
- **Domains:** Namecheap/Cloudflare Registrar
- **Payment:** PayPal integration on one site

Why vanilla? Two reasons:
1. I wanted to actually *learn* how the web works, not cargo-cult framework patterns
2. Faster iteration — no `npm install`, no build times, just edit and refresh

---

## The 8 sites (in build order)

### 1. Nuclear Energy Brief
My first attempt. Clean landing page explaining nuclear energy basics. Built it mainly to test the workflow.
- **Live:** [thomassanders3888-svg.github.io/nuclear-energy-brief](https://thomassanders3888-svg.github.io/nuclear-energy-brief/)
- **What I learned:** GitHub Pages deploy is actually painless. Just push to `/docs` folder and enable Pages.

### 2. ResumePro
Simple resume builder tool. Takes a form input and generates a clean, printable resume with toggle for ATS-friendly vs design-focused templates.
- **Features:** JSON export/import, live preview, print-to-PDF
- **Tech notes:** Learned about `window.print()` and CSS `@media print` styling
- **Status:** Functional MVP, working on template variety

### 3. AI Tools Directory
Curated directory of AI tools categorized by use case (writing, coding, image gen, etc.) with user ratings and affiliate links where applicable.
- **Features:** Search/filter, category sidebar, mobile responsive
- **Data:** Static JSON for now, considering Airtable for crowdsourced submissions
- **Early numbers:** ~50 daily visitors from organic search

### 4. GameHub
A collection of browser-based games (Tetris, Snake, 2048, Wordle clone). Integrated PayPal for "support the dev" voluntary payments.
- **Games:** 6 games total, all canvas-based vanilla JS
- **Monetization:** PayPal donate button + option to remove ads for $2.99
- **Lesson:** Game loop logic in JS is more fun than I expected. Collision detection is pain.

### 5. Navy Study Guides Repository
Unofficial study resource hub for Navy nuclear power school students. Organized by rate/rating with flashcards and practice exams.
- **Auth:** Simple localStorage "bookmark" system — no login required
- **Why this matters:** If even one sailor learns something and gets qualified faster, worth it

### 6. Engineering Calculators Portal
Basic calculators for thermodynamics, fluid mechanics, and stress analysis. Think "engineer's tool belt in the browser."
- **Tech:** ES6 modules for calculator logic separation
- **Future:** Unit converters with actual engineering constants (not just "2.54 cm/in")

### 7. Minimalist Weather Dashboard
Clean widget-style dashboard pulling from OpenWeatherMap API. Shows current + 5-day for saved locations.
- **API handling:** Learned async/await properly by building this
- **Styling:** Glassmorphism attempt for the learning experience

### 8. Personal Portfolio (this one)
You're looking at the result. Aggregates all projects with live demos and GitHub links.

---

## Lessons learned (the real stuff)

**Vanilla JS isn't dead, especially for small projects.**

I kept waiting to hit a wall where I'd "need" a framework. It never came. For sites under 10 pages, vanilla is actually *faster* to develop once you have a pattern down.

**GitHub Pages + Cloudflare is surprisingly capable.**

Free hosting that handles decent traffic, auto-HTTPS, and Cloudflare's caching makes them feel snappy. I thought I'd need to upgrade to Netlify/Vercel Pro. Haven't needed to yet.

**Mobile-first isn't optional.**

I started #1 desktop-first and paid for it. By #3, I was writing mobile styles first and life got better. The "pop open DevTools, toggle device toolbar" workflow is muscle memory now.

**SEO is 80% just doing the basics right.**

Proper `<title>` and `<meta description>`, semantic HTML tags, fast load times. The AI Tools Directory ranks for "AI writing tools" already with zero link building — just clean markup and relevant content.

**Payment integration is easier than I feared.**

PayPal's Smart Payment Buttons took 20 minutes to set up. The hard part isn't the code — it's convincing yourself to ship something that asks for money.

---

## Traffic & Revenue (month 1, totally honest)

| Metric | Number |
|--------|--------|
| Total unique visitors | ~1,200 |
| Peak daily (AI Tools launch) | ~180 |
| PayPal donations (GameHub) | 3 × $2.99 |
| Affiliate clicks (AI Tools) | ~40 |
| Revenue | ~$15 USD |

Not quitting my day job anytime soon. But:
- Traffic is *growing* — AI Tools doubled week-over-week
- GameHub has surprisingly high session time (avg 8 min)
- ResumePro got shared organically in a Discord I'm in

The goal was learning, not money. The fact that strangers are visiting at all feels surreal.

---

## What I'd do differently

1. **Started analytics sooner.** Only added Plausible in week 3. Lost early data.
2. **Built a component system earlier.** Copied/pasted navbars across 8 sites like a caveman before I templated the header/footer.
3. **Used a base template from day 1.** Site #1 looks rough compared to #8. Consistent starting point would have helped.

---

## What's next?

- **AI Tools Directory:** User submissions (likely via Airtable/Zapier), newsletter for new tools
- **GameHub:** Multiplayer Snake using WebSockets (learning excercise)
- **ResumePro:** Actually charge money — currently 100% free

Also planning to document this journey more. Maybe some "build a game in 1 hour" posts or deep dives into specific challenges (canvas animation, SEO for tool directories, etc.)

---

## The code

Everything is on GitHub. Not the cleanest, but it works and it's public. Happy to share specific repos if anyone wants to see the vanilla JS patterns I landed on.

**What should I prioritize next?** The AI Tools Directory has the most traction, but GameHub is the most fun to work on. Also considering whether vanilla still makes sense if I add user accounts (leaning toward keeping it simple with localStorage + JSON for longer than I should).

Open to questions, feedback, or roasting my CSS. Thanks for reading.

---

**Edit:** Formatting fixes, added GameHub details
**Edit 2:** Added revenue numbers because "show your work" and all that
