# Echoes of the Rift — future design backlog

Revision 32, 28 September 2026.

This backlog turns the current master plan into a player-facing product direction. It is informed by the current game state, the My Heroes / Brave Frontier reference work, Apple Design Award winners, BAFTA Game Design winners, Google Play award winners and official mobile onboarding/performance guidance. It prioritizes memorable play and recognition over adding more menus.

## Product direction

Echoes of the Rift should feel like a fast, colorful pixel-action RPG where every short expedition produces a story, a build decision or a visible cosmetic improvement. The player should understand the next goal immediately, make meaningful combat choices, see the result of those choices in the world, and have a fair path to recognition through skill, collection and community contribution.

The core promise is:

> Enter a dangerous room, read the tell, make a build choice, survive a spectacular fight, and leave with something worth showing.

## Priority order

| Priority | Upgrade | Why it matters | Acceptance signal |
|---|---|---|---|
| P0 | Event drawer plus reward inbox | Turns the new server event system into a visible reason to return and protects earned rewards after disconnects | A player can see active events, claim a reward once, reconnect and recover an unclaimed reward |
| P0 | Short-session expedition loop | Gives mobile players a satisfying 3–8 minute session with a clear start, climax and payout | First-time players complete one run without a menu explanation; returning players see a next objective in under 10 seconds |
| P0 | Combat readability pass | Makes flashy combat strategic instead of noisy | Every boss attack has a readable tell, danger area, hit reaction and recoverable failure state |
| P0 | Real-device performance gate | A beautiful game that stutters loses trust | 60 FPS target on the reference device, no save loss, no soft-lock, and measured cold/warm start budgets |
| P1 | Buildcraft and loadouts | Gives gear, skills and cosmetics a reason to exist beyond larger numbers | Three viable builds can clear the same chapter through different decisions; loadouts swap without hidden state |
| P1 | Boss and chapter identity | Creates memorable content and shareable moments | Each major boss has a silhouette, arena rule, phase change, signature reward and unique defeat beat |
| P1 | Collection showcase and recognition | Converts investment into status players can show | Profile, town inspection and end-of-run cards show rare gear, cosmetics, titles and verified achievements |
| P1 | Social cooperation | Gives multiplayer a purpose beyond seeing another avatar | Players can join a short co-op objective, rescue a teammate, contribute to a shared goal and receive clear credit |
| P1 | Rift Defense skill-book mode | Reuses the collection in a strategic short-session format with strong replay value | Solo seeded defense is readable, fair across collections and completable in 6–10 minutes |
| P2 | Town life and player expression | Makes the hub a destination rather than a loading screen | NPC routines, rotating contracts, cosmetic spaces and player-created displays change between visits |
| P2 | Seasonal collaborations and story packs | Adds discoverable peaks without destabilizing the base game | A collaboration is a self-contained content namespace with legal sunset, rerun and reward recovery rules |
| P2 | Creator/community layer | Extends content discovery while keeping moderation manageable | Safe loadout guides, boss notes and screenshots can be searched, reported, revised and removed |

## Design pillars to carry into every feature

### 1. Learn by doing

The opening should place the player in control quickly. Use a short playable onboarding sequence: move, attack, dodge, use one skill, pick up a drop, equip it and enter the next room. Introduce one mechanic at a time and move advanced explanations into context tips and the Help page. Let experienced players skip or replay the lesson.

Do not stack registration, permissions, store offers, social prompts and long story panels before the first playable action. Defaults should let a new player start immediately; account linking and optional settings can follow the first successful run.

### 2. Readable spectacle

Keep the dark curved UI and high-definition pixel art, but give every effect a gameplay job. Boss tells use a consistent language: wind-up, danger shape, impact, recovery. Damage numbers, status icons, HP bars and skill cooldowns must remain legible when neon particles are at their brightest. High-tier gear may add border particles, trails and finishers, while hitboxes and telegraphs remain stable.

Add impact layers in this order: anticipation, contact, hit pause, knockback or stagger, sound, number, then reward feedback. Pool effects, cap decoration, and preserve warnings and damage logic under load.

### 3. Depth from combinations

Use a small number of understandable tags rather than hundreds of hidden modifiers. Suggested tags are element, weapon family, status, range, movement and role. Skills and gear can form visible two- and three-piece synergies, for example:

- **Storm Ranger:** marked targets chain lightning after a dodge shot.
- **Ember Guardian:** blocking a telegraphed hit stores a short cone burst.
- **Tide Medic:** healing a player who is below 30% health creates a slowing field.

Each build should have a strength, a cost and a counter. Avoid a single best combination by giving bosses different resistances, movement demands and phase rules rather than simply inflating health.

### 4. Recognition is a reward

Recognition should come from verifiable play. Add titles, profile frames, town banners, emotes, weapon trails and end-of-run cards for first clears, no-hit clears, rescue plays, speed records, helpful co-op contributions and collection milestones. Separate competitive rank from spend; paid items can be expressive but should not determine leaderboard power.

### 5. Respect the player’s time

Every recurring system needs a catch-up rule, an end date, a visible purpose and a way to stop notifications. Use rotating goals, not overlapping obligations that demand constant attendance. Never make a player fear losing an earned reward because the app crashed, the event ended or the device clock changed.

## Feature modules to prepare

### Combat and encounter modules

- **Boss pattern library:** phase graphs, telegraphs, arena hazards, adds, enrage rules, rewards and replay-safe score validation.
- **Encounter mutators:** low gravity, moving walls, cursed floor, escort target, timed rescue and limited-heal rooms. Mutators should be data-driven and combinable with a difficulty budget.
- **Build challenge rooms:** weekly rooms with a fixed loadout or a restricted tag set so skill is visible and new builds are discoverable.
- **Co-op roles:** aggro, damage, control and support contributions with rescue interactions; credit contribution without forcing a class queue.
- **Spectator and ghost run:** replay a verified best run as a translucent guide or inspect the boss timeline after defeat.

### Progression and collection modules

- **Saved loadouts:** gear, skills, enhancement snapshot, cosmetic profile and a named build card. Validate every item on equip and preserve missing-item warnings.
- **Set and synergy index:** show exactly which tags activate a bonus and which content teaches the counter.
- **Cosmetic mastery:** cosmetic-only levels, recolours, trails, victory poses and town displays earned through use rather than power inflation.
- **Collection codex:** item origin, first discovery, upgrade history, lore, preview and a clear missing-item path.
- **Catch-up track:** returning players receive a short sequence that points to the strongest current free progression path without invalidating old collections.

### Town and social modules

- **Rotating town contracts:** three daily contracts from a server pool; one combat, one exploration and one social/helpful objective.
- **Inspection and showcase:** inspect gear, skills, titles, verified records and cosmetics; hide private account data by default.
- **Guild or expedition lodge:** shared goals, donation caps, weekly co-op milestones and a visible contribution history.
- **Safe player expression:** emotes, profile cards, stickers and cosmetic room displays with mute, block and report controls.
- **Town discoveries:** short NPC routines, secret rooms, seasonal decorations and optional micro-quests that reward lore or cosmetics.

### Live-ops and collaboration modules

Reuse the current server event templates for login calendars, daily quests, progressive achievements, Boss Challenges, raid ladders, Event Bazaars, shops, community goals, double-drop windows, news/inbox and collaboration packs. Add three safeguards to every new module:

1. A visible player goal and a clear end time.
2. A server-side claim key, reward receipt and inbox recovery path.
3. A staging fixture that tests clock skew, reconnect, duplicate taps, event rollback and expired content.

Collaboration packs should contain a short story, a limited dungeon, cosmetic or character references, a token shop and a rerun flag. Keep licensed names, art, audio and promotional text in a separate namespace so a takedown or expiry does not corrupt the base catalogue.

## Production systems to add before scale

### Player experience measurement

Track the funnel without collecting unnecessary personal data:

- first launch to first movement;
- first movement to first clear;
- first clear to first equip;
- day 1, day 7 and day 30 return;
- event view to participation to claim;
- failed claim and inbox recovery;
- time to first upgrade and time to first meaningful choice;
- crash, ANR, frame-time, battery and memory by device tier.

Use these measurements to improve a flow, not to pressure players with artificial scarcity. Test one change at a time and keep a rollback switch.

### Content pipeline

Create a content checklist and preview tool for every item, enemy, boss, skill and event: native-size sprite, enlarged pixel inspection, colorblind check, mobile safe area, animation frame budget, atlas membership, localization strings, reward references, and server manifest validation. A designer should be able to assemble a new event from data without changing gameplay code.

### Reliability and trust

Keep the server authoritative for rewards, scores, timers and purchases. Add a durable ledger or outbox for rewards, a reconciliation job on sign-in, backup/restore drills, alerting for economy anomalies, rate limits, operator audit logs and a read-only support view. Never use the client clock or client-submitted reward amounts as proof.

### Platform quality

Maintain low, medium and high visual presets, dynamic effect caps, safe-area layouts, remappable controls, colorblind-friendly status shapes, text scaling, reduced-flash options and keyboard/controller support. Measure real devices instead of relying on the Windows touch simulation. The release target should include crash-free sessions, ANR rate, memory, startup and frame-time budgets per device class.

## Suggested release sequence

1. **Polish the first 10 minutes:** playable onboarding, first boss, first upgrade, first profile card and first event drawer.
2. **Make combat expressive:** boss pattern library, status clarity, hit feedback, three build synergies and one memorable chapter boss.
3. **Make progress visible:** saved loadouts, collection codex, cosmetics, titles, end-of-run cards and inspection showcase.
4. **Make multiplayer meaningful:** short co-op objective, rescue, contribution credit, guild/expedition lodge and ghost runs.
5. **Make live operations safe:** inbox/ledger, staging preview, dashboards, rollback, compensation and restore rehearsal.
6. **Expand content:** rotating events, collaborations, chapters, challenge rooms and creator/community features.

Do not move to the next step because a menu exists. Move when the player can complete the loop, understand the reward, recover from interruption and explain why the feature is fun.

## External review notes

- Apple’s 2024 Design Awards describe award-winning games through delight, inclusivity, innovation, interaction and visuals; the 2025 winners highlight Balatro’s layered variability, DREDGE’s smooth touch/controller interaction and cohesive world, and Skate City’s patient, well-spaced lessons. See [Apple Design Awards 2024](https://developer.apple.com/design/awards/2024/) and [Apple Design Awards 2025](https://developer.apple.com/design/awards/2025/).
- Google Play named AFK Journey its 2024 Best Game for its expansive roster, satisfying tactical battles, explorable world and art direction. Its 2025 Best Game, Pokémon TCG Pocket, was praised for tactile collection moments and presentation. See [Google Play Best of 2024](https://blog.google/products-and-platforms/platforms/google-play/google-play-best-apps-games-2024/) and [Google Play Best of 2025](https://blog.google/products-and-platforms/platforms/google-play/best-apps-games-2025/).
- BAFTA’s recent Game Design winners include Astro Bot, DAVE THE DIVER, Vampire Survivors and Hades, with Evolving Game recognizing Vampire Survivors. See [BAFTA Game Design](https://www.bafta.org/awards/games/game-design/).
- Apple recommends fast, fun, optional onboarding with interactive teaching and context-specific tips. See [Onboarding](https://developer.apple.com/design/human-interface-guidelines/onboarding) and [Onboarding for Games](https://developer.apple.com/app-store/onboarding-for-games/).
- Android’s guidance emphasizes measuring CPU/GPU bottlenecks, verifying optimization with A/B tests and monitoring crashes, ANRs, memory, battery and slow sessions. See [game performance](https://developer.android.com/games/optimize/gameperformance?hl=en), [quality benchmarks](https://developer.android.com/games/guidelines) and [Android vitals](https://developer.android.com/games/optimize/vitals).
