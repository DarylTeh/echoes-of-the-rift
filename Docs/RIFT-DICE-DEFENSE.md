# Rift Defense — skill-book tower mode

Revision 66, 4 October 2026.

## Co-op gameplay reference study

Before this iteration's build work, 20 distinct Random Dice co-op recordings were opened in the browser and inspected during match footage (usually around 0:30–1:30; long-run recordings show later wave states too). This sample intentionally includes beginners, deck guides, long-wave farming, support/control, gear tests, challenge/co-op comparisons and viewer matches. It is a qualitative UI and loop review, not a frame-by-frame or balance-data extraction.

1. [Time + Solar co-op](https://www.youtube.com/watch?v=6qKKIpLH_R0) — two player boards, wave and SP controls.
2. [Co-op wave 4976](https://www.youtube.com/watch?v=GV1EFlaL7Wo) — compact board state remains readable in a very long run.
3. [Combo + Alignment co-op](https://www.youtube.com/watch?v=o3IORBsfeB4) — randomized board assembly and co-op play.
4. [Soul / Sword / Snow / Scope wave 2500+](https://www.youtube.com/watch?v=vQ9z11Fa4as) — late-wave builds and high-effect density.
5. [Holy Sword + Lunar + Scope co-op](https://www.youtube.com/watch?v=Hk53sFM9Ido) — two simultaneous boards, deck roles and a shared wave HUD.
6. [Co-op wave 5138](https://www.youtube.com/watch?v=Vl-SlN-SEis) — evolving board and bottom summon/upgrade controls.
7. [NimbleThor multiplayer/co-op overview](https://www.youtube.com/watch?v=39mCCKHE-00) — paired boards, wave pressure and summon economy.
8. [DarkDream wave 1000+ challenge](https://www.youtube.com/watch?v=45w0PCPk0oE) — accelerated late-wave co-op and boss pressure.
9. [Co-op wave 20630](https://www.youtube.com/watch?v=AcXYvquLReE) — high-wave state and compact progression strip.
10. [1 to 1050 co-op setup guide](https://www.youtube.com/watch?v=RPt2VliiPwg) — build setup and high-wave play.
11. [No-legendary beginner co-op, wave 70](https://www.youtube.com/watch?v=zkIlMsNxdFM) — early progression and a starter-friendly board.
12. [Viewer co-op stream](https://www.youtube.com/watch?v=JuhXq7hwiEw) — live-match presentation and viewer interaction.
13. [Monostortion co-op](https://www.youtube.com/watch?v=5xYT2qeDAW4) — board growth, positioning and late-run coordination.
14. [Gear Dice co-op test](https://www.youtube.com/watch?v=mh77GTshDJk) — gear interaction within normal board play.
15. [Holy Sword + Medusa co-op guide](https://www.youtube.com/watch?v=v0xJZxRoJ2Q) — high-wave paired boards.
16. [Recharge co-op stream](https://www.youtube.com/watch?v=W5h40lHbl9M) — evolving build and long-session interface.
17. [Medusa Rage critical co-op guide](https://www.youtube.com/watch?v=80DFXz-dh-M) — side-by-side player board progression.
18. [Beginner co-op deck](https://www.youtube.com/watch?v=VEJ9Z1oeyI0) — beginner board and readable deck explanation.
19. [Control co-op deck](https://www.youtube.com/watch?v=3j6Yxu2GJv8) — paired grids and crowd-control role.
20. [Current-meta wave 500 co-op](https://www.youtube.com/watch?v=NuOnE7peeDM) — meta setup, support role and late-wave play.

Repeated patterns from the footage: paired, equal-weight boards stay visible at once; wave status and match currency are always near the top; the player repeatedly summons and merges under pressure; board occupancy and dice face/rank communicate build state; action buttons and progression remain grouped below the board; special dice create distinct support, control, economy and damage roles. Co-op is about complementary builds as much as raw damage. High-wave videos speed up or compress downtime, but the actual board and action states remain legible. We should borrow these interaction patterns and pacing cues while using Echoes' spell-book fiction, original art and its own UI assets.

Rift Defense is an optional tower-defense mode built around the existing skill-book collection. It should feel like Echoes of the Rift from the first tap: the same books, elements, names, neon effects and bosses appear in a new tactical format, but the rules are separated from campaign combat so the mode cannot destabilize normal progression.

## Why this mode fits

Random Dice shows a strong mobile loop: players bring a small deck, summon random towers from that deck, earn resources by defeating waves, and merge matching towers to create stronger board states. Its official store description also emphasizes PvP, co-op, solo, ranked and rotating modes. [Random Dice on Google Play](https://play.google.com/store/apps/details?hl=en-CA&id=com.percent.royaldice) and [dice mechanics reference](https://random-dice.fandom.com/wiki/Dice_Mechanics)

The Clash of Critters ads point toward a simple, readable lane-defense fantasy: place colorful units on a path, watch the wave advance, and make upgrades before the next threat. The game itself should deliver the advertised interaction honestly; the mode must be a real part of Echoes of the Rift rather than a disconnected ad-only mockup.

## Player promise

> Build a spellbook deck, defend the Rift gate, merge under pressure, and make one clever counter before the boss reaches the core.

The first version is solo, 20 waves and a compact 5x3 placement board; its duration remains unmeasured. The co-op samples informed the board size, random summon/merge loop and elite-wave rhythm, while the first implementation stays solo until its rules can be tested locally. Add the paired second board and shared run after the solo rules are stable. Do not start with PvP; random outcomes and network latency make balance and player trust harder before the core is proven.

## Revision 66 implementation state

The solo client slice is wired to the North / Raids gate. Its portrait layout has a compact book strip, a 5x3 playable board, a second visibly inactive co-op board, enemies entering from the left toward core labels on the right, and grouped summon/upgrade/merge/burst actions at the bottom. Owned books and current class skills fill the starter deck. Same-rank merges roll a random deck book at the next rank. Runner, armored and swarm units vary speed and health. Waves 5, 10, 15 and 20 each introduce an elite; the final boss is Ashen Gatekeeper. Rift Mana and match state remain local and isolated from the profile. Tier color pulses appear around occupied tower borders.

This iteration was built after inspecting 20 distinct co-op match recordings, listed above. The first pass adopts their paired-board-friendly dimensions, random build decisions, merge pressure, persistent wave/economy cues and elite-wave cadence. This remains a solo run: actual two-player shared-wave synchronization, partner board, support casts and server-authoritative actions are future work. Bosses currently differ in identity/presentation and the Cinder Knight's pace, but the other three do not yet have their full planned mechanics.

Validation: the Unity editor build passed (`Logs/step-10-20261004-213203.log`). The portrait Windows development player completed its 20-wave stress test with four elite bosses, 100% core, 15 test towers, and no runtime errors (`Logs/RiftDefenseRegression/player.log`). The visible showcase launch switch is `-cookieSmoke -showRiftDefense`; its capture is `Logs/RiftDefensePreview/rift-defense-preview.png`. The smoke setup maxes every tower and does not prove human balance, progression, or a normal-run clear. Shared-board networking, normal-run duration/balance and Android/low-end performance remain unmeasured.

This is not release-ready. Server-authoritative seeds, actions, reconnects and reward receipts are still required before rewards are enabled. Do not describe the 6–10-minute target as measured until a normal run has been timed on target devices.

## Core match loop

1. Select five owned skill books and one hero passive. The deck has a role budget: no more than two economy/control books and at least one damage source.
2. Start with a small amount of **Rift Mana**, a mode-only run resource that is never sold, purchased or converted into gems.
3. Summon a random book from the selected deck onto an empty grid cell. The card preview shows its target rule, element, range and next upgrade.
4. Towers attack automatically. The player can drag a tower to reposition it, spend Mana to upgrade its base power, or merge two matching board towers.
5. A merge increases the tower dot rank and grants a controlled mutation from the chosen deck. The result is not an uncontrolled power jump: the server records the seed, available deck and merge result.
6. Every fifth wave offers a choice of three temporary mutations, such as chain range, burn duration, slow strength, summon count or economy rate. Only one can be chosen.
7. The player still controls one hero ability or emergency cast. It is the bridge back to the action RPG and prevents the mode from becoming a passive screen saver.
8. Clear the boss, extract, and receive a server-verified reward summary. On defeat, keep a small participation reward and expose the counter that would have stopped the leak.

## Skill-book tower roles

| Role | Existing-book mapping | Tower behavior |
|---|---|---|
| Direct damage | Dark Strike, Arrow Wave, Fire Base | Single-target projectile with different range and target rules |
| Area damage | Crimson Cyclone, Solar Hammers | Burst or orbit that clears clustered enemies |
| Control | Frost or future slow books | Slow, root, knockback or path mark with diminishing returns |
| Beam/pierce | Astral Lance, Tempest Volley | Line attack that rewards lane geometry |
| Support | Therapy and future support books | Heal the gate, buff adjacent towers or reduce cooldowns |
| Economy | A future resource book, never a premium currency | Generates Mana only when its condition is met |
| Summon | Future companion or familiar books | Creates temporary blockers or interceptors |

Book level, rarity stars and visual glow should carry over as presentation. The defense mode uses a separate `TowerPower` coefficient and wave budget so an infinitely enhanced campaign item cannot trivialize every tower-defense season. Skill ownership and upgrade progression remain meaningful, but the match still depends on deck composition and decisions.

## Enemy and boss rules

The first enemy set should teach counters rather than inflate health:

- **Runner:** fast, low health; rewards early single-target damage.
- **Shellback:** armor that weakens repeated small hits; asks for pierce or burst.
- **Swarm:** many weak units; asks for area damage.
- **Wisp:** briefly untargetable; asks for timing or a control effect.
- **Splitter:** divides on defeat; punishes careless splash placement.
- **Leech:** drains gate health; forces a priority decision.

Bosses should change the board, not only the number above their head:

- **Mire Matron:** leaves slowing pools on occupied cells.
- **Clockwork Colossus:** disables one lane for a visible countdown.
- **Rift Mirror:** copies the player’s highest-rank tower for one wave.
- **Ashen Gatekeeper:** shields a different body segment each phase.

Every boss needs a readable warning, an answer, a recovery window and a signature cosmetic or title. Boss modifiers are data-driven and can be reused by the event system.

## Modes to unlock in order

1. **Solo Defense:** fixed maps and deterministic seeded enemy schedules for learning.
2. **Daily Rift:** one rotating mutator and a server-seeded board; leaderboard is score, wave and damage with caps.
3. **Co-op Defense:** two players share a core but have separate grids and can send one support cast to each other per wave.
4. **Draft Defense:** each player receives a temporary pool and chooses five books; owned power is normalized.
5. **Seasonal Defense:** event manifest controls map, bosses, mutations and cosmetic rewards.
6. **PvP Defense:** only after telemetry shows that the core is readable and fair. Use mirrored schedules and normalized tower power, not pay-to-win book levels.

## Rewards and economy

- Give gold, ordinary gear, book fragments and cosmetic tickets through the existing server reward path.
- Give a first-clear cosmetic, title or profile frame for memorable milestones.
- Use a daily participation cap and an account-level event receipt to prevent farming loops.
- Do not create a new premium currency for this mode.
- Do not make the mode the fastest source of gems or the mandatory path for campaign power.
- Show the reward source, pity or guarantee rules and the reason a run failed.

## UI and performance

The mobile layout uses a clear top wave/core bar, a compact deck strip, visible Rift Mana, a 5x3 board and a bottom-right hero cast. Co-op should preserve both equal-sized boards simultaneously, with wave/core/match currency in shared top chrome and controls grouped below each board. Towers use the same curved dark panels and neon border particles as the inventory. A long press opens a skill-book inspection card; dragging is the primary placement and merge gesture, with keyboard/controller alternatives on PC.

Use pooled projectiles and hit effects, an upper effect budget, sprite atlases and deterministic simulation ticks. The defense board must not spawn one GameObject per projectile. The low-end preset should reduce decorative particles while preserving path hazards, boss telegraphs, tower targeting and damage numbers.

## Server contract

The server owns deck validation, seed, wave schedule, tower placement, Mana, merges, mutations, boss state, score, rewards and receipts. The client submits intent (`summon`, `move`, `merge`, `upgrade`, `cast`, `extract`), never final damage or reward amounts. Store:

- `modeRunId`, account, ruleset version and content/balance version;
- deck book IDs, seed and deterministic action sequence;
- tower cells, dot ranks, mutations and Mana transactions;
- wave/boss progress, damage and time with rate limits;
- completion receipt and reward result before acknowledging the client.

This supports reconnect recovery, replay review, leaderboard validation, compensation and balance rollback. Add an event type such as `tower_defense` so the current live-event scheduler can activate a seasonal board without a client update.

## Acceptance gates

- A new player understands summon, merge and one counter within the first three waves.
- Every enemy has a readable counter and every boss phase has a readable tell.
- A run ends in 6–10 minutes on the target device; no runaway wave or unbounded effect count.
- Reconnecting resumes from the last committed action or safely ends with an inbox reward.
- Duplicate taps, reordered intents, invalid cells, forged scores and duplicate claims do not create rewards.
- Solo and co-op boards remain fair across low and high book collections.
- At least three decks are viable in the first balance simulation; no single book is mandatory.
- Rewards remain secondary to the campaign economy and do not make the mode compulsory.
