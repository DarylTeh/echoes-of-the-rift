# Echoes of the Rift master plan

Revision 67 - 4 October 2026. Shared skill, hero and equipment art reset.

## Current iteration - revision 67: replace shared skill, hero and equipment art

The art reset replaces the race, equipment and expansion atlases, and adds a 12-icon neon skill-cube atlas. `IllustratedArt.Skill` and `Book` now share the same cube selection across main-mode skill HUDs, inventory skill books and Rift Defense deck/towers. The inventory, shop and item details use the new equipment art through their existing family mapping. Hero race choices still select the corresponding race sprite; the existing skin/hair palette controls remain active. New atlas textures use point filtering and the existing 5% pixel-grid sampling. The face-covering weapon position was moved beside the compact hero.

The art follows the supplied MyHeroes: SEA / Random Dice visual language without extracting their game assets. Reference reading: [MyHeroes Wiki](https://myheroesofficial.fandom.com/wiki/MyHeroes_Wiki) and [Random Dice dice catalogue](https://randomdice.wiki.gg/wiki/Dice). The cube atlas is shared art, rather than a Rift Defense-only treatment.

The Unity Windows build passed (`Logs/step-10-20261004-223947.log`), desktop and touch-layout suites passed 350/351 checks with zero failures, and Rift Defense passed 20 waves/four bosses using 15 towers (`Logs/ArtUAT/FinalDefense/runtime-smoke.txt`). Inventory and combat captures in `Logs/ArtUAT/Final` were visually reviewed. The tested player was copied to the tracked `Release/Windows` handoff, and both player executable hashes match. Utility/interface symbols, enemies, world tiles, attack effects and remaining procedural art still need replacement. See [ART-DIRECTION.md](Assets/_Project/Resources/Illustrated/ART-DIRECTION.md) for atlas maps and remaining scope. This is not yet user acceptance of full-art replacement or 90% MyHeroes parity.

## Previous revision 66: Rift Defense reference-layout correction

## Current iteration - revision 66: rebuild the Rift Defense screen from the reference composition

Inspected actual gameplay footage from 20 distinct Random Dice co-op recordings before changing the mode. The sample covered beginner deck guides, live viewer matches, long-wave farming, challenge runs, damage/support/control roles and gear tests. The repeatable patterns are concurrent equal-weight boards, persistent wave and match-currency cues, randomized summon/merge decisions, legible board occupancy, and builds with complementary roles. Source links and observations are in [Docs/RIFT-DICE-DEFENSE.md](Docs/RIFT-DICE-DEFENSE.md).

The Rift Defense screen now uses a portrait 5x3 book board, a compact deck strip, left-to-right enemy lane, core bars, a stacked co-op partner board area, and grouped bottom controls. Tiers pulse around tower borders. The partner board is explicitly inactive because networked shared-board play is still planned. The single-player simulation still has random summons, same-rank merges, match-only Rift Mana, upgrades, hero burst, varied enemy profiles and bosses every fifth wave. No currency or rewards are written to the profile.

Validation: the updated Windows development build passed (`Logs/step-10-20261004-213203.log`); the portrait 20-wave stress run passed with four bosses and no runtime errors (`Logs/RiftDefenseRegression/player.log`). A visible preview launches with `-cookieSmoke -showRiftDefense` and writes `Logs/RiftDefensePreview/rift-defense-preview.png`. The max-rank test setup verifies progression flow only. Human-run balance/duration, visual acceptance against a full co-op match, actual shared-board networking, Android and low-end-PC performance remain open. The shipped `Release/Windows` package has not been replaced.

Next: replace the inactive partner board with server-synchronized co-op, and match the remaining in-match reference interactions (dice upgrade/merge feedback, enemy pressure, and boss presentation) using original Echoes art. Then play a normal run, measure timing/balance, and validate on Android/low-end PC. After these gates, develop East DPS Trial, West World Boss and scalable 4/8-player raids.

## Previous revision 62: sign-in and recovery error-state UAT

## Current iteration - revision 62: submit and verify sign-in/recovery errors

Extended the release account-flow smoke test to submit invalid credentials through the actual Sign in control and invalid recovery details through Recover account. Both forms remain usable, report a server error without claiming success, and preserve the typed username. The recovery form is verified to expose a clearly labeled “New password (5+ characters)” field. Fresh captures are saved at `Logs/Dedicated/Register/sign-in-rejected.png` and `Logs/Dedicated/Register/recovery-rejected.png`.

Validation: Unity optimized Windows build passed (`Logs/step-10-20261004-141156.log`); desktop UI passed 350 checks; touch-layout passed 351; account registration/recovery-code gate/authenticated campaign passed, with resume correctly skipped and reported because this hidden session still receives DPAPI Win32 error 2; full server `npm test` passed. `Release/Windows` matches `Builds/Windows` at SHA-256 `D7DB0BE44A6300955FA5FB306B9809DBEB4F0B39937797F646BE1F12BAC29563` and includes no PDB files. The recovery-error capture was visually reviewed.

Revision 62's next item was to test successful Unity sign-in/recovery and confirmed Settings > Accounts sign-out/re-login in a foreground player. Revision 63 temporarily prioritizes the requested mode development; that account-flow check and Android/low-end PC device validation remain open.

## Previous revision 61: explicit account choices and recovery confirmation

Revision 61 account-entry implementation and validation are recorded in the execution history. Protected session resume still requires a foreground player check.

## Previous revision 60: clean release validation and refactor

Revision 60 validation and repository cleanup are recorded in the preceding execution history. Physical Android and low-end PC performance measurements remain release gates.

## Previous revision 59: town hierarchy and contextual interaction

Reworked the Rift Haven navigation source around a compact left activity rail for Campaign, Events and Inbox, keeping Backpack and Settings on the right. Replaced the always-visible `Interact [F]` control with a right-anchored prompt that appears only when the player is close enough to a service or gate; keyboard `F` remains available. Pinned the rail and minimap to screen edges for aspect-ratio safety. Extended the town runtime smoke test to require the rail, campaign action and contextual prompt.

This is a meaningful source/layout change toward My Heroes' stable left activity navigation and situational interaction affordance. Visual acceptance is pending: the installed Unity Editor returned license error 198 (`No valid Unity Editor license found`), so neither a current player rebuild nor fresh desktop/touch captures could be produced. Existing 342/343 results cover the previously packaged build and are not evidence for this iteration. Activate the licensed editor, rebuild, then run desktop/touch UI checks and review fresh 1280x720 plus portrait captures before rating parity. Do not delete and replace the full UI until that fresh capture is reviewed against the My Heroes references.

## Previous revision 58: account, startup and connection UAT

## Current iteration - revision 58: entry and recovery flow review

Extended the local UAT through startup-service failure, in-game connection loss, Settings > Accounts, registration, the account-ready state, and server-side login/recovery/logout. The 342 desktop and 343 touch-layout checks, isolated registration/resume campaign flow, both retry/error modals and full `npm test` pass. Detailed pass levels and captures are in [Docs/UAT-2026-10-03-MyHeroes.md](Docs/UAT-2026-10-03-MyHeroes.md).

New findings: the project has no dedicated branded splash or loading/progress screen; its `SplashScreenUI` is the title/account form. After account creation, Login and Recover controls remain visible but their handlers are inert. Login/recovery API behavior passes, but neither Unity submission flow has been exercised end to end; confirmed sign-out followed by UI login and a human creator-to-town path also remain open. Preserve the protected Settings > Accounts sign-out and the non-destructive retry dialogs.

Next: implement and test the entry sequence (splash/loading → explicit Sign in/Create/Recover → recovery-code acknowledgement → creator if needed → town), with clear invalid-credential/reset states. Extend the Unity runtime account tests to cover success/failure for each form, confirmed logout and return/re-login. Package only after a current Unity rebuild; the release binary still carries the obsolete 15-character label. Then continue the My Heroes screen hierarchy and Android touch acceptance.

## Previous revision 57: local My Heroes reference UAT

## Current iteration - revision 57: live local UAT and UI parity baseline

Started the local server and Windows client, then ran the isolated register/resume-to-campaign acceptance flow and the packaged UI layout checks. Account/campaign flow passed twice across four stages with server-authoritative rewards, pause and rankings; UI bounds passed 342 checks with no runtime errors. A screen-by-screen comparison against the supplied My Heroes references and existing video/wiki research is recorded in [Docs/UAT-2026-10-03-MyHeroes.md](Docs/UAT-2026-10-03-MyHeroes.md).

The current build is functional but does not meet the requested 90% visual/layout target. The strongest matches are the combat corner anchors and the paperdoll/grid silhouette. The biggest gaps are the extra title-entry step, sparse town navigation and actor-label collisions, desktop-shaped combat controls, three-slot gear display, weak item detail hierarchy, generic shop layout and oversized rankings panel. The release password text is also stale (`15+` versus the implemented `5+` rule). The release player requires a current Unity rebuild before its UI can reflect the source tree. Matching DirectStorage runtime DLLs have been restored to `Release/Windows`; the running local server reports ready.

Next implementation: rebuild/package the current Unity source, make first-run entry direct and explicit, then implement the reviewed town navigation and label hierarchy. Continue with touch-first combat spacing and the complete gear/skill/item/shop/result hierarchy. Re-run real client screenshots and repeat this UAT; Android device acceptance remains open. Preserve the server's existing account, reward and gameplay authority.

## Previous revision 56: read-only event manifest preview

## Current iteration - revision 56: safe event manifest preview

The localhost admin API now exposes `POST /api/events/preview`. It uses the same manifest validator and normalizer as publication, evaluates the UTC schedule at the supplied `previewAt` (or current server time), and returns the normalized manifest, state, next boundary and currently published version. It rejects non-UTC preview timestamps and versions that cannot be published. Preview is read-only: it does not write an event, history entry or player data.

Validation: full `npm test` passes, including active/ended schedule previews, next-boundary calculation, malformed timestamp/manifest rejection, version conflict and proof that preview leaves the event list unchanged. Swagger includes the route and the operator guides explain the staging workflow.

Next: use preview when preparing recurring event manifests, then run the existing publish/kill-switch staging and backup checks. Keep the event bazaar UI behind wireframe feedback; continue with another server/production-readiness slice without changing unapproved navigation.

## Previous revision 55: configurable event-currency earning

## Revision 55 details: event earning rates and daily caps

Verified event progress now honors each event manifest's `progressPerClear` and `dailyTokenCap`, with UTC-day earnings recorded per player, event version and metric. Earning is capped by both the event's total progress cap and the per-day cap. The event-shop status response includes the daily cap, amount earned today and remaining daily earning allowance. Existing manifests without an earning rate retain their previous ten-token campaign-clear fallback; new templates explicitly configure the rate and cap. Invalid rates/caps are rejected before publication.

Validation: full `npm test` passes, including configured rate precedence, an exhausted daily cap, UTC rollover, mode eligibility, status output and manifest bounds. No Unity UI changed; bazaar integration still waits on wireframe feedback. Final OCI recheck at 2026-10-02 14:27 UTC found A1 Flex and E2.1.Micro still out of host capacity, paid E5 Flex still available, and no VM or reserved public IPv4. No cloud resources were launched.

Next: integrate the approved shop wireframes against the server-owned status and purchase endpoints. Keep the remaining combat/gameplay UI changes behind player review; continue with another server feature only where it does not preempt that review.

## Previous revision 54: read-only event-shop status API

## Revision 54 details: shop offer and eligibility status

Added authenticated `POST /event-shop-status`, version-pinned to the current event manifest. It returns UTC server time and state, player event-token balance, each configured reward/price, shared remaining stock, the player's remaining purchase allowance and a clear purchase eligibility reason. A status read does not create stock records or mutate player data; the purchase route remains authoritative if balance or shared stock changes afterward. Tests cover empty stock initialization, balance/stock updates after purchase, limit eligibility and stale manifest rejection.

Validation: full server test suite passes. No Unity UI changed because the wireframes are still awaiting player feedback. OCI remains unattached: the Singapore Always Free capacity report remains unavailable and the Linux server build prerequisite is unresolved.

Next: fold the approved wireframes into a focused event bazaar UI using `/event-shop-status` and `/event-shop-purchase`, with server response as the only source for price, eligibility and stock. Keep the UI change separate from the local-only OCI deployment gate.

## Previous revision 53: live OCI hosting feasibility check

## Revision 53 details: OCI capacity and deployment gate

Queried the tenancy with OCI CLI 3.94.1. Only the Singapore home region is subscribed, it has one availability domain, and no compute instances or reserved public IPv4 addresses exist. The existing VCN and public subnet are available. Capacity reports show Always Free A1 Flex (1 OCPU/6 GB), E2.1.Micro and paid E4 Flex (2 OCPU/12 GB) out of host capacity; paid E5 Flex (2 OCPU/12 GB) is available and quota is sufficient. E5 is approximately US$61.32 per 730-hour month for compute alone at Oracle's published rates, excluding storage/network/tax; nothing billable was launched. Always Free compute is limited to the home region, and Oracle region subscriptions cannot be undone.

The installed Unity Editor 6000.6.2f1 does not have Linux Standalone support installed, and there is no Linux game-server build in the repo. The checked-in game remains loopback-only, with no Oracle IP or public endpoint. The official Unity CLI installer was blocked by automatic review because it would execute an uninspected remote PowerShell script with elevated privileges. No OCI infrastructure was changed except generating capacity reports.

Hosting decision gate: if the player keeps the free-tier-only requirement, wait for Singapore A1/E2 capacity to return; there is no free, deployable host now. If the player approves E5 billing and a monthly cap, install the Unity Linux module through the normal licensed Unity workflow, build and test Linux x86_64 server, then deploy HTTPS/account plus encrypted gameplay only after transport and client endpoint work are validated. Do not subscribe another region as a free-tier workaround.

## Previous revision 52: server-authoritative event shop

## Current iteration - revision 52: server-authoritative event shop

Implemented authenticated `POST /event-shop-purchase` with event-version validation, active schedule/type checks, offer validation, available event-token checks, per-player limits, shared stock, and an idempotent request key. The token debit, shared-stock decrement, purchase receipt and reward-inbox enqueue commit in one SQLite transaction, so retries cannot double-spend or duplicate rewards and failures roll back together. Added coverage for success, retries, stale versions, player limits, low balances, sold-out stock and rollback.

Validation: all server suites pass with `npm test`. This is a server feature only; no Unity UI/build changes were made. The shop UI remains behind the player's wireframe review. Oracle deployment is still not connected: the client endpoint is loopback (`127.0.0.1`), and the latest deployment notes record no VM or public IP after Singapore capacity failures. A live OCI inventory could not be confirmed in this turn.

Next: add a read-only event-shop status/offer endpoint so clients can display server-authoritative prices, remaining stock and purchase eligibility, then wire it into the event UI after the player approves the wireframes. Continue OCI deployment only after a compatible Linux server build and available VM capacity are confirmed.

## Previous revision 51: design feedback pack

## Current iteration - revision 51: design feedback pack

Created 30 separate, editable 1280×720 landscape SVG wireframes for the current player-facing screens and key conceptual gaps. The pack is in [Design/Wireframes](Design/Wireframes/README.md); open `Design/Wireframes/index.html` to browse each screen. `Tools/GenerateUiWireframes.py` regenerates the vector layouts. Existing flows and proposed concepts are labelled separately, and each file asks one high-impact design question. These are low-fidelity proposals for the player to edit; they are not final art or approved UI decisions.

Next: incorporate the player's edited wireframes and answers, then implement the approved navigation/hierarchy changes screen-by-screen. Keep UI implementation behind that visual review rather than adding more controls before the player can approve the layout. The strict review remains in [Docs/UI-STRICT-REVIEW.md](Docs/UI-STRICT-REVIEW.md).

## Previous revision 50: strict player-facing UI audit

## Current iteration - revision 50: screen-by-screen UX review

The strict review is recorded in [Docs/UI-STRICT-REVIEW.md](Docs/UI-STRICT-REVIEW.md). It covers startup/authentication, character creation, Rift Haven, collection/shop/item detail, skills, settings/account, events/inbox, combat HUD, pause, results/recovery, reconnect, revive and leaderboard. This revision is a documented review only; runtime UI has not yet been redesigned.

The main finding is weak hierarchy: too many simultaneous destinations and equivalent-weight controls, with reward/error explanations detached from the action that follows them. Highest-priority redesign is the sign-in path and town navigation, then the incomplete three-slot paperdoll/dedicated skill page, combat touch spacing, event/reward surfaces and result panels. Preserve the protected sign-out path, server-authoritative rewards, account safety, fixed neon motion and current combat rules.

My Heroes references confirm stable lobby rails, compact edge HUD groups, an equipment paperdoll beside its grid, separate item inspection, and goal/reward-oriented quest rows. They do not establish every hidden interaction or equivalent screens for our live-event calendar, backend recovery, or placeholder modes. The audit marks those distinctions and requires live-client capture review before claiming visual acceptance.

Next UI implementation sequence: (1) simplify first-run account entry into explicit sign-in/create/recovery steps, (2) establish the top-level screen map and remove duplicate town navigation, (3) fix honest hero/skills hierarchy after choosing the gear/skill migration contracts, (4) reduce combat overlay competition at Android landscape sizes, (5) make event, inbox, item and result states outcome-first, (6) run the screen-by-screen capture and first-time-player acceptance in the review. Do not add further menu buttons before that navigation pass.

## Previous revision 49: bounded HUD and event work

## Current iteration - revision 49: bounded HUD and event work

The skill HUD now updates health/mana text, bars, weapon art and skill art only when their values change; cooldown fills still refresh on the short gameplay cadence. The event service caches the active manifest list for up to one second and invalidates it immediately after publication, reducing repeated JSON/history scans while preserving schedule transitions and authoritative claims.

## Previous revision 48: measured hot-path optimization

SQLite now has supporting indexes for session/ticket expiry cleanup, player inbox reads, event progress/claim lookups and admin audit history, with a five-second busy timeout on the shared store. Overhead HP bars keep smooth per-frame positions while refreshing sprite-bound scans, health fill and colours at bounded rates. These changes reduce repeated work without changing authoritative combat, reward or account behavior.

## Previous revision 47: complete clone handoff

The repository now contains the complete reproducible game handoff: Unity project files, server source/tests, deployment scripts, portable launchers, Windows release payload and endpoint runbook. Repo-local command files cover dependency installation and starting the server/player. OCI provisioning resolves paths from the new laptop's user profile or environment instead of the original developer's absolute paths. No public OCI IP exists yet because the VM launch was out of capacity; private keys, credentials, databases, logs and dependencies remain machine-local.

## Previous revision 46: accessible account password minimum

Account registration and recovery now accept passwords from 5 through 128 characters. The Unity account form labels the requirement consistently, and the account suite covers both rejection below five characters and successful five-character registration. Existing longer passwords remain valid.

## Previous revision 45: clone-ready release packaging

The repository now contains a tracked `Release/Windows` development distribution with the current playable client payload, portable player/server launchers and the admin shortcut. The launchers resolve `Tools/` relative to their own location, so a fresh clone no longer depends on the old machine path. `Server/` source, package lock and operational scripts remain tracked; `node_modules`, progress databases, logs, admin hashes and credentials remain local by design. All test scripts prefer the tracked release build and fall back to `Builds/Windows` for editor-generated outputs. The next feature slice remains module-specific event rewards and event-shop spending.

## Previous revision 44: authoritative event progress and claim eligibility

Verified campaign reward transactions now advance matching active event modules exactly once. SQLite stores progress per event version and player, caps progress at the manifest rule, filters eligible modes, returns progress and the next threshold from `/event-status`, and gates milestone claims until the server confirms the threshold. The Events drawer renders the server-owned metric/value/cap/next-threshold alongside claim status. This keeps device clocks, client counters and duplicate reward receipts from changing event progress. The next live-ops slice is module-specific reward delivery and event-shop spending, followed by measured device acceptance.

## Previous revision 43: Local playable stack verified; OCI VM capacity remains unavailable

The hot paths now avoid avoidable per-frame allocations: co-op snapshots reuse exact-size buffers and cooldown arrays, replica/bolt removal sets are reused, network presentation caches its rigidbody, summoned runes use `OverlapCircleNonAlloc`, skill HUD labels update only when values change, and safe-area transforms recalculate only after a screen or safe-area change. Low and medium quality defaults are selected for Android/iPhone and standalone development targets; neon effects remain active with a lower quality-aware burst budget. The local account/persistence services pass health checks, the dedicated Unity server listens on UDP 7770, and the packaged Windows client is running against the local stack. OCI authentication and network resources are ready, but both Always Free VM shapes returned out-of-host-capacity in Singapore; Linux server build, measured device/server tests and encrypted public gameplay remain open for cloud deployment.

## Previous revision 39 - 30 September 2026

Revision 39 - 30 September 2026. Low-end runtime optimization and allocation audit. Co-op snapshots reuse buffers and cooldown arrays, rune targeting is non-allocating, repeated HUD/safe-area writes are reduced, and Android/iPhone/standalone quality defaults are tuned for low-end targets. The remaining 20 Hz scene scans stay measurement-driven.

## Previous revision 38 - 29 September 2026

Revision 38 - 29 September 2026. Authoritative event status.

## Previous revision 37 - 29 September 2026

Revision 37 - 29 September 2026. Event detail and eligibility presentation.

Revision 36 - 29 September 2026. Transactional reward inbox.

## Current iteration - revision 36: recoverable event rewards

The server now validates event reward payloads, creates inbox entries in the same transaction as an idempotent event claim, and exposes private persistence operations to list and collect entries. Currency and catalogue-item rewards are bounded, unknown items are rejected, replayed event claims do not duplicate entries, and replayed collection returns the existing result without applying the reward again.

The town HUD now exposes a Reward Inbox panel. It lists pending server-delivered rewards, collects them through the persistence service, and applies the returned authoritative profile. Collection is safe to retry after a disconnect; the client never supplies reward amounts or item definitions. The next live-ops slice is event-specific progress and claim eligibility, followed by broader real-device acceptance.

## Previous revision 35 - 28 September 2026

Revision 35 - 28 September 2026. In-game event drawer.

## Current iteration - revision 35: server-timed event visibility

The town HUD now exposes a compact Events entry point and a modal event drawer backed by the existing server-authoritative `/events` feed. It shows active event titles, reusable event types, versions and server-time countdowns, handles an unavailable feed without blocking the town, and owns input while open. It is intentionally read-only: rewards and eligibility remain server/inbox work.

The drawer reuses the dark curved panel language, safe-area/focus handling and fixed neon presentation policy. Escape closes the drawer before pause, and the town cannot interact while the drawer owns the modal layer. The next live-ops slice is transactional inbox delivery and event-specific progress screens.

## Previous revision 34 - 28 September 2026

Revision 34 - 28 September 2026. UI safety and presentation audit.

## Current iteration - revision 34: safe modern navigation

The runtime audit is recorded in [UI-SAFETY-AND-PRESENTATION-AUDIT.md](Docs/UI-SAFETY-AND-PRESENTATION-AUDIT.md). Sign out is confined to Settings > Accounts with a confirmation whose safe default is Stay in game; connection-loss UI exposes Reconnect only. The reduced-motion preference is intentionally absent, while neon borders, world trails and skill effects stay active under fixed performance budgets.

The shared `GameUI` frame/focus/safe-area layer, town mode routing, collection/shop inspection flow, combat skill wheel and pause ownership are the approved navigation foundation. New modes must use explicit availability gates, own input while open, provide a safe close/back route and pass desktop and touch smoke checks. Remaining visual work is authored directional animation, boss/environment art and broader gear-family coverage; it should not add risky HUD actions or extra settings switches.

## Previous revision 33 - 28 September 2026

Revision 33 - 28 September 2026. Player-first progression, award-informed design and Rift Defense mode proposal.

## Current iteration - revision 33: Rift Defense mode

The game can support a tower-defense mode without changing the core action RPG. [RIFT-DICE-DEFENSE.md](Docs/RIFT-DICE-DEFENSE.md) defines a 6–10 minute solo mode where five owned skill books form a deck, books summon onto a 5x7 board, matching towers merge, temporary mutations appear between waves and the hero retains one emergency cast. The mode reuses current book names, elements, art and progression presentation while keeping `TowerPower`, wave budgets and rewards in a separate ruleset.

The implementation order is solo deterministic defense, daily seeded mutators, co-op, normalized draft mode, seasonal events and only then PvP. The server must validate deck, seed, placements, merges, Mana, boss state, score and rewards; the client submits intents and never final damage or reward amounts. Add `tower_defense` to the event scheduler for seasonal boards. The mode must remain optional, avoid a new premium currency, and never become the fastest mandatory source of campaign power.

This mode is inspired by Random Dice's deck/summon/merge adaptation and the readable lane-defense fantasy shown in Clash of Critters advertising, while preserving an honest, playable implementation inside Echoes of the Rift. See the full board rules, tower roles, enemy counters, boss ideas, economy safeguards, performance budgets and acceptance tests in the design document.

## Previous iteration - revision 32: player-first future design

The next layer of the game is now planned in [FUTURE-DESIGN-BACKLOG.md](Docs/FUTURE-DESIGN-BACKLOG.md). The highest priorities are the event drawer and recovery inbox, a short readable expedition loop, boss pattern readability, real-device performance budgets, meaningful loadouts, memorable boss identities and verified player recognition. Future additions are organized as reusable combat, progression, town, social, live-ops, measurement and production modules rather than isolated screens.

The design review compares recent Apple Design Award, BAFTA Game Design and Google Play winners with the existing My Heroes / Brave Frontier direction. It adds an explicit first-ten-minute target, three-to-eight-minute sessions, build synergies with counters, cosmetic recognition, catch-up rules, contextual onboarding, accessible controls, telemetry, staging, rollback and device-quality gates. The backlog records what to build, why it matters, and how to accept it.

The implementation order is: first ten minutes, combat readability, visible collection progress, meaningful multiplayer, reliable live operations, then larger content and community systems. A feature is not considered complete because its menu exists; it must be playable, understandable, recoverable after interruption and measurable on target devices.

## Previous iteration - revision 31: reusable live-event architecture

The item language now separates fixed rarity stars from enhancement: `***..  +12` (font-safe pixel asterisks). This keeps a high enhancement number readable and follows the useful division seen across Clash of Critters, AFK Journey and IdleOn. `ProgressionRules` contains the server-mirrorable gear/skill formulas, soft power caps, chapter target curve and first-five-chapter carry flag; [PROGRESSION-MODEL.md](Docs/PROGRESSION-MODEL.md) records the calculations and migration requirements.

The current five-band catalog field remains backward-compatible. Server reads and writes now normalize profiles with authoritative `EnhancementLevel`, `SkillLevels`, `EquippedEnhancementLevels` and `ProgressionVersion` fields without changing existing currencies, counts or AdminRevision. The uncapped enhancement transaction remains gated until the client supports multiple enhancement-level stacks per catalog item.

Validation for this slice: Node persistence/account/admin/progression suites pass; final Windows export `Logs/step-10-20260928-192803.log`; desktop UI 342 checks, touch UI 343 checks and full gameplay regression pass with no runtime errors.

## Revision 31 implementation record: reusable live-event architecture

Added versioned UTC event manifests, reusable event templates, an SQLite event store with historical versions and idempotent claim receipts, server-authoritative active-event listing at `POST /events`, local Swagger event list/publish/disable operations, and a client countdown/feed adapter that consumes server time. The reusable modules cover login calendars, token exchanges/Event Bazaars, daily quests, progressive achievements, Boss Challenges, raid ladders, community goals, event shops, temporary drop/energy modifiers, collaboration packs and news/inbox delivery. Full operating guidance, cadence, staging, rollback, legal and economy safeguards are in [EVENTS-OPERATIONS.md](Docs/EVENTS-OPERATIONS.md). The next slice is the in-game event drawer and transactional inbox reward delivery.

Reference review: Clash of Critters demonstrates overlapping rotating events, daily reset boss ladders, randomized daily quests and progressive achievements. Brave Frontier demonstrates Vortex/event dungeons, login campaigns, resource boosts, recurring arenas, raid/grand content, Event Bazaars and collaboration packs with exclusive story, units, tokens and community goals. The game should reuse these structures while keeping all timers, progress, reward claims and score validation on the server.

### Revision 31 production plan

| Workstream | Reusable production module | Exit condition |
|---|---|---|
| Calendar and authoring | Versioned manifest, UTC scheduler, Swagger publish/disable, staging preview | An operator can schedule, pause and roll back an event without editing SQLite |
| Recurring content | Login calendar, daily quests, progressive achievements, token exchange/Event Bazaar, event shop, double-drop modifier | A new event only supplies IDs, dates, text, catalog references, caps and rewards |
| Competitive content | Boss Challenge, raid ladder, arena season, community milestone | Scores, attempts, contribution and ranked rewards are server-verified and replay-safe |
| Client delivery | `/events` feed, event drawer, server-time countdown, news/inbox | Reconnect and device clock changes never alter eligibility or duplicate rewards |
| Economy and recovery | Transactional reward inbox, unique claim keys, ledger/outbox, compensation grant | Disconnects, crashes and database restarts recover every earned reward exactly once |
| Collaboration release | Story/dungeon pack, cosmetic/unit catalog namespace, token shop, rerun flag, legal sunset metadata | Rights, localization, asset expiry, rerun and takedown rules are approved before publish |
| Operations | Staging time acceleration, dashboards, alert thresholds, backups, restore drill, kill switch | The live team can detect inflation, claim failures or score abuse and stop an event safely |

Productionization order is calendar/admin, client event drawer, inbox/ledger transactions, then the deeper competitive modules. Keep event content data-driven and isolated from the base item catalog; keep the game server, database, backups, metrics and admin service on separate access boundaries before public launch. See [EVENTS-OPERATIONS.md](Docs/EVENTS-OPERATIONS.md) for the cadence, reference patterns and runbook.

## Previous build - revision 29, 27 September 2026

Implemented the reference-led collection redesign: separate equipment/hero and five-column backpack panels, real equipped gear and six current stance skills, visible category tabs and a sort-choice menu. Items open in a dimmed inspection popup with rarity ribbon, large icon, real stat comparisons, story and Equip/Upgrade actions. Back closes the popup first and retains the browsing page; background controls stay blocked. Starter skills have a read-only inspection view. The merchant now uses a separate wide three-card layout with real gold prices and owned counts; inspection precedes Buy.

Dark double-bevel panels, red close controls and tier-only border ornament match the documented reference hierarchy more closely. Existing approved item/hero art is retained. Shop browsing suspends the preview camera; full-screen dimming covers taller aspect ratios and safe-area offsets. No server schema or economy rules changed.

Validation: final Windows export `Logs/step-10-20260927-210718.log`; strict text encoding PASS, 283 authored files. Desktop UI PASS 342 checks and touch-layout PASS 343 checks at four sizes, zero failures/runtime errors. Includes starter-skill text, all 140 item descriptions/lore, category filtering, nested Back, keyboard focus, border effects and actual Buy/Equip/Upgrade against disposable saves. Final native backpack, item popup and merchant captures reviewed. Solo ten-run campaign/save-recovery regression and isolated dedicated account registration/resume regression passed with no runtime errors. Physical Android testing remains open. The 1024px-wide development capture still shows the existing world-camera pixel-reference warning; it is not a UI overflow failure and lower-resolution world rendering remains a device-acceptance item.

Reference coverage and unknown flows are recorded in [the UI audit](Docs/MYHEROES-UI-AUDIT.md). Core inventory/shop/profile samples and two available wiki pages received a second pass; matchmaking and unobserved social flows are explicitly not claimed as fully verified. Six-equipment/four-skill migration, saved loadouts, complete quest/profile/social screens and production world animation remain open. Six broad roadmap milestones remain open. Reopen **Play Echoes of the Rift.exe** to load this build.

## Previous build - revision 28, 27 September 2026

Fixed malformed UTF-8 punctuation throughout the authored UI and repaired affected project documentation. Added a strict UTF-8/mojibake build gate plus runtime visible-label checks; `.editorconfig` specifies UTF-8. The gold/safe-zone, level/gems, movement/interact and gate strings no longer contain the stray accented A.

Reworked the reference-led HUD: cached pixel coin, faceted cyan gem and cog icons; top-right wallet counters, level below the upper-left portrait, backpack down the right edge, a separate safe-zone subtitle, smaller merchant service bubbles with nearby-only names, and matching inventory/shop currency headers. Boss/map spacing was adjusted around the wallet. Dark rounded surfaces remain; wallet labels update only when values change. Original approved hero/item art and combat contracts remain intact.

Moved sign-out to Settings > Accounts with a separate confirmation and Stay in game selected by default. The overlay blocks movement and bag interaction. Back first cancels confirmation, then closes settings and restores controls. Controls and Accounts tabs are available; no sign-out action remains on the town HUD.

Validation: Windows export `Logs/step-10-20260927-201608.log`; source encoding gate PASS (282 authored files). Desktop layout 319 checks and touch layout 320 checks passed across four sizes, zero failures/runtime errors, before the final NPC-height-only correction. Final build touch checks also PASS 320 with zero failures/runtime errors; final town capture confirms bubbles clear NPC faces. Authenticated registration and resume runs passed account cancellation (same token and connection), reconnect, campaign, server-only rewards, pause and rankings with no runtime errors. Native town, inventory, combat and Accounts/confirmation screenshots reviewed. Physical Android input/performance acceptance remains open.

References: [MyHeroes review and wiki links](Docs/MYHEROES-GAMEPLAY-REFERENCES.md). This implements the HUD/settings feedback, not a complete recreation of MyHeroes. Remaining visual work includes authored town structures/NPC differentiation, directional animation, richer paperdoll and item comparisons. The broader six open milestones remain in the master plan; server economy/drop configuration is still the next backend slice. Reopen **Play Echoes of the Rift.exe** to load the rebuilt game.

## Previous build — revision 27, 27 September 2026

Implemented localhost Swagger administration at `http://localhost:8083/allah/api/hehe/v69/docs/`, with username/password authorization backed by an ignored salted scrypt hash. The parent-folder **Open Rift Admin.cmd** launcher starts/reopens it. See [admin guide](Server/ADMIN.md) for query/edit workflow. Player/account/catalog queries, give/set gold and gems, set level, inventory stack creation/replacement/removal, equipped-copy protection, atomic audit history, idempotent request IDs and revision conflicts are implemented. Existing game/account URLs are unchanged; admin access remains loopback-only.

Gems, character Level and AdminRevision persist with legacy-save defaults. The town displays level/gems; large stack counts use compact labels. Catalogue exports include names. Reconnect refreshes admin edits; character level currently does not affect combat stats/campaign unlocks. Uncapped upgrades, DB-configurable economy/drop rules, achievements/inbox and live profile push remain open. A reconnect teardown guard prevents the expedition ally HUD from querying an uninitialized network object.

Validation: all three Node suites pass (persistence, accounts, admin), including wrong-password/Host/Origin rejection, old-route removal, duplicate grant prevention, revision conflicts and durable audit/profile reopen. Swagger renders in the browser at the requested localhost URL. Desktop UI passed 305 checks, zero layout failures/errors, before the teardown-only fix. Final Windows export: `Logs/step-10-20260927-153710.log`; dedicated registration/resume/reconnect regression PASS for both runs, including authentication, server-only grants, four campaign stages, central rewards, pause and rankings; no runtime errors. Production player rewards were not changed during tests; a consistent database backup was created first.

Review/next: finish milestone 4 with database balance/drop-rule versions and gem progression, then safe uncapped upgrades and achievement/inbox recovery. Six broad workstreams remain open; this admin foundation does not complete the entire economy milestone. Android and public-deployment acceptance remain pending.

## Current remaining-work count

The active roadmap now groups the work into **seven major milestones**, not seven small tasks. The current combat HUD/navigation foundation is implemented and locally accepted; six broader milestones remain open. Device/network/public-release acceptance remains in milestone 6 even when a feature's Windows checks pass. This checklist supersedes historical ordering below; all detailed requirements remain unless explicitly changed by the user.

1. Combat HUD/navigation foundation: named boss health, two-player ally portrait/health/down-state, current-room map and actor markers. Local acceptance recorded below.
2. Art, animation and loadouts: directional animation; finished boss/NPC/environment/skill art; six-equipment/four-skill/runic migration with preserved saves; comparisons and readable paperdoll.
3. Town, inspection and multiplayer ownership: public player identity/inspection, instances/capacity/routing, safe selling, modal and reconnect consistency.
4. Authoritative economy and recovery: DB balance versions/drop rules, gold/gems, uncapped upgrade migration and simulations, achievements, receipt-safe inbox/admin recovery.
5. Campaign/modes/capacity: requested chapter structure, varied bosses, trial/world boss/survival and validated 4/8-player raids.
6. Device performance and public release: Android support and real-phone testing, low-end PC profiling, separate-device adverse-network tests, secure public transport/storage, portable packaging, deployment/backups/monitoring and five first-time-player sessions.
7. Community/commercial integrations: safe moderated build/wiki publishing and compressed uploads, opt-in verified cinema rewards, chapter gem offers and storefront sandbox/receipt recovery before live payments.

Full acceptance checklist: [MVP-ROADMAP.md](MVP-ROADMAP.md). These are broad workstreams with many subtasks; no completion percentage or release date is implied. Milestone 7 remains in the requested full roadmap; it is not secretly implemented by the existing placeholder helpers.

## Current iteration — expedition HUD foundation

Implemented `ExpeditionHUD`: named purple boss health bar with exact health values; it uses local authoritative combat state in solo/host play and the received boss snapshot in dedicated-client play. It disappears when the boss dies/room clears. A compact ally portrait uses received race appearance and health, with ALLY DOWN for defeat; hidden when solo. This supports the current two-player prototype, not a claim of 4/8-player UI/capacity or public username inspection.

Dungeon minimap uses the actual current stage's width/height, preserves aspect ratio and shows bounded cyan player, gold ally and pink boss markers with matching legend. It represents the current rectangular room; interconnected maze topology/fog of war is not implemented. Expedition panels hide in town and share SkillHUD ownership, so inventory suspension restores them consistently. The middle stays clear. Boss bar moved to the top edge after screenshot review; stage/status copy sits below it. Refresh is limited to 20Hz with a reused ally list and cached UI objects; no new per-frame scene-wide scans or render cameras.

Windows export: `Logs/step-10-20260927-124226.log`. Initial feature build passed touch UI checks, damage/clear/map assertions, dedicated Leader/Peer boss+party visibility and boss-clear hiding plus both revive paths/rewards, and solo combat/ten-run campaign regression, all without reported runtime errors. Final build UI: 304 touch checks and 303 desktop checks PASS at four resolutions, zero layout failures/runtime errors. Boss damage/clear, solo party hiding and minimap bounds assertions pass. Final top-edge boss/map capture reviewed. Results are recorded in EXECUTION-LOG.md; gameplay/network checks preceded that layout-only rebuild. Backend Node suites last passed revision 23; no backend data contract or RPC schema changed here.

Revision 26 review (superseded by the user priority for milestone 4): next software slice was milestone 2's authored directional hero/weapon animation. In parallel with future implementation, prepare milestone 6's performance capture and Android build pipeline; actual physical-device QA cannot be claimed from the Windows touch simulation. Public player names, larger parties, maze generation and fog are consciously open requirements, not fake values on this HUD. Real-device acceptance remains open.

## Previous iteration — revision 25 touch-control implementation and desktop simulation

Completed the software part of revision 24 priority 1: movement and attack/aim now have fixed circular backgrounds with separate cyan drag thumbs. The hit area no longer moves with the finger. Both sticks clamp travel, have a small centre dead zone and independently own one pointer; another finger cannot steal or release a held stick. They release on pointer-up, cancellation, application focus loss/pause, disabling the HUD, paused gameplay or disabled player controls. Existing virtual-gamepad input still flows through the server-authoritative movement/attack path.

Added `-touchControls` for Windows touch-layout QA and `Tools/Test-UI.ps1 -Touch` for repeatable pointer fixtures and captures. Device detection is cached, avoiding command-line allocations every frame. Default desktop mouse/keyboard layout is unchanged.

Validation: Windows export `Logs/step-10-20260927-102610.log`. Touch UI PASS 303 checks; desktop UI PASS 302 checks; both zero layout failures/runtime errors. Pointer fixture PASS simultaneous move/aim control values, fixed background positions, bounded thumb travel, pointer ownership, independent release, focus reset, inventory disable/reopen reset and cancellation. Four screen sizes checked in each mode. Reviewed 1280x720 touch combat capture. These are simulated Unity pointer events on Windows, not Android OS input/device acceptance. Solo combat/presentation fixtures and ten-run campaign regression PASS: movement, dodge, cooldowns, boss-only clear, inventory, loot, save recovery, no campaign revive, pause/resume and defeat; no runtime errors. No new server protocol or backend changes; dedicated pair last passed revision 24.

Review / next ordered work:
1. Real Android touch QA remains OPEN: test actual multitouch, dragging outside controls, interruption/resume, safe areas and comfortable thumb reach. Install Android editor support and establish a named target device before claiming mobile readiness.
2. Add repeatable frame-time capture, then profile a 10-minute crowded fight on a named low-end PC and Android phone. Measure allocations, memory, CPU/GPU frame times and latency before further optimization.
3. Test two real devices under latency/jitter/loss; tune the current fixed 50ms interpolation window and camera response. Local pair tests are not internet-network acceptance.
4. Next feature implementation: named boss bar, authoritative party portraits and dungeon minimap, keeping the middle clear and retaining overhead vitals.
5. Finish directional walk/attack/hit/death animation, boss/world art and skill sheets; continue weapon balance, two-device equipment/book tests and server-managed loot distribution.

The software touch-control improvement is complete; the real-device acceptance gate is not. Remaining MVP gates from previous revisions remain in effect.

## Previous iteration — revision 24 reference-led HUD and motion

Reviewed browser samples from five My Heroes Dungeon Raid / SEA gameplay videos. Titles, links, sample locations, observed patterns and remaining work are recorded in [the reference review](Docs/MYHEROES-GAMEPLAY-REFERENCES.md). These are visual samples, not full-playthrough or FPS/latency measurements.

Implemented: compact upper-left portrait/vitals, left-aligned draining health/mana fills, slimmer collapsible objective drawer, compact upper-right inventory button, three real skill circles around a larger weapon attack control, radial cooldown shading and readable numbers. PC mouse/keyboard controls remain; the large touch attack control uses the existing aim-stick firing path. Skill contract remains three active slots across two stances, six total; no fake fourth skill. Approved detailed art and 5% pixel treatment remain.

Motion: client-only characters/enemies/projectiles interpolate across the 50ms snapshot period. Teleports above three units and explicit room entry snap. Camera follows with short frame-rate-independent exponential easing, then final 1/64-unit rounding; transitions and large jumps snap. Server damage/movement authority remains unchanged; presentation adds approximately one snapshot of delay and does not predict input. Enemy replica identifiers now stay stable inside a stage when another enemy dies. Identical remote appearance snapshots no longer rebuild all character layers.

Validation: final Windows export `Logs/step-10-20260926-185717.log`. All 302 UI checks PASS, including combat HUD at 1280x720, 1024x768, 1920x1080 and 1600x900; runtime errors false. Reviewed combat screenshot. Earlier HUD label clipping was found and corrected before acceptance. Movement presentation fixture PASS initial no-jump, arrival, teleport and room snap. Combat expansion fixtures and ten-run solo campaign/equipment/save/pause/defeat regression PASS on the same gameplay code before the final text-height-only rebuild. Dedicated pair on final build: Leader and Peer each PASS self-revive, teammate rescue and saved rewards. No reported runtime errors. Account/store suites last passed revision 23; backend Node code did not change this iteration.

Review / next ordered work:
1. Real-device touch QA: drag/hold/release, multiple fingers, switching focus, inventory open/close. Improve fixed stick background and thumb feedback. The touch layout is implemented but not Android-tested.
2. Profile a 10-minute crowded fight on a named low-end PC and Android device. Record frame-time percentiles, allocations, memory and latency. Android editor module/device pipeline is still missing. No measured FPS improvement claim.
3. Test two physical devices with latency/jitter/loss, tune the 50ms presentation window and camera response, consider a bounded jitter buffer or reconciled prediction only with evidence. Local pair testing is not wide-area network acceptance.
4. Add real named boss bar, authoritative party portraits and a dungeon minimap; preserve clear centre and overhead vitals. Current town minimap does not satisfy dungeon navigation.
5. Finish directional character and weapon animations, boss/world art and bespoke skill effects in the approved visual style, then review at actual phone size. This iteration is HUD/motion work, not completion of all animation art.
6. Continue revision 23 weapon/skill balance, two-device book/equipment tests and server-managed loot distribution.

Public MVP remains gated on those device/network/art checks and the existing economy/security/deployment milestones. Normal local services were not stopped; restart both normal server and game to load this build.

## Previous iteration — revision 23 weapon-specific combat and expanded art

User direction: more weapons/items/skills, following My Heroes and Terraria combat feel. Preserve top-down movement and online authority; use distinct aimed weapon patterns rather than making every weapon fire the same projectile. Do not imply this is a Terraria platforming, terrain or crafting implementation.

Implemented content: Tidepiercer spear, Verdant Return boomerang and Emberlock pistol, five catalogue tiers each (15 new gear entries); four tier-2 skillbooks (Tempest Volley, Astral Lance, Crimson Cyclone, Solar Hammers). Total catalogue 140 items / 34 books. Hephaestus sells spears; Artemis sells boomerangs/pistols; Athena sells the new books. Current skill equip remains Primary Q. New content is available through shops; campaign reward tables still award the existing starter gear and are not expanded random loot tables.

Combat: sword/dagger/greatsword/scythe/hammer/shield use directional melee arcs, spear narrow long reach; bows/pistols aimed projectiles, crossbows 3-bolt spread, staffs/wands piercing bolts, boomerangs hit each target once per outbound/return leg. Family-specific cooldowns and damage multipliers are prototype balance. Facing/range, friendly targets, walls and safe zones are checked server-side. New skills: five-arrow volley (14 per arrow), four-target lance (40 per foe), three whirlwind hits (24 each), six rotating hammer sweeps (16 each). Repeated skill effects stop when their owner dies or enters town. Mobile aim-stick fires while held; device verification pending.

Visuals: new 16-cell transparent atlas fills boots, leggings, pauldrons and shield art, adds unique new weapon shapes and four spellbooks/four skill icons. Same approved dense style and 5% coarser point sampling. Pooled directional slash effects replicate to clients; projectile snapshots carry the boomerang visual family. Shared textures, materials and cosmetic budgets retained. Existing animation remains limited; these are distinct combat patterns, not complete bespoke frame-by-frame animation sets.

Validation: content/import `Logs/step-10-20260925-075719.log`, export `Logs/step-10-20260925-075818.log`. UI PASS 298 checks, all descriptions/lore, safe area and cosmetic pool stress. Local combat fixture PASS facing, reach, walls, piercing, return double-hit, safe-zone rejection, volley count, cyclone damage and orbital expiry/damage. Ten campaign runs, equipment/fusion/loot/save recovery/pause/defeat PASS. Dedicated pair Leader/Peer each PASS self-revive, teammate rescue and rewards; original store/account Node suites PASS. No reported runtime errors. The new RPC/snapshot and catalogue require restarting BOTH normal server and game; normal services were not stopped by development.

Review / next iteration: test new-family purchases/equip and each new skill over a real two-device connection (pair regression currently verifies general co-op/revives, not every new skill); balance kill-time, mana and cooldowns at multiple equipment tiers; extend loot distribution through server-managed rules; distinguish bow/gun/ice projectile sprites and add recoil/thrust/spin animation; improve touch targeting on device; finish hair variants and boss/world art. Sustained crowded-combat profiling and Android acceptance remain open. Original gameplay/client/server contracts beyond the documented changes remain.

## Previous iteration — revision 22 approved illustrated art

User approved the generated hero/equipment sheets and requested 5% pixelation. These are now the visual standard; avoid returning to the sparse geometric sprite treatment for replacement assets. Sources and exact generation prompts: `Assets/_Project/Resources/Illustrated/ART-DIRECTION.md`.

Shipped: two transparent 1254x1254 atlases containing nine front-facing racial heroes and sixteen detailed equipment/skill motifs. Runtime presentation now uses these for world heroes/NPCs, portraits, supported gear families, equipped weapons and skill/book emblems. Existing item IDs, stats, slots and transactions remain unchanged. The two new shaders apply 95% linear sampling density with point filtering, preserving original source PNGs. Shared cached atlas slices/materials and previous cosmetic pooling remain. No downloaded My Heroes/Brave Frontier game assets were used.

Scope limits: this replaces the main hero and representative equipment/skill art, not every asset. Boots/leggings, UI glyphs, bosses/environment and directional animation remain older art. Related families/tiers reuse motifs. One authored hairstyle per race currently ships; creator now offers hair colour presets and RGB controls, preserving saved hairstyle values for future sheets. Race-specific colour-mask refinement remains art QA work. Additional hairstyle generation hit the image usage limit; no alternate paid generation path was invoked.

Validation: import/compile passed `Logs/step-10-20260925-071026.log`; Windows export `Logs/step-10-20260925-071121.log`. All 260 UI checks passed, including preserved trail pool stress/memory cap. Solo ten-run campaign/equipment/race-data/fusion/save recovery regression passed without reported runtime errors. Reviewed 1280x720 inventory capture: detailed starter necklace, armour, sword and hero preview render with transparency and readable item frames. One hidden-window creator capture was black; that capture is not visual evidence of creator acceptance. Co-op was last tested in revision 21 and was not rerun for this art-only pass.

Next acceptance: authored hair variants preserving all original choices, race-specific colour masks and creator visual QA, remaining gear/book motifs, directional walk/attack/hit/death sheets, distinct boss and skill animation sheets. Review at real in-game size and on low-end devices before expanding the set. Production-art milestone remains open despite approval of this source direction.

## Previous iteration — revision 21 performance and always-on effects

Player request: keep the neon motion active and optimize for Android/low-end PCs. The attached AI conversation is background advice, not evidence about the internals of My Heroes/Brave Frontier. Existing game startup already targeted 60 FPS; unlimited frame rate was not established as the reported slowdown's cause.

Implemented: removed the old motion-preference option and every runtime read/write of its preference. Neon motion remains active. Reusable scene-owned cosmetic trail pool with 128 desktop / 64 mobile maximum renderers; pooled hit bursts capped at 48 desktop / 24 mobile. At saturation only decorative effects are omitted; damage, projectiles and boss warning circles retain their logic. Shared burst material; cached sprite lists for burst updates and health bars; no per-frame rarity array allocation or unchanged weapon sprite-key regeneration. Off-screen weapon/projectile trails are skipped. Dedicated headless servers continue broadcasting visual events but skip rendering objects. Portrait rendering is limited to 24 Hz, ornament to 30 Hz and HUD readouts to 20 Hz, without changing input/simulation rates. Shadows/vsync are disabled for this unlit pixel pipeline; the existing 60 FPS target remains.

Validation on export `Logs/step-10-20260924-215701.log`: 260 UI checks PASS. Stress requested 12,000 trail emissions; objects stayed at 128, expired correctly, were reused and measured 0 managed bytes during the warmed 10,000-request loop. This is a scoped pool test, not a zero-allocation claim for the game. Ten campaign runs, loot/equipment/save recovery, no campaign revive, pause/defeat PASS. Dedicated co-op Leader/Peer each PASS self-revive, teammate rescue and saved rewards with no reported runtime errors. Normal local server was not stopped/reconfigured.

Performance acceptance still required: establish a named low-end PC and Android device baseline; record CPU/GPU frame time, draw calls, garbage collection and memory over a 10-minute crowded fight. Target a stable 30 FPS minimum on the agreed low-end phone, 60 FPS where hardware permits; targets are not measured results. Android build support is not installed in this Unity editor (only Windows/WebGL), and no phone build/device test was done. FPS speed-up and Android readiness are not claimed.

Next: profile remaining snapshot/appearance rebuilds, inventory full rebuilds and canvas batching; atlas/sprite-sheet original assets after measurements establish draw-call costs; add release-build performance capture and Android module/device pipeline. Keep cosmetic budgets separate from gameplay and never remove boss warnings under load. Art priorities from revision 20 remain active; performance changes do not close that milestone.

## Previous iteration — revision 20 dark surfaces and neon item borders

Latest player feedback supersedes the earlier ornate panel treatment: match the reference hierarchy with dark charcoal/violet surfaces, curved pixel bevels, quiet readable content and bright item-edge ornament. Animate item borders only inside inventory/shop; keep icons and text stable. Higher-tier world equipment gets progressively denser trails and sparks. Retain colourful combat skills and readable boss warnings.

Implemented: clean nine-sliced curved frames throughout shared menus, removing tiled dots and gold corner clutter; darker inventory cells with separately coloured rarity edges; selected-item frame, inset description and rarity-coloured name; circular skill buttons; fixed-budget perimeter sparks/halos at tiers 3–5 (2/5/8 sparks), no particle allocation per frame. The old motion-preference path is absent, so UI particles and world trails remain active. World weapons retain a stable sprite/colour, attack swing and tier-scaled emission; idle colour cycling and idle rotation removed. Existing catalogue icons and hero sprites are unchanged in this increment.

Export: `Logs/step-10-20260924-215029.log`.

Validation: final export passed 259 UI checks with zero failures/runtime errors, including border-only motion, static icon core and click-through sparks. Desktop inventory captures at four resolutions and simulated notch were generated; 1280x720 visually reviewed. The first export of this pass also passed ten campaign runs, loot/save recovery, inventory/equipment/fusion, pause/defeat and no-campaign-revive checks. Final refinements only changed item presentation and UI checks. Dedicated account/co-op were not rerun in revision 20; their last full pass is revision 19.
 Validation results are recorded in PLAYTEST.md. Prior revision's dedicated account/co-op results remain historical; no backend, economy, map or protocol change in this pass.

Review and next iterations:
1. Replace the still-simple weapon/armour/book silhouettes with individually authored detailed pixel assets, retaining IDs and readable forms at 32/64px. Approve a representative weapon, armour and skill sheet against the references before replacing the whole catalogue.
2. Author directional character animation and differentiated spell choreography; do not describe shared burst circles as finished skill animation.
3. Complete real paperdoll/loadout migrations before showing additional gear or skill slots; add compact stat hierarchy, comparison tooltip and priced merchant cards using verified data.
4. Verify border timing and visual density on an actual mobile device and at fractional scaling. Add tier-specific earned cosmetic treatments without increasing combat obstruction.

The new skin improves presentation; it does not finish the production-art milestone. Reference-quality asset detail, stronger character identity and full shop/tooltip composition remain acceptance work.

## Latest visual feedback — 24 September

The six new My Heroes screenshots supersede the previous narrow-faced hero direction. Match the compact large-head/small-body silhouette, strong costume colours, readable weapon silhouette and screen-space sizing with original assets. All characters should have overhead health bars. Maps should scroll with the player, and bosses must differ visibly and mechanically. Move inventory toward hero/details left and collection right; distinguish shop identity, ownership, price and actual stat contributions. Flashy combat must preserve boss warning readability.

Revision 19 implementation: original chibi character layers with larger square faces, smaller tunics/boots, colourful cape and armour, walking bounce and 48px reference-world size. Town expands to 33x19 tiles, dungeons to 29x19, with a grid-snapped bounded follow camera. HUD stays fixed; world NPC labels follow the camera and hide off-screen. All combatants and town NPCs receive overhead bars; enemy/boss bars are red, friendly bars green. Four boss sprites/pattern sets: Osiris Thorn Warden, Ra Sun Phoenix, Sobek Tidal Hydra, Zeus Storm Colossus. Patterns differ in strike radius, windup and recovery; all still use readable area warnings, not four fully unique AI systems. Skill casts/impacts have expanding coloured bursts and sparks, bolts have bright arrow shapes and afterimages. Inventory/shop is a 5x4 right-hand grid with hero, item comparison and upgrade information on the left; shop headings identify the NPC. No fake slots, DPS, set bonuses or currencies were added.

Validation complete locally on final export Logs/step-10-20260924-212504.log: UI PASS 258 checks; local campaign/town/inventory PASS; scrolling=True; four distinct boss IDs exercised. Dedicated Register/Resume each PASS scrolling, town safety, renewed-ticket reconnect, four-stage campaign rewards and pause/rankings. Dedicated Leader/Peer each PASS town checks, self-revive, teammate rescue and saved reward. No reported runtime errors. Dedicated tests now use separate ports 18081/18082/17770 to avoid the already-running normal server. Old normal server processes must be restarted before playing the new build so server and client share updated maps.

Next visual iteration: authored directional walk/attack/hit/death frames, differentiated melee/healing/elemental spell choreography, detailed item icon silhouettes, more boss AI shapes/encounters and the real six-slot paperdoll migration. Current art remains a deterministic prototype pass, not a claim of reference-game production quality. HP bars are implemented; floating damage numbers and a dedicated named boss HUD are still future polish.

## Active overhaul sequence — player feedback and UI references

This sequence takes priority over the older feature pipeline below. Existing economy, backend, campaign and public deployment requirements remain release gates. Reference screenshots guide hierarchy and density; all shipped artwork must be original.

1. **Identity and compatibility:** Echoes of the Rift in title, window, build and launcher; Rift Haven in town copy. Rename internal namespaces/shader paths together. Preserve stable item/account IDs and migrate existing local sessions once before using the new product storage. Keep the installed repository directory as a compatibility path until a separately verified relocation. Mythology catalogue naming must preserve IDs and update server/client catalogue together.
2. **Pixel calibration:** target 32–48px hero height across the reference desktop sizes, starting with 32px at 1280x720. Render at a 1280x720 pixel reference, without stretching the 320x180 buffer. Shrink visual layers only, preserving combat bounds and authoritative movement. Portraits retain a separate inspection scale. Thin slate pixel frames; readable high-contrast type. Verify world and UI at native resolutions, including fractional-scale limitations.
3. **Single-location flow:** splash/authentication -> Rift Haven -> selected expedition. Exactly one active location and one primary interaction overlay, with errors above it. Returning users enter town; new users complete appearance onboarding once. Never leave creator/town menus active behind inventory. Preserve blocking startup readiness, retry and separate server startup. Add explicit screen ownership and modal-close input rules before replacing town controls.
4. **Rift Haven:** replace the menu with a walkable shared safe zone, WASD and touch joystick. Reject attacks and damage on the server, clear hostile entities, replicate town movement. North: 4/8-player raids; south: campaign; east: boss DPS trial; west: world boss. Unimplemented modes remain labelled unavailable, never silently launch another mode. Physical NPCs: Hephaestus (melee/heavy), Artemis (ranged/light), Helios (magic), Asclepius (support), Athena (skills/passives). Interact by proximity/tap; expose verified services only. Preserve the current two-player co-op fixture until larger raids are implemented.
5. **HUD and overlays:** top-left compact avatar/name/level/HP/MP; top-center server-backed gold/gems/keys; top-right 64px minimap, settings and bag. Do not fabricate level or currencies. Bottom-right attack plus four skill slots needs a loadout contract migration from current six skills/two stances. Inventory: left paperdoll/pedestal and six gear slots, right 5x4/5x5 rarity grid. Migrate current three-slot profiles before activating new equipment slots. Add detailed slate item tooltip with real stats, stars and mythology name. Skills: four numbered runic loadout slots and 4x3 book grid; elemental icons and cooldowns. Original high-detail art, animation and earned cosmetic milestones remain separate acceptance work.

Screenshot mapping: 1–3 compact world HUD and town readability; 4–5 layered item statistics/tooltips; 6–7 loadout beside skill catalogue; 8 paperdoll beside equipment grid; 9 combat readability; 10 earned avatar/frame collection; 11 clearly priced shop cards. These examples do not override the gold/skill-gem economy decision.

After every increment: compile, run affected account/gameplay/UI regressions, inspect screenshots, record limitations and revise this section. MVP gate: a fresh player can connect, create a hero, walk town, select a working mode, finish a boss, understand loot, equip/upgrade, reconnect without lost rewards and use co-op revives correctly. Mobile device acceptance, public transport security, server balance configuration and production art remain outstanding.

Previous increment: title/window/export/launcher branding, runtime namespace and shader names updated. Windows local storage migrates once without replacing newer files or restoring a signed-out token. Repository folder, historical logs, legacy migration literals, test flags and server environment variable names retain compatibility identifiers. Mythology catalogue display names are still pending. Compact hero rendering and thin slate frames implemented; inventory suspends town/HUD/status and restores them on close. Escape closes inventory before opening pause. Safe-zone world and four labelled gates are implemented in revision 17. Full location ownership, six-slot inventory and four-slot skills remain incomplete.

Review findings: combat screenshot confirms the smaller hero but enemies/environment still use the older visual density; calibrate and redraw these together in the next art increment. Existing procedural icons remain below reference quality. Inventory now uses five columns and the left/right reference arrangement; six-slot paperdoll data migration remains open. HUD still uses three active skills/two stances; do not claim the requested four-slot interaction is finished. Fractional UI scale and actual phone/gamepad acceptance remain open.

## Town foundation acceptance history (revisions 17–18)

Implemented: server-side movement in town while combat actions and damage are suppressed; town snapshots no longer trigger a dungeon load. Town location unloads before campaign/co-op. Town has Hephaestus (melee/heavy catalogue), Artemis (ranged/light), Helios (magic), Asclepius (support accessories), Athena (books). Existing buy/equip/fuse transactions remain authoritative; selling and new passive training are not implemented. NPC taps require proximity; F/interact works for nearby destinations. Existing mobile move-stick bindings are retained, with device testing pending. Unavailable gate messages do not start a different mode; north offers the separately labelled two-player test.

Acceptance result: `Logs/step-10-20260923-234542.log` built successfully. Final UI audit PASS: 258 checks, zero failures, no runtime errors. Native 1280x720 and 1024x768 town captures reviewed: labels stay readable/in bounds and the north sign no longer overlaps the header. Increased sign height, clamped world labels to safe bounds, reserved header space, and hid combat Objectives controls in town. The Objectives drawer now retains the player's collapse choice during combat.

Local regression PASS: town movement/safety, Athena-only book catalogue, unavailable gate dialog, ten complete campaign runs, boss-only completion, no campaign revives, loot/save recovery, inventory filters, pause/resume and defeat. Testing caught merchant state leaking into later bag opens; closing inventory now resets that state.

Dedicated Register and Resume PASS: server town movement and damage/skill/dodge protection, renewed-ticket reconnect, all four campaign stages, central rewards, pause and rankings. Dedicated Leader and Peer PASS: both complete town checks, then one self-revive and one teammate rescue each, with saved reward and no runtime errors. Added an explicit test readiness barrier so a raid cannot start before both clients finish town checks. These are local-process tests, not separate-device or public-network certification.

Remaining presentation limitation: the development player warns when 1024x768 is below the 1280x720 pixel-camera reference, and world framing is tighter. The UI checks pass there; native-resolution camera framing below the reference and real mobile scaling still require work. Town art remains a functional procedural pass. Gate collision/bounds extremes and hostile-input penetration testing are not comprehensively certified by these regressions.

Next iteration: mythological display names preserving item IDs, original NPC/item art and consistent environment density; peer inspection and visible player names; full single-overlay ownership including rankings/pause; six-slot profile migration/paperdoll, then four-skill loadout migration. Larger raids and independent expedition instances require server architecture work; the current shared server still supports two players and one expedition, so solo campaign requires only one connected player.

## Review of the build we actually have

Status: playable Windows/local-server prototype, not the requested finished online RPG. Desktop Play Echoes of the Rift.exe starts only the player. A separate Echoes of the Rift Server.exe controls the server. Keep the workspace at its installed location. No public server is deployed.

- Combat: movement, dodge, two skill stances, four boss-led campaign stages and two-player co-op. Campaign clears on boss death with small mobs optional; campaign has no revive. Co-op supports the blank-ad self-revive and nearby teammate rescue; full wipes reject both.
- Presentation: original procedural 32px layered heroes, nine race choices, RGB colours, Tiny5 pixel text, frames, HUD, icons and tier trails. These are a functional art pass, not production-grade reference quality. Mobile readability and native-window scaling are not certified.
- Items: 121 catalogue definitions including 30 generated books; inventory quantities, inspection of own equipment, shop, sorting/filtering and fusion. Only three gear slots (weapon/armour/charm), primary-Q book selection and a five-tier fusion cap. Catalogue breadth is not proof of balance or unique finished art.
- Accounts: local username/password registration/sign-in, Windows-protected rotating login sessions, single-use game tickets, recovery codes, sign-out and ownership-checked legacy linking now exist. Remote account access is blocked pending HTTPS/encrypted transport. Android/iOS secure storage and public account-service hardening remain open.
- Town: Rift Haven now has an original runtime tilemap, five physical NPCs, server-replicated movement for the existing two-player capacity, safe-zone attack/damage protection, gold display and a 64px position map. NPCs open specialist catalogues. South launches campaign; north exposes the explicitly labelled current two-player co-op test. 4/8-player raids, DPS trial and world boss remain unavailable. Rankings show saved profiles, not live player inspection.
- Community: CommunityForumManager.cs has local text/loadout rendering helpers. There is no published forum, upload service, moderation system or functioning community terminal.
- Backend: dedicated authoritative game process plus private Node/SQLite service; central profiles and atomic transactions with deduplicated stage receipts. Rewards/prices still include hard-coded rules; catalogue is a JSON file. Gems, achievements, durable reward inbox, admin grants and database-managed balance versions do not exist.
- Scale: one expedition per process, two players. No four/eight-player raid queues, survival, guild membership system, 50-level chapter campaign, real ads or real-money purchases.

Prior full regression baseline: revision 19, export log `Logs/step-10-20260924-212504.log`. Current revision 20 export is recorded above. Executable: `Builds/Windows/EchoesOfTheRift.exe`; desktop launchers use this build. UI, solo/town, dedicated account and dedicated pair results are current and passed; details above.

## Deployment direction - latest user instruction

Use this PC as the separate server for now. The player must never start a backend automatically. Every normal launch checks account/game/database readiness; an unavailable server produces a blocking connection-error modal above the splash with Retry and Quit. The server control window remains independent of the game window. Accounts, inventory, currencies and rewards remain server-owned; local assets, settings, endpoint configuration and protected login tokens still exist on player devices.

Implemented: separate desktop server controller; client-only launcher; account-side readiness backed by private game heartbeat plus database query; startup popup at sorting order 1000 over splash 600; retry without server startup; endpoint schema with gameplay address and account URL. No router/firewall rules or public hosts were changed. See Server/DEPLOYMENT.md for requirements and Caddyfile.example for the account-proxy template.

Before Oracle: install appropriate Linux server build support, validate the chosen CPU architecture (A1 is Arm; current build is Windows x64), implement encrypted gameplay and HTTPS account deployment, and measure capacity. Keep private persistence port 8081 and database files off the internet. Current client remote-account guard stays in place until secure transport is ready. Public non-development client packaging/server-code separation and database-managed values remain open.

## Binding direction and decisions

1. Aim for original high-detail pixel art with My Heroes UI readability and Brave Frontier character richness as references. Do not copy their assets. Use consistent 16px/32px design grids, authored silhouettes, shading, animation, readable rarity and icon families. Larger assets may use multiples of that grid; merely enlarging existing sprites is not the overhaul.
2. English is the initial UI language. Centralize player-facing strings; separate internal IDs from displayed text. Pixel typography must remain readable at actual mobile and PC sizes, not just in screenshots.
3. The later economy instruction takes precedence over the earlier suggested gem spending: gold funds gear upgrades and merchant purchases; gems are reserved for skill-book upgrading. Do not add gem purchases for cosmetics, keys, gear materials or randomized draws without a new design decision. Keys remain earned access items, not a third currency. Merchant book purchases, if offered, use gold and are balanced against their rarer drops.
4. Gold and gems are earnable in every implemented mode and through achievements. Gold is substantially more frequent; gear is easier to obtain than books or gems. No mode becomes paid-only through raid keys.
5. Separate item rarity/visual tier (five current categories) from an uncapped progression level. Character level is also separate and needs its own XP design. Preserve existing inventories with a versioned migration; do not silently reinterpret T5 items as five levels with lost value.
6. Cache a revocable session credential locally, not a plaintext password. Username/password registration authenticates over encrypted transport; store salted password hashes on the server. Device storage contains account metadata plus OS-protected session tokens (Windows protection / Android Keystore-backed storage). Application.persistentDataPath is a location, not encryption. Android paths must come from the platform API, not a hard-coded Android/data path.
7. Online authority belongs to the remote server and database. The local launcher remains a development/playtest environment with its own economy. Local progress must not be automatically trusted by a public server.
8. Server-only configuration prevents client tampering and makes tuning easier; it cannot guarantee players never infer drop rates. The distributed local server is inspectable. If paid randomized items are introduced later, review applicable storefront odds-disclosure requirements rather than promising secret paid odds.
9. Selling gems used for book power upgrades creates a paid progression advantage. Balance with fully earnable supply, measured power curves and, if needed, normalized competitive modes; do not describe this as purely cosmetic. Validate the advantage with testing before monetization.

## UI, item identity and earned recognition - priority design direction

The user's emphasis is game feel through UI and item design. Every screen must make selection, cost, improvement and ownership obvious. Show item art prominently, expose understandable stat differences, explain upgrade requirements before action and preserve readable combat beneath effects. Do not confuse a UI polish increment with the production-art milestone in step 4.

Research direction: competence, autonomy and relatedness are useful motivation principles, not a guaranteed retention formula. Apply them through visible mastery, meaningful loadout choices and social recognition. Measure whether players understand the game and enjoy returning, rather than optimizing session length alone.

- Tie earned cosmetic unlocks to item progression and verified campaign accomplishments. Maintain separate cosmetic entitlement records once progression migrates; previews explain the precise unlock and never imply an unearned item is owned. Preserve current T3 animation/T4 trail/T5 mythic hints while the five-tier prototype exists; final uncapped-level thresholds belong in database configuration.
- Plan campaign titles, profile borders and inspectable trophies for genuine boss/milestone achievements. Build recognition around mastery, helpful co-op participation and build creativity as well as progression. Baseline attack tells and accessibility must never be locked behind levels.
- Design opt-in competitive boards with explicit scoring, comparable rules and server verification. Separate normalized skill competition from raw progression boards; purchases must not silently determine a supposedly skill-based rank. Do not fabricate competitors, ranks or popularity.
- Prefer permanent earned rewards, personal bests, achievable next goals and social loadout sharing. Avoid punishment for taking breaks, fake scarcity or irreversible streak pressure. Cosmetics can be impressive without obstructing other players or forcing animation delays.
- Validate with first-time players: can they identify the selected item, predict equip/upgrade effects, find their next goal and explain what a cosmetic milestone recognizes? Track voluntary return, satisfaction, build diversity and perceived fairness. Telemetry and experiments come later with clear privacy choices; none are currently installed.

Primary references: https://selfdeterminationtheory.org/SDT/documents/2010_PrzybylskiRigbyRyan_ROGP.pdf (game motivation framework); https://learn.microsoft.com/gaming/accessibility/xbox-accessibility-guidelines/101 and https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102 (text/contrast guidance). These inform design hypotheses and readability checks, not claims that this prototype is accessible-certified or proven to retain players.

## Sequential implementation pipeline

Execute steps 1 through 8 in this order. Define shared data contracts early, but do not activate real payments, ad rewards or community publication against placeholder services. Steps 5 and 6 can be completed as tested integration-ready features before step 8; their live release gate depends on the authoritative infrastructure in step 8. Finish each step with compile, relevant tests, visual review, findings and a plan revision.

### 1. English strings and crisp pixel typography
Status: in progress. First increment shipped: EnglishUI inventory presentation catalogue, selected-item icon and border, comparison with equipped gear, explicit upgrade requirements, weapon cosmetic milestone hints and separate lore view. Shared Canvas now uses pixel alignment and expands its reference space to avoid aspect-ratio cropping; TMP does not auto-shrink or interpret player strings as rich-text tags. These settings are not a claim of perfect pixels at every fractional scale.
- Added EnglishScreens with 57 centralized fixed labels/prompts/error prefixes across creator, town, combat, pause, reconnect and rankings; HUD/town/ranking and inventory value formatting is centralized in EnglishUI. Some combat-specific interpolated copy and authored content remain to migrate.
- Added shared safe-area fitting and menu keyboard/gamepad-entry focus with visible selection colours. Native screenshots were captured at 1280x720, 1024x768, 1920x1080 and 1600x900; a simulated asymmetric notch was also checked. Historical pre-town audit passed 253 screen/item/lore checks with no text overflow or off-screen labels (255 after revision 16). Revision 17 introduced 38 town layout findings; revision 18 resolves them and passes 258 checks. Keyboard focus was exercised; physical gamepad and mobile touch were not.
- The audit found and fixed undersized equipped badges and lore button text boxes. Native creator and inventory/notch captures were visually reviewed.
- Remaining Step 1 gates: migrate residual dynamic combat copy, validate physical gamepad/touch and real phone readability/safe areas. Portrait reflow is not implemented; landscape is the current game layout. Do not label simulated checks as Android/iOS device certification. Local protected-session registration is implemented as described in Step 2; public encrypted account/game transport remains pending.
- Create centralized English string keys/constants for menus, errors, item presentation and tutorials. Retire outdated two-copy/T5-only library text as the corresponding mechanics change.
- Set a common TMP pixel-font atlas/import policy, point-filtered art, grid-aligned borders and consistent spacing. Avoid fractional scaling that blurs pixel edges. Use responsive layouts, safe areas and readable font sizes instead of forcing an entire PC canvas onto a phone.
- Check button targets, long names, counters, high contrast, text wrapping, keyboard/gamepad focus and touch operation. Decorative title lettering must not replace readable body text.
Acceptance: no clipped labels at selected PC and mobile resolutions; actual-device readability review; screenshots at native scale; gameplay and launcher regressions remain green.

### 2. Splash screen, registration and automatic authentication
Status: implemented for the local Windows prototype. SplashScreenUI.cs offers a glowing enter prompt and registration/sign-in and recovery. New heroes continue to character creation; returning/linked heroes keep their saved appearance. Windows DPAPI protects the rotating refresh token. The loopback account API uses salted scrypt hashes, request/concurrency limits, two-minute single-use game tickets and 30-day renewable sessions. Passwords do not travel through the game RPCs or persist in device storage.
- Backend tests cover registration, uniqueness, wrong password, refresh rotation/expiry, ticket replay/expiry, recovery/revocation, legacy profile ownership, saved loot, concurrent registration and HTTP limits. UI account tests register, load protected sessions after process restart, connect to gameplay and save four campaign rewards. Reconnect validation and final export are recorded in EXECUTION-LOG.md.
- Gates still open: HTTPS and encrypted remote transport, Android/iOS secure storage, public abuse controls, immediate revocation of already-connected gameplay sessions and real-device checks. The active gameplay session is not yet forcibly disconnected when a password is recovered. Local completion does not imply those release requirements are done.
- Current development milestone is Step 3: accept the implemented town/NPC foundation, then add player inspection. Keep the Step 1 physical-device checks alongside this work; do not claim device certification.
 - Animated original pixel title scene with glowing Click / Tap to Enter. The motion-preference option is intentionally absent; animations and neon effects remain enabled.
- Start silent authentication for returning accounts while the splash is visible. Enter transitions to the town only when authenticated. New players see username/password registration plus sign-in for an existing account; new accounts then create their character.
- Implement unique normalized usernames, password validation, secure password hashing, rate-limited login, short-lived sessions, rotated refresh credentials, logout/revocation and a recovery flow. Never log passwords or tokens.
- Account service verifies credentials then loads/syncs the DB profile. Reconnect uses renewable credentials and retrieves authoritative progress, including pending rewards. Handle offline, expired credentials, duplicate usernames and failed registration without creating duplicate accounts.
- Link legacy device accounts only after proving ownership; preserve current local saves. Cross-device login must not let a client upload arbitrary progression.
Acceptance: new registration, returning auto-login, invalid password, restart, expired/revoked token, connection drop and legacy migration tests. Real username/password authentication must not use the current unencrypted game transport.

### 3. Multiplayer town and player inspection
Status: town foundation implemented and locally accepted in revision 18. TownHubManager.cs builds the walkable town and specialist interactions; replicated movement and safe-zone protection passed local dedicated tests. PlayerInspectorUI.cs, larger town instances and the later services below remain planned.
- Implemented foundation: walkable tilemap, WASD/existing mobile-stick binding, five NPCs and two-player town snapshots. Still needed: separate-device acceptance, real touch-device validation, visible player identity and separate town/expedition instances with explicit capacity/routing.
- Click/tap a player to view a server-approved public snapshot: name, character level, helmet, armour, weapon, accessories, equipped books and stat breakdown. Exclude private inventory, credentials and account data. Support leaving/disconnecting players gracefully.
- Merchant buys/sells gear, consumables and books for gold. Define safe sale rules for equipped/locked items and preview sale value. Forge upgrades gear/books with a clear cost/result preview and visual progression.
- Cinema/TV station: strictly optional, maximum 50 verified rewarded views per account per UTC day, across devices. Server time, unique ad receipts and atomic counters prevent replay or double reward. Award configured bonus gems and earned raid keys only after verified completion. No forced ads and no default autoplay. Keep the current co-op blank-ad revive separate; it does not count as a cinema view or grant currency.
- Guild/raid portal exposes solo campaign, 4-player raid, 8-player raid and survival queues as those modes become playable. Existing two-player co-op remains available during migration. Guild membership/social tools are a separate extension, not implied by drawing a portal.
Acceptance: two real devices see movement and inspect each other; NPC transactions survive disconnects; 50th view succeeds and 51st fails across concurrent devices; portals never advertise nonfunctional modes as available. Capacity tests must cover 4/8-player expeditions before those queues ship.

### 4. High-detail pixel art assets
Status: planned; replaces prototype art incrementally.
- Produce an art style sheet and one finished vertical slice first: hero, enemy/boss, NPC, weapon, book, consumable, skill, panel, button and quest drawer.
- Establish consistent palette, outlines, material shading, silhouette detail, icon legibility and animation timing. Preserve distinctive race modules and RGB customization.
- Expand the approved style across all units, gear, skills, NPC overlays, splash/town scenery and UI borders. Distinct bosses need attack tells and hit/death animation; heroes need walk/attack/dodge frames. Use texture atlases and measure mobile memory/performance.
- Keep higher-tier glow readable and restrained. Upgrade level, rarity and cosmetic trail thresholds must remain separate.
Acceptance: review native-size and enlarged captures, moving combat footage, crowded town readability and low-end-device frame time. Do not mark production art complete based only on generated asset count.

### 5. Chapter milestone gem offer
Status: planned. New ChapterRewardShop.cs; real purchases gated on steps 7/8 and storefront integration.
- Expand story structure to 10 levels per chapter, 5 chapters per milestone (50 levels). Current four stages are a prototype slice and do not satisfy this content requirement.
- Proposed tally rule: count server-recorded gems earned from first-clear story rewards within each 50-level block. Replays, ads, purchases, support grants and unrelated mode rewards do not inflate the tally. This is a provisional interpretation of chapter-earned gems; expose the exact rule in the UI.
- Freeze tally G on milestone completion. Offer one of three mutually exclusive packs: base USD 5 gives G additional gems; value USD 10 gives 2G; mega USD 20 gives 5G. The player keeps all previously earned gems. One purchase total per milestone/account, not one of each pack. Later milestones create new offers. Allow dismissing and reopening an unpurchased offer; do not fabricate urgency.
- Server stores milestone, tally, offered products and claimed receipt. Validate store transactions and idempotently deliver rewards after pending/confirmed purchase handling; recover after crashes and duplicate callbacks. Display actual localized storefront pricing. A claimed discount requires a defensible standard price comparison; otherwise label it a milestone offer.
Acceptance: exact tally boundaries at levels 49/50/51 and 99/100, one-of-three purchase locking, replay exclusion, zero tally, failed/cancelled/pending payment, duplicate receipt and restore/reconciliation. Sandbox only until live economy and payment gates pass.

### 6. Community terminal
Status: planned. New CommunityForumUI.cs using existing rendering helpers where suitable.
- Town library supports writing/publishing builds, loadout snapshots, equipment synergies, boss strategies, wiki/stat mathematics, and math/computer-science/game guides. Include search, revisions and authorship.
- Store lightweight sanitized Markdown in the database; support a documented safe subset. No arbitrary HTML, executable content or uncontrolled embedded external images. Loadout snapshots include balance version so outdated advice is identifiable.
- Enforce a maximum of five attached images per post on the server, including edit and concurrent-upload paths. Server decodes, validates and re-encodes uploads, strips metadata, preserves aspect ratio and constrains each dimension to at most 1024 pixels. Define input byte/decompression limits and an output byte budget. Store optimized media in object/file storage with metadata in DB; paginate/lazy-load. Dimension limits alone do not ensure instant mobile loading.
- Add author permissions, draft/save recovery, report/block, moderation/removal and upload quotas before public publishing.
Acceptance: sixth-image rejection, malformed/oversized file rejection, safe Markdown rendering, access controls, edit races, compression size/dimension assertions and mobile loading measurements.

### 7. Economy and drops
Status: planned; replaces hard-coded gold-only five-tier fusion.
- Exactly two currencies: gold and gems. Gear upgrades consume duplicates plus gold; skill-book upgrades consume book duplicates plus gems. Both currencies and relevant drops are earned in all playable modes and from server-verified achievements.
- Proposed duplicate formula for upgrading to target level L >= 2: D(L) = 5 * ceil(L / 100). Targets 2-100 cost 5 copies, 101-200 cost 10, 201-300 cost 15, and so on. Test 100-to-101 explicitly. Retain the upgraded item and consume additional matching base copies; do not require recursively creating five equally upgraded items. Duplicate matching identity/rarity is defined in DB configuration.
- No designed level cap. Use safe large-number storage/calculation, validated arithmetic and scalable display formatting; reject overflow without consuming resources. Distinguish practical numeric bounds from gameplay progression rules.
- Prototype tuning proposal, not final balance: upgrade resource cost C(L)=ceil(baseCost * L^1.15), independently configured for gear gold and book gems; stat gain baseStat * (1 + 0.02 * (L-1)^0.75). Simulate and adjust coefficients before shipping. Multiplicative interactions, healing, cooldowns and damage reduction require their own bounded rules.
- Configure gold-common, gear-common, gems-rare and books-rarer weighted pools by mode/stage/difficulty. Specify roll opportunities, quantities, guarantees and any pity tracking. Set actual probabilities from targets for minutes per upgrade at early/mid/high levels, not arbitrary untested percentages.
- Measure duplicate acquisition bottlenecks across the broad catalogue; support targeted earned rewards if simulations show impossible progress. Prevent gold merchant resale loops and measure paid gem acceleration against free progression.
Acceptance: deterministic seeded simulations and distribution confidence checks, long-run progression estimates, boundary costs, inventory migration, all-mode earnability, no negative balances and atomic competing upgrade requests. Publish a balance report before calling the economy ready.

### 8. Database configuration, authoritative rewards and operator recovery
Status: partial foundation only; implement and migrate before activating live economic features.
- Store all tunable gameplay/economy values in versioned DB configuration: drop weights/quantities, rarity/level curves, gold/gem sources and sinks, NPC prices, skill/item stats, XP, achievements, ad caps/rewards, key rewards and chapter products. Rendering assets remain in the client; publish only necessary display/quotation data, not privileged reward rules. Core validation logic still lives in server code.
- Validate draft configuration, preview/simulate, activate a version and support rollback. Pin an expedition to a version so a mid-raid rebalance cannot change already-earned rewards. Audit who changed what and when. Server caches have explicit invalidation/versioning.
- Plan normalized records for accounts/sessions, characters/equipment/books, currency ledger, balance versions, drop tables, achievements, reward events/receipts, inbox claims, milestone offers, purchases, ad receipts/counters, guides/media and admin grants.
- Server rolls rewards from verified gameplay events. Persist event ID, account, source, balance version and rolled result BEFORE acknowledging completion. Retries reuse the persisted result; disconnects must not permit rerolling. Atomically commit currencies/items/achievement state plus receipt, or use a durable outbox to retry across services.
- Maintain an unclaimed reward inbox and reconcile on sign-in. Exactly-once economic effects come from unique event keys and atomic claims, not assuming delivery happens once. Extend current stage receipts to achievements, survival checkpoints, raids, ads, purchases and compensation.
- Provide an authenticated operator tool to locate missing rewards and grant gold/gems/items with reason, actor, audit trail and unique grant ID. Resending the same compensation cannot duplicate it. Avoid manual blind edits to a player's profile JSON.
- Backups, restore rehearsal, migrations, least-privilege service access, encrypted external transport, monitoring and load tests precede public deployment. Select public host/DNS and account recovery channel during deployment planning; none is currently configured.
Acceptance: crash/disconnect before and after commit, duplicate/out-of-order events, concurrent claims, database outage/restart, backup restore, operator grant replay and configuration rollback. Clients cannot submit amounts, roll results or arbitrary completed achievements.

## Release gates and work deliberately still open

- Preserve current boss-only completion, campaign no-revive and co-op rescue rules throughout the migration.
- Do not call a menu prototype a multiplayer town, 121 templates production art, or a purchase modal a functioning purchase system.
- First expanded playable milestone: readable UI -> secure onboarding -> shared town/inspection, using existing campaign/co-op content. Then finish art and integration-ready chapter/community features in the stated sequence.
- Public monetized release requires all eight steps plus real-device recovery, balanced progression, verified purchase/ad delivery, moderation, 4/8-player scale tests where advertised and operator recovery tooling.
- Fifty story levels, distinct bosses, survival and 4/8-player raid encounters require separate content production and testing within this roadmap; screens alone do not complete them.
- Mobile joystick bindings and shared safe-area handling exist, but real-device input/layout acceptance, profiling and mobile exports remain unfinished. Desktop launcher works only with the installed workspace/Node runtime; portable installer/update packaging remains open.
- After every step: record changes, tests and failures in EXECUTION-LOG.md; revise priorities and acceptance findings here. Preserve an honest distinction between planned, implemented, locally tested and released.

## Reference notes

- Art/UI references: https://myheroesofficial.fandom.com/wiki/Skills_(Information), https://myheroesofficial.fandom.com/wiki/Quests_(UI), https://www.bravefrontier.jp/library/bf2/bf2_list.php . Existing Tiny5 license is bundled with the font.
- Password storage guidance: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html . Use an adaptive password hash such as Argon2id for human passwords, not the prototype's fast hash of a random secret.
- Session/authentication guidance: https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html and https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html .
- Storefront reference for future paid random rewards: https://developer.apple.com/app-store/review/guidelines/ . Verify platform requirements again when integrating purchases.
