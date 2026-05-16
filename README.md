# 🎰 Slot Machine — Unity Game

A polished, feature-complete slot machine built in Unity with smooth reel animations, weighted RNG, wild symbols, and a jackpot celebration system.

---

## 🕹️ Game Overview

A classic 3-reel slot machine with 4 symbol types plus a Wild bonus symbol.
- **Win Condition:** All 3 reels show the same symbol (Wild substitutes any).
- **Jackpot:** Three `7` symbols → 50× your bet.
- **Lever Pull:** Click the lever on the right of the machine for an alternative spin trigger.
- **Paytable Button:** Tap `?` to view all win multipliers at any time.

---

## ▶️ How to Run the WebGL Build

1. Clone this repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/slot-machine-unity.git
   ```
2. Navigate to the `/Build/WebGL/` folder.
3. Open `index.html` in a **local web server** (WebGL requires one):
   - **VS Code:** Install the *Live Server* extension → right-click `index.html` → *Open with Live Server*
   - **Python:** `python -m http.server 8080` then open `http://localhost:8080/Build/WebGL/`
   - **Node:** `npx serve Build/WebGL`

> ⚠️ Opening `index.html` directly via `file://` will fail due to browser CORS restrictions on WebGL builds.

---

## 🎮 Controls

| Action | Input |
|---|---|
| Spin | Click **SPIN** button or pull the **Lever** |
| View Paytable | Click the **?** button |
| Close Popup | Click the **✕** on the popup |

---

## 💰 Paytable

| Symbol | Combination | Multiplier |
|---|---|---|
| 🎰 Seven | 7 · 7 · 7 | **×50 JACKPOT** |
| 🃏 Wild | W · W · W | ×20 |
| BAR | BAR · BAR · BAR | ×10 |
| 🔔 Bell | 🔔 · 🔔 · 🔔 | ×5 |
| 🍒 Cherry | 🍒 · 🍒 · 🍒 | ×3 |

Wild substitutes for **any** symbol. A reel showing Wild counts as whichever symbol completes the line.

---

## ✨ Bonus Features

### 1. Animated Pull Lever
A fully animated lever on the side of the machine with a spring-physics return animation.
- Pull down phase uses `EaseInQuad`.
- Return uses `EaseOutExpo` with an overshoot for a spring feel.
- Fires the spin at the bottom of the stroke (just like a real machine).

### 2. Wild Symbol System
Symbol index 4 is a **Wild** that substitutes for any other symbol. The `WildResolver` utility resolves wilds before the win check — keeping win-evaluation logic clean and separation of concerns intact.

### 3. Jackpot Celebration
Three Sevens triggers:
- Gold screen flash (`JackpotEffect`)
- Confetti particle system
- Frame shake animation
- Dedicated jackpot audio cue

### 4. Credit Roll Animation
The credit counter animates to its new value using `EaseOutCubic` — satisfying whether you win or lose.

### 5. Weighted RNG
`RNGService` uses a cumulative weighted distribution rather than uniform randomness — Cherry appears most often (~40% single-reel), Seven is rare (~5%). All weights are in one place and clearly labelled.

---

## 🧠 Thought Process & Architecture

### Separation of Concerns
Each class has one job:

| Class | Responsibility |
|---|---|
| `SlotMachineManager` | Orchestrates game flow, owns state |
| `RNGService` | Generates random outcomes (stateless) |
| `PayoutTable` | Payout data lookup (static data) |
| `WildResolver` | Wild substitution logic (stateless) |
| `ReelController` | Visual reel animation (one per reel) |
| `UIController` | All HUD / popup updates |
| `AudioManager` | All sound playback (singleton) |
| `LeverController` | Lever animation + spin trigger |
| `JackpotEffect` | Win celebration FX |
| `CreditRollAnimator` | Animated credit counter |

### Animation Strategy
Reels use a **scrolling strip** pattern — a tall RectTransform with repeated symbol images scrolls downward. This avoids complex state machines and makes adding symbols trivial (just add more images to the strip).

Phases:
1. **Acceleration** — `EaseInQuad` from 0 → 2400 px/s (0.3 s)
2. **Full speed** — constant scroll
3. **Deceleration** — `EaseOutQuart` snap to target position (0.6 s)

Reels start/stop with a **staggered delay** (left first, right last) for the classic cascade feel.

### RNG Fairness
Outcome is determined **before** animation starts — the visual animation does not affect the result. This mirrors how real slot machines work and ensures the game is not gamble-able by timing inputs.

### Event Architecture
`SlotMachineManager` exposes `UnityEvent` hooks rather than direct references to the UI. This means the UI can be completely rebuilt or swapped without touching game logic.

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── SlotMachineManager.cs   ← Main game controller
│   │   ├── ReelController.cs       ← Per-reel animation
│   │   ├── RNGService.cs           ← Weighted random outcomes
│   │   ├── PayoutTable.cs          ← Win multipliers & SymbolType enum
│   │   ├── WildResolver.cs         ← Wild substitution
│   │   └── LeverController.cs      ← Lever pull animation
│   ├── UI/
│   │   └── UIController.cs         ← HUD, popups, paytable
│   ├── Audio/
│   │   └── AudioManager.cs         ← Singleton sound manager
│   └── Utils/
│       ├── CreditRollAnimator.cs   ← Animated credit counter
│       └── JackpotEffect.cs        ← Jackpot celebration FX
├── Prefabs/
│   ├── Reel.prefab                 ← Single reel with symbol strip
│   ├── SlotMachine.prefab          ← Full machine assembly
│   └── JackpotFX.prefab            ← Particle system
├── Animations/
│   ├── ReelSpin.anim
│   ├── LeverPull.anim
│   └── PopupIn.anim / PopupOut.anim
├── Sprites/
│   ├── slot-symbol1.png (Seven)
│   ├── slot-symbol2.png (Cherry)
│   ├── slot-symbol3.png (Bell)
│   ├── slot-symbol4.png (Bar)
│   ├── slot-machine1.png … slot-machine5.png
│   ├── slot_machine_Middle_box.png
│   ├── slot_machine_buttons-02/03/04.png
│   ├── bg_gradient.png
│   └── popup.png
├── Scenes/
│   └── MainGame.unity
└── Sounds/
    ├── spin_start.wav
    ├── reel_stop.wav
    ├── win.wav
    ├── jackpot.wav
    └── casino_loop.wav
Build/
└── WebGL/
    ├── index.html
    ├── Build/
    └── TemplateData/
```

---

## 🛠️ Unity Setup Notes

- **Unity Version:** 2022.3 LTS (URP or Built-in RP both work)
- **TextMeshPro:** Required — import via `Window → Package Manager`
- **Symbols per reel strip:** 8 images (the strip loops — 4 unique symbols × 2 repeats for seamless scrolling)
- **Canvas:** Screen Space — Overlay, Reference Resolution 1920×1080

---

## 🔄 Git Commit Strategy

Commits follow a feature-by-feature progression:
1. `init: project scaffold and folder structure`
2. `feat: import provided asset sprites and configure atlas`
3. `feat: slot machine background and frame assembly (UI)`
4. `feat: RNGService with weighted probability table`
5. `feat: PayoutTable and SymbolType definitions`
6. `feat: ReelController — scrolling strip with eased spin`
7. `feat: SlotMachineManager — staggered spin orchestration`
8. `feat: UIController — credits HUD and result popup`
9. `feat: LeverController — pull animation with spring return`
10. `feat: WildResolver — wild symbol substitution`
11. `feat: JackpotEffect — screen flash, shake, confetti`
12. `feat: AudioManager and placeholder audio clips`
13. `feat: CreditRollAnimator — animated counter on win/lose`
14. `fix: reel strip Y wrapping for long sessions`
15. `build: WebGL export to /Build/WebGL`

---

*Built with Unity 2022.3 LTS · Assets provided by assignment pack*
