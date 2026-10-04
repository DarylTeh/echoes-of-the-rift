# Echoes of the Rift — remaining roadmap

Revision 64, 4 October 2026. This is the current checklist; historical build notes live in EXECUTION-LOG.md and MASTER-PLAN.md. Seven major milestones group the work; milestone 1 is locally accepted and six remain open. They are workstreams with many smaller tasks, not seven quick patches or a release-date estimate. The progression model uses fixed star rarity plus enhancement, live events are reusable, server-timed and progress-aware, event details consume version-pinned status with authoritative progress and milestone eligibility, the Windows release is clone-ready with portable launchers, low-end runtime allocations have been reduced, the local playable stack is verified, OCI authentication and the network foundation are provisioned while VM capacity is pending, the award-informed player-experience backlog is prioritized, Rift Defense is in source implementation after a 20-video co-op gameplay study, the UI safety/presentation audit is a release gate for every screen and mode, event rewards have a recoverable transactional inbox, account passwords accept five or more characters, the fresh-clone handoff includes portable install/start/play commands and machine-independent OCI path resolution, server/database hot paths have bounded-work safeguards, and the latest optimized release passes desktop/touch layout and local account/campaign regression. Public deployment, physical device performance and 90% My Heroes visual parity are still open gates.

## Current iteration - revision 64, 4 October 2026

Completed the latest optimized release, account/campaign tests and repository cleanup; the existing release remains the last verified Windows release player. Revision 65 built a separate Windows development player and passed a 20-wave/four-boss stress test plus general runtime smoke checks. The automated defense run uses a full grid of rank-5 towers, so ordinary balance/duration and visual acceptance remain open, as do Android/low-end-PC measurements. Rift Defense findings and next gates are in `Docs/RIFT-DICE-DEFENSE.md`.

## Historical build note - revision 49, 2 October 2026

Skill HUD stable labels/art update only when changed, and active event manifests use a one-second invalidated cache. Cooldown and event schedule correctness remains authoritative.

## Current build - revision 48, 1 October 2026

Supporting SQLite indexes cover expiry cleanup, player inbox/event access and audit history. Overhead vitals cache expensive bounds/value work while retaining smooth positioning. Full server tests cover the schema additions.

## Current build - revision 47, 1 October 2026

Unity files, server source, deployment scripts, Windows executable payload, launchers and endpoint notes are inside the repository. No public IP is available yet; OCI VM capacity remains the deployment gate.

## Current build - revision 46, 1 October 2026

Account registration and recovery accept passwords from 5 through 128 characters. The account form uses the same requirement, and account tests cover the five-character boundary.

## Current build - revision 45, 1 October 2026

`Release/Windows` is tracked for the playable Windows handoff and excludes debug symbols. `Release/README.md` documents the first-run Node dependency install, server/player launcher order and intentionally local runtime data. Tests resolve this release payload first, then use `Builds/Windows` when working from an editor-generated build.

The co-op snapshot path, HUD refresh path and safe-area path now avoid repeated temporary allocations and redundant work. Quality defaults target Android/iPhone Low and standalone Medium while preserving the fixed neon effects policy. The local server/client stack is running, while both Always Free VM shapes are out of host capacity in Singapore; public deployment remains gated by Unity activation, a compatible Linux server build, encrypted gameplay transport and measured device/server tests.

The town Events drawer consumes active server-timed manifests and renders safe countdowns. The Reward Inbox lists and idempotently collects server-issued currency/item rewards.

The server now stores versioned UTC event manifests and historical revisions, exposes active events and server time through `POST /events`, returns version-pinned player status through `POST /event-status`, records idempotent claim receipts and transactional inbox entries, and provides authenticated local Swagger list/publish/disable operations. Verified campaign reward transactions advance matching active event metrics once per receipt; progress is capped, mode-filtered and stored per event version/player. Milestone claims are rejected until their configured threshold is reached. Unity renders the returned progress beside the existing claim status in the Events drawer. Reusable templates cover login calendars, token exchanges, Boss Challenges, community goals, collaboration packs and Tower Defense seasons; the operations guide catalogs the remaining reusable modules and publish/rollback safeguards. The next live-ops slice is module-specific reward delivery and event-shop spending. The first-ten-minute, combat-readability, collection-recognition, social and device-quality priorities are recorded in [FUTURE-DESIGN-BACKLOG.md](Docs/FUTURE-DESIGN-BACKLOG.md), with the tower mode in [RIFT-DICE-DEFENSE.md](Docs/RIFT-DICE-DEFENSE.md).

## Historical build - revision 30, 28 September 2026

Star rarity and enhancement are now presented independently in inventory, shop and item inspection. Gear and skill cost curves, campaign target power and the bounded late-game plateau are recorded in [Docs/PROGRESSION-MODEL.md](Docs/PROGRESSION-MODEL.md). Server/client profiles now normalize separate progression fields; the uncapped separate-stack transaction remains milestone 4.

Implemented the reference-led collection redesign: separate equipment/hero and five-column backpack panels, real equipped gear and six current stance skills, visible category tabs and a sort-choice menu. Items open in a dimmed inspection popup with rarity ribbon, large icon, real stat comparisons, story and Equip/Upgrade actions. Back closes the popup first and retains the browsing page; background controls stay blocked. Starter skills have a read-only inspection view. The merchant now uses a separate wide three-card layout with real gold prices and owned counts; inspection precedes Buy.

Dark double-bevel panels, red close controls and tier-only border ornament match the documented reference hierarchy more closely. Existing approved item/hero art is retained. Shop browsing suspends the preview camera; full-screen dimming covers taller aspect ratios and safe-area offsets. The server now normalizes progression fields while preserving legacy catalog tiers.

Validation: final Windows export `Logs/step-10-20260928-192803.log`; strict text encoding PASS, 283 authored files. Desktop UI PASS 342 checks and touch-layout PASS 343 checks at four sizes, zero failures/runtime errors. Server persistence/account/admin/progression suites, full solo campaign/save-recovery regression and the legacy storage guard all pass. Physical Android testing remains open. The 1024px-wide development capture still shows the existing world-camera pixel-reference warning; it is not a UI overflow failure and lower-resolution world rendering remains a device-acceptance item.

Reference coverage and unknown flows are recorded in [the UI audit](Docs/MYHEROES-UI-AUDIT.md). Core inventory/shop/profile samples and two available wiki pages received a second pass; matchmaking and unobserved social flows are explicitly not claimed as fully verified. Six-equipment/four-skill migration, saved loadouts, complete quest/profile/social screens and production world animation remain open. Six broad roadmap milestones remain open. Reopen **Play Echoes of the Rift.exe** to load this build.

## Historical build - revision 28, 27 September 2026

Fixed malformed UTF-8 punctuation throughout the authored UI and repaired affected project documentation. Added a strict UTF-8/mojibake build gate plus runtime visible-label checks; `.editorconfig` specifies UTF-8. The gold/safe-zone, level/gems, movement/interact and gate strings no longer contain the stray accented A.

Reworked the reference-led HUD: cached pixel coin, faceted cyan gem and cog icons; top-right wallet counters, level below the upper-left portrait, backpack down the right edge, a separate safe-zone subtitle, smaller merchant service bubbles with nearby-only names, and matching inventory/shop currency headers. Boss/map spacing was adjusted around the wallet. Dark rounded surfaces remain; wallet labels update only when values change. Original approved hero/item art and combat contracts remain intact.

Moved sign-out to Settings > Accounts with a separate confirmation and Stay in game selected by default. The overlay blocks movement and bag interaction. Back first cancels confirmation, then closes settings and restores controls. Controls and Accounts tabs are available; no sign-out action remains on the town HUD.

Validation: Windows export `Logs/step-10-20260927-201608.log`; source encoding gate PASS (282 authored files). Desktop layout 319 checks and touch layout 320 checks passed across four sizes, zero failures/runtime errors, before the final NPC-height-only correction. Final build touch checks also PASS 320 with zero failures/runtime errors; final town capture confirms bubbles clear NPC faces. Authenticated registration and resume runs passed account cancellation (same token and connection), reconnect, campaign, server-only rewards, pause and rankings with no runtime errors. Native town, inventory, combat and Accounts/confirmation screenshots reviewed. Physical Android input/performance acceptance remains open.

References: [MyHeroes review and wiki links](Docs/MYHEROES-GAMEPLAY-REFERENCES.md). This implements the HUD/settings feedback, not a complete recreation of MyHeroes. Remaining visual work includes authored town structures/NPC differentiation, directional animation, richer paperdoll and item comparisons. The broader six open milestones remain in the master plan; server economy/drop configuration is still the next backend slice. Reopen **Play Echoes of the Rift.exe** to load the rebuilt game.

## Historical build — revision 27, 27 September 2026

Implemented localhost Swagger administration at `http://localhost:8083/allah/api/hehe/v69/docs/`, with username/password authorization backed by an ignored salted scrypt hash. The parent-folder **Open Rift Admin.cmd** launcher starts/reopens it. See [admin guide](Server/ADMIN.md) for query/edit workflow. Player/account/catalog queries, give/set gold and gems, set level, inventory stack creation/replacement/removal, equipped-copy protection, atomic audit history, idempotent request IDs and revision conflicts are implemented. Existing game/account URLs are unchanged; admin access remains loopback-only.

Gems, character Level and AdminRevision persist with legacy-save defaults. The town displays level/gems; large stack counts use compact labels. Catalogue exports include names. Reconnect refreshes admin edits; character level currently does not affect combat stats/campaign unlocks. Uncapped upgrades, DB-configurable economy/drop rules, achievements/inbox and live profile push remain open. A reconnect teardown guard prevents the expedition ally HUD from querying an uninitialized network object.

Validation: all three Node suites pass (persistence, accounts, admin), including wrong-password/Host/Origin rejection, old-route removal, duplicate grant prevention, revision conflicts and durable audit/profile reopen. Swagger renders in the browser at the requested localhost URL. Desktop UI passed 305 checks, zero layout failures/errors, before the teardown-only fix. Final Windows export: `Logs/step-10-20260927-153710.log`; dedicated registration/resume/reconnect regression PASS for both runs, including authentication, server-only grants, four campaign stages, central rewards, pause and rankings; no runtime errors. Production player rewards were not changed during tests; a consistent database backup was created first.

Review/next: finish milestone 4 with database balance/drop-rule versions and gem progression, then safe uncapped upgrades and achievement/inbox recovery. Six broad workstreams remain open; this admin foundation does not complete the entire economy milestone. Android and public-deployment acceptance remain pending.

## 1. Combat HUD and navigation — locally accepted; device acceptance pending

Boss name/health, two-player ally portrait/health/down-state and an aspect-correct dungeon room map with player/ally/boss markers. Keep the central battlefield clear; hide expedition panels in town and suspend them with inventory. Existing overhead bars and touch sticks remain. Acceptance: local and server-backed damage/clear states, no solo phantom ally, marker bounds and four-resolution visual checks. The map represents the current rectangular room, not a connected multi-room maze.

## 2. Art, animation and loadouts — open

Preserve approved detailed artwork and 5% pixel treatment. Finish directional hero walk/attack/dodge/hit/death animation, distinct bosses/NPCs/environment and skill effects. Review readability at native size and crowded combat. Migrate three equipment slots to the requested six and six skills/two stances to the requested four-slot/runic system with saved-progress compatibility; improve paperdoll, comparisons and locked/equipped cues. Do not silently change the current loadout contract.

## 3. Town, inspection and multiplayer ownership — open

Show verified public player names and inspectable level/gear/books/stats without exposing private account/inventory data. Separate town and party instances and define capacity/routing. Audit every modal and pause/reconnect transition. Add safe sell rules and clearly identified unavailable services. Current town/two-player foundation is already implemented.

## 4. Authoritative economy and recovery — open; profile migration in place

Version DB-controlled prices, stats, drop weights, gold/gem rewards and achievements. Gear uses duplicates plus gold; books use duplicates plus gems. The server/client profile migration and progression simulations are implemented; the uncapped separate-stack enhancement transaction, DB reward tuning, pending-reward inbox and rollback remain open. Current five-tier gold fusion is a prototype and remains until the new transaction is tested.

## 5. Campaign, modes and online capacity — open

Expand the four-stage slice into the requested chapter structure, ultimately 50 levels per milestone with varied bosses. Add DPS trial, world boss, survival and 4/8-player raids incrementally. Validate rewards, reconnects, encounter readability, balance and server capacity for every advertised mode. Keep boss-only campaign completion/no revive and the current co-op revive rules.

## 6. Device performance and public-release readiness — open

Install Android build support; establish actual phone and low-end PC baselines. Verify multitouch, focus/resume, safe areas and controller input. Record crowded-combat frame-time percentiles, allocations, memory and CPU/GPU cost. Test separate devices with latency/jitter/loss. Complete HTTPS accounts, encrypted game transport, mobile secure session storage, client/server packaging separation, backups/restore, monitoring and portable updates; prepare Oracle after compatible server build support. Run five first-time-player acceptance sessions covering launch to boss to equip/upgrade to reconnect. No public readiness claim until these gates pass.

## 7. Community and commercial integrations — open

Build the Markdown/build-sharing library with safe media processing (five images, maximum 1024px dimensions), ownership, moderation and upload limits. Implement voluntary cinema rewards with the server-enforced 50/day cap and verified receipts. Implement chapter gem tally/offer state and storefront sandbox receipt recovery before any real purchase release. These later services depend on milestones 4–6; the blank co-op revive method remains separate from real ads.

## Iteration procedure

For each slice: implement, run focused regressions, inspect real-size captures, record limitations, revise the master plan, then select the next slice. Physical-device and human playtest gates stay open until actually observed. Feature completion is not public-release certification.

## Current evidence

Windows build: Logs/step-10-20260927-124226.log. Final build UI: 304 touch checks and 303 desktop checks PASS at four resolutions, zero layout failures/runtime errors. Boss damage/clear, solo party hiding and minimap bounds assertions pass. Final top-edge boss/map capture reviewed. Dedicated pair HUD/revives/rewards and solo ten-run campaign/combat regressions passed before the final layout-only rebuild. Previous accepted baseline: revision 25, 303 touch UI checks, 302 desktop checks and solo combat/campaign regression. Current catalogue: 140 items / 34 books, shop additions with unchanged starter campaign reward pools.

Operation: [PLAYTEST.md](PLAYTEST.md). Full requirements and history: [MASTER-PLAN.md](MASTER-PLAN.md). [Server guide](Server/README.md), [deployment](Server/DEPLOYMENT.md).
