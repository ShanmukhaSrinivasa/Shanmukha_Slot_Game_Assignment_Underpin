# 🎰 2D Slot Machine Game - Technical Assessment

A fully playable 2D slot machine built in Unity (URP 2D), designed to showcase clean Object-Oriented architecture, data-driven design, and polished "Game Feel" (Juice).

## 🎮 Game Overview
This project simulates a classic mechanical slot machine. Players can choose varying bet amounts (10G, 50G, 100G) and pull the lever to spin the reels. The game evaluates a 30% win probability using Unity's RNG, with payouts dynamically calculated based on the specific winning symbol's multiplier.

## 🚀 How to Play (WebGL)
A WebGL build is included in this repository.
1. Navigate to the `/Build/WebGL/` folder.
2. Run the `index.html` file using a local server (or view it via GitHub Pages if hosted).
3. Select a bet amount from the right-hand panel.
4. The lever will pull automatically, and the reels will spin!

## 🧠 System Architecture & Thought Process
My primary goal was to ensure the core game math was entirely decoupled from the visual presentation, preventing hardcoded dependencies.

1. **Data-Driven Symbols (ScriptableObjects):** Instead of hardcoding payout multipliers, I utilized `ScriptableObjects` for `SlotSymbol` data. This allows designers to easily add new symbols, swap sprites, or adjust payout math directly in the Inspector without touching the C# scripts.
2. **The Visual Illusion (RectMask2D & Lerp):**
   The `ReelController` visually moves the symbols linearly, wrapping them instantly to the top when they cross a Y-axis threshold. To achieve a realistic, mechanical stop, the script transitions from a linear speed into a smooth `Mathf.Lerp`, guaranteeing a pixel-perfect snap to `Y = 0` every time.
3. **Outcome Pre-Calculation:**
   The `GameManager` rolls the RNG (30% win chance) the exact millisecond the bet is placed. The visual spinning is purely theater. A Coroutine waits for the longest visual spin to conclude before executing the payout sequence.

## ✨ "Game Feel" & Polish (Bonus Features)
To elevate the player experience beyond the basic requirements, I implemented several creative additions:
* **The "Casino" Payout Loop:** When a player wins, a `Mathf.Lerp` Coroutine creates a rapidly ticking balance counter, paired with a custom 2D Particle System "Coin Fountain" to maximize visual reward.
* **Dynamic Audio States:** Implemented looping mechanical whirring during the spin phase, synchronized 'thuds' as each reel snaps into place, and one-shot Victory/Loss fanfares.
* **Staggered Reel Stops:** Reels stop sequentially (Left to Right) with specific time delays (0s, 1.0s, 2.0s), mimicking real mechanical tension.
* **Auto-Spin Flow:** Clicking a bet amount automatically hides the UI menu and triggers the animated lever pull, keeping the player in the action.
