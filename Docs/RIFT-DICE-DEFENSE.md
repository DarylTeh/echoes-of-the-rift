# Rift Defense — skill-book tower mode

Revision 33, 28 September 2026.

Rift Defense is an optional tower-defense mode built around the existing skill-book collection. It should feel like Echoes of the Rift from the first tap: the same books, elements, names, neon effects and bosses appear in a new tactical format, but the rules are separated from campaign combat so the mode cannot destabilize normal progression.

## Why this mode fits

Random Dice shows a strong mobile loop: players bring a small deck, summon random towers from that deck, earn resources by defeating waves, and merge matching towers to create stronger board states. Its official store description also emphasizes PvP, co-op, solo, ranked and rotating modes. [Random Dice on Google Play](https://play.google.com/store/apps/details?hl=en-CA&id=com.percent.royaldice) and [dice mechanics reference](https://random-dice.fandom.com/wiki/Dice_Mechanics)

The Clash of Critters ads point toward a simple, readable lane-defense fantasy: place colorful units on a path, watch the wave advance, and make upgrades before the next threat. The game itself should deliver the advertised interaction honestly; the mode must be a real part of Echoes of the Rift rather than a disconnected ad-only mockup.

## Player promise

> Build a spellbook deck, defend the Rift gate, merge under pressure, and make one clever counter before the boss reaches the core.

The first version should be solo, 6–10 minutes, 20 waves, one boss and a compact 5x7 placement grid. Add co-op after the solo rules are stable. Do not start with PvP; random outcomes and network latency make balance and player trust harder before the core is proven.

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

The mobile layout should use a clear top wave/core bar, a compact deck strip, a visible Mana counter, a 5x7 board and a bottom-right hero cast. Towers use the same curved dark panels and neon border particles as the inventory. A long press opens a skill-book inspection card; dragging is the primary placement and merge gesture, with keyboard/controller alternatives on PC.

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
