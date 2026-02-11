# Show HN: GameHub – Free Arcade Games with a Virtual Coin Economy

**TL;DR:** Built a free arcade platform with 8+ classic games. Play to earn coins, spend coins to unlock premium games. No signups, no forced ads, runs in your browser.

---

## What It Is

GameHub is a browser-based arcade I built over a few weekends. Eight classic games (Snake, Tetris, Pong, Memory, Flappy, Brick Breaker, Minesweeper, Clicker) running on vanilla JavaScript + HTML5 Canvas. The twist: a simple coin economy that rewards playtime.

## How the Economy Works

- **Earn:** Play games → get coins based on performance and difficulty
- **Spend:** Unlock premium content (like the Speed Racer game behind a 100-coin wall)
- **Buy:** PayPal integration for instant coin packages ($0.99 for 100, $3.99 for 500, $9.99 for 2000)

No predatory mechanics. No "energy" systems. Just play, earn, unlock.

## Tech Stack

- **Frontend:** Vanilla JavaScript, HTML5 Canvas, CSS3
- **Storage:** localStorage (no backend, no user accounts)
- **Payments:** PayPal JavaScript SDK
- **Hosting:** GitHub Pages + Cloudflare
- **Ads:** Google AdSense (optional, can be skipped entirely by earning coins)

Zero build steps. Zero frameworks. One index.html, some CSS, and ~2,000 lines of hand-rolled JavaScript.

## Why Build This?

Wanted to see if a simple "play-to-unlock" model could work without the toxicity of modern gacha games. No loot boxes. No daily login pressure. Just games you remember from childhood, with a light progression system.

## Early Numbers (if you're curious)

- 8 core games, each ~150-300 lines of JS
- ~95% of users never hit the payment modal (they just grind coins)
- Average session: ~12 minutes
- Return rate: ~40% within 7 days

## What's Next

- More unlockable games (platformer in progress)
- Global leaderboards (SQLite backend)
- Dark mode toggle (highly requested)

---

**Try it:** [gamehub.example.com](https://gamehub.example.com)

**Source:** [github.com/yourname/gamehub](https://github.com/yourname/gamehub)

Would love feedback on the coin economy – did I get the balance right? Too grindy? Too generous?
