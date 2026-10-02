# Execution log

## Current OCI feasibility check - revision 53, 2 October 2026

Used OCI CLI 3.94.1 to inspect region subscriptions, compute instances, shapes, quotas, network and reserved IPs. Only the Singapore home region is subscribed (one AD); the existing VCN/subnet are available, with no VM or reserved public IP. Non-billable capacity reports returned A1 1/6 GB, E2.1.Micro and E4 Flex 2/12 GB as `OUT_OF_HOST_CAPACITY`; E5 Flex 2/12 GB is `AVAILABLE` with sufficient quota. E5's published compute-only estimate is about US$61.32 per 730-hour month; no paid VM was created. Always Free compute must stay in the home region.

The local Unity 6000.6.2f1 install has Windows Standalone support but no Linux Standalone module, and the repo has no Linux server output. The client remains loopback-only. The Unity CLI install was not run: automatic review rejected elevated execution of an uninspected remote PowerShell installer. No OCI VM, reserved IP, DNS or public service was provisioned.

## Current server feature - revision 52, 2 October 2026

Added authenticated `POST /event-shop-purchase`. Purchases validate the pinned event version and active schedule, token balance, per-player cap and shared stock; token debit, stock decrement, idempotency receipt and item inbox grant commit atomically in SQLite. Retries return the original purchase, while any failed grant rolls back the spend and stock change. Added tests for success, idempotency after version changes, stale versions, insufficient currency, player caps, shared stock depletion and rollback.

Validation: full `npm test` passes across persistence, account, admin, progression, event and inbox suites. No Unity code or build changed; client shop integration waits on the player's wireframe feedback. Oracle remains unconnected: checked-in endpoints are loopback, the latest deployment notes report no VM/public IP, and the live CLI inventory query did not return usable data in this turn.

## Current design pack - revision 51, 2 October 2026

Generated 30 individual, editable 1280×720 landscape SVG wireframes plus an HTML browse index and editing guide in `Design/Wireframes/`. The pack covers existing onboarding, town, inventory, shop, settings, events, combat, recovery and results screens, and labels unimplemented goals/profile/matchmaking pages as concepts. Added a small Python generator at `Tools/GenerateUiWireframes.py` so the vector layouts can be rebuilt after feedback.

Validation: all 30 SVG files parse as XML; index presence confirmed; repository diff whitespace check passed. No Unity/game code changed and no runtime build was run. The next design iteration depends on the player's edits/preferences.

## Current review - revision 50, 2 October 2026

Performed a strict source-level review of every current player-facing screen and interaction state, from startup/login through town, inventory/shop, skills, events/inbox, combat, pause, revive, results, save recovery, reconnect and leaderboard. Compared each available surface to the existing two-pass My Heroes UI audit, five sampled gameplay videos, wiki notes and user screenshots. Findings and a prioritized redesign/acceptance sequence are in `Docs/UI-STRICT-REVIEW.md`; `MASTER-PLAN.md` now treats navigation hierarchy as the next UI slice.

No runtime UI code was changed and no visual parity claim was made. Main issues: competing town navigation, two-step entry before auth, no dedicated skill assignment screen, only three current gear slots, crowded combat edges, event rows without visible overflow navigation, duplicated close controls and result explanations detached from their actions. The account sign-out safety flow is retained as a correct existing decision.

Validation: source inventory and screen/action tracing only; `git diff --check` passes. The companion Canvas summary is saved in the Codex canvas directory. No fresh Unity export or Android device capture was run; existing layout checks/captures are prior revision evidence, not acceptance of this review's proposals.

## Current build - revision 49, 2 October 2026

Cached unchanged skill HUD text, bars, weapon art and skill art so the low-end client only refreshes cooldown visuals at the short cadence. Added a one-second active-event manifest cache with immediate invalidation on publish; schedule boundaries still refresh after the cache window and claims continue to read authoritative event definitions.

Validation: full `npm test` passes, including event schedule and claim tests. Unity source edits remain pending the local Editor license for a new export.

## Previous build - revision 48, 1 October 2026

Added SQLite indexes for session/ticket expiry and player cleanup, inbox ordering, event progress/claims and admin audit history. Enabled a shared five-second SQLite busy timeout. Reduced overhead HP-bar work by caching sprite-bound scans and bounded-rate value/color updates while leaving positions smooth every frame.

Validation: the full `npm test` suite passes, including schema-index assertions; the Unity source change passes the source-level brace check. A fresh Unity compile remains subject to the existing local Editor license gate.

## Previous build - revision 47, 1 October 2026

Audited the clone handoff and confirmed Unity source/settings, server source/tests, deployment scripts, tracked Windows payload and portable launchers are inside the repository. Added repo-local install/start/play command files, an endpoint runbook, and machine-independent OCI CLI/config path resolution. Documented that no public IP exists because OCI has not created a VM.

Validation: repository status is clean after the prior push; the endpoint defaults remain loopback-only, and no credential, private key, SQLite database or dependency directory was added.

## Previous build - revision 46, 1 October 2026

Lowered the minimum player account password from 15 to 5 characters while keeping the 128-character maximum. Updated server registration/recovery validation, the Unity account form label and the playtest instructions. Added boundary coverage for rejecting four characters and accepting five.

Validation: `node accounts.test.mjs` passes, including the new five-character boundary checks. Existing longer passwords and account flows remain covered.

## Previous build - revision 45, 1 October 2026

Moved the desktop handoff into the repository. Added a tracked `Release/Windows` payload without PDB debug symbols, rebuilt portable player/server launchers that locate the project by `Tools/`, moved the admin shortcut into the project root, and made all Windows test/play scripts prefer the tracked release while retaining an editor-build fallback. The clone now includes the playable client and server source; `Server/node_modules`, SQLite progress data, logs, admin hashes and credentials remain ignored machine state.

Validation: release payload contains 167 files and no PDB files; launcher sources compile with the installed .NET Framework compiler; server `npm test` remains green. The old parent-folder launchers were moved into ignored `Release/Legacy` and the old hard-coded path is no longer used.

## Previous build - revision 44, 1 October 2026

Implemented the next live-ops slice. Event manifests can declare a progress metric, cap and eligible modes; SQLite stores progress by event/version/player and returns it through version-pinned `/event-status`. Verified campaign reward receipts advance matching active metrics once, while duplicate receipts do not. Milestone events expose the next threshold and reject `milestone:N` claims before the server confirms completion. Token-exchange templates now track a bounded event-token progress metric. The Events drawer renders metric, value, cap and next threshold without creating client-owned counters.

Validation: `npm test` passes all suites, including campaign-to-event progress integration, cap/mode handling and milestone claim gates. Changed C# files pass source brace checks. A fresh Unity compile remains blocked by the local Editor license; the existing accepted Windows export and UI evidence remain the current runtime baseline.

## Previous build - revision 43, 30 September 2026

OCI CLI authentication now succeeds with the locally generated API-signing pair. Created the `echoes-rift-vcn` VCN, internet gateway, public route table, `10.42.1.0/24` public subnet, empty-ingress security list and `echoes-rift-nsg`; NSG ingress is limited to the operator's current `/32` on SSH plus public TCP 80/443, with all other game/admin/persistence ports closed. Both `VM.Standard.A1.Flex` and `VM.Standard.E2.1.Micro` launches returned Oracle's out-of-host-capacity response in Singapore, so no VM or public IP was created. The local server launcher now passes `/health` checks on 8081/8082 and starts the dedicated Unity server on UDP 7770; the packaged Windows client is running against it. No secret/auth token was stored or committed. The Windows x64 versus Linux ARM64 build boundary, encrypted gameplay transport and measured-device gates remain open.

## Previous build - revision 39, 30 September 2026

Completed a low-end runtime allocation audit. Co-op actor and projectile snapshots now reuse exact-size buffers, per-player cooldown arrays are reused, replica and bolt removal collections are retained, network presentation caches its rigidbody, summoned runes use a non-allocating physics query, skill HUD cooldown/objective text avoids unchanged writes, and `UISafeArea` skips unchanged display transforms. Quality defaults now select Android/iPhone Low and standalone Medium; `CombatVisual` lowers the burst cap on the lowest tier while keeping neon effects active.

Validation: `npm test` passes all suites; desktop UI 342 and touch UI 343 checks remain green; source brace checks pass for the changed C# files. A fresh Unity compile is blocked by the local Editor license and is recorded at `Logs/step-10-20260930-204549.log`. Measured Android and low-end-PC frame-time/memory capture remains the next device gate. The co-op snapshot still uses `FindObjectsByType` for enemy and projectile discovery at 20 Hz; this is logged as the next profiled optimization rather than changed without spawn/despawn measurements.

## Previous build - revision 38, 29 September 2026

Added the version-pinned event status route. Event detail cards now request `/event-status` with the exact manifest version, and the server returns active/scheduled/ended state, eligibility, the reason and server-recorded claim receipts. The client renders those values as status evidence and keeps actual module gameplay progress and claims server-owned.

Validation: `npm test` passes with event-status store and HTTP coverage; existing desktop UI 342 and touch UI 343 checks remain green. A fresh Unity compile is blocked by the local Editor license and is recorded at `Logs/step-10-20260929-204557.log`.

## Previous build - revision 37, 29 September 2026

Added event detail and eligibility presentation. Event rows now open a server-described detail card with the type, live server-timed end countdown, explicit eligibility boundary and reusable progression guidance for login, shop, boss, raid, tower-defense and collaboration modules. The client does not invent progress or make claims; Back returns to the event list and Inbox remains the recovery path for delivered rewards. Layout smoke coverage now exercises the row-to-detail transition.

Validation: server `npm test` remains green; existing Windows export remains green at desktop UI 342 and touch UI 343 checks. A fresh Unity compile is still gated by the local Editor license; the previous attempt is recorded at `Logs/step-10-20260929-080944.log`.

## Previous build - revision 36, 29 September 2026

Added transactional event reward delivery. Event claims now enqueue validated bounded currency/item rewards in the same SQLite transaction; the persistence service exposes inbox listing and idempotent collection, and Unity adds a Reward Inbox panel that applies the returned authoritative profile. Added server tests for reward validation, duplicate event claims, currency/item collection and replay safety, plus town/layout smoke coverage for the new panel.

Validation: `npm test` passes central-store, account, admin, progression, event and inbox suites. The existing Windows export remains green at desktop UI 342 and touch UI 343 checks. A fresh Unity compile for this slice was attempted at `Logs/step-10-20260929-080944.log` and is blocked by the local Editor reporting no valid license (exit 198).

## Previous build - revision 35, 28 September 2026

Added the in-game event drawer. Town now exposes a safe Events entry point; the drawer consumes the server-authoritative `/events` feed, displays active event type/title/version/countdown data using server time, handles unavailable feeds without blocking town, and closes before pause. It is read-only and does not grant client-requested rewards. Added a town smoke assertion for the drawer and documented transactional inbox delivery as the next slice.

## Previous build - revision 34, 28 September 2026

Completed a runtime UI safety review. Removed the connection-loss "Sign in again" action so that failure recovery can only reconnect; account revocation remains behind Settings > Accounts and its explicit confirmation. Added a regression assertion for unsafe account actions outside that boundary and documented the fixed neon-motion, modal-input and mode-gating policy in `Docs/UI-SAFETY-AND-PRESENTATION-AUDIT.md`. Historical references to a reduced-motion toggle were clarified as an intentionally absent preference.

Validation: `npm test` passes all central-store, account, admin, progression and event suites. The existing Windows export passes desktop UI 342 and touch UI 343 checks with zero failures/runtime errors. A fresh Unity compile was attempted at `Logs/step-10-20260928-211556.log` but remains blocked by the local Editor reporting no valid Unity license (exit 198); this is an environment limitation, not a reported source error.

## Previous build - revision 33, 28 September 2026

Added the Rift Defense design and event template: an optional skill-book tower-defense mode with five-book decks, seeded summoning, merges, wave mutations, readable counters, separate TowerPower tuning and server-validated runs. It is planned for solo first, then daily mutators, co-op, draft and seasonal content; PvP remains later. See [RIFT-DICE-DEFENSE.md](Docs/RIFT-DICE-DEFENSE.md).

Added the award-informed future design backlog covering first-ten-minute onboarding, short expedition loops, readable boss patterns, build synergies, collection recognition, social play, live-ops safety, telemetry and device-quality gates. The master plan now prioritizes player experience and measurable acceptance over menu count; see [FUTURE-DESIGN-BACKLOG.md](Docs/FUTURE-DESIGN-BACKLOG.md).

Added the reusable live-event foundation: versioned UTC manifests and historical revisions, login/token/Boss Challenge/community/collaboration templates, SQLite event storage, idempotent claim receipts, server-authoritative `POST /events`, loopback Swagger list/publish/disable operations, a Unity feed adapter and presentation-only countdown formatting. The full reusable module list, Clash of Critters/Brave Frontier reference review, publish cadence, rollback procedure and production safeguards are in [EVENTS-OPERATIONS.md](Docs/EVENTS-OPERATIONS.md). The event endpoint now accepts an empty request body safely.

Validation: server `npm test` passes persistence, account, admin, progression and event suites; an empty-body HTTP request to `/events` also passed; desktop UI 342 and touch UI 343 checks passed. Client event transport awaits a fresh Unity compile because the local licensing client currently reports no valid Editor license. Reward grants remain intentionally gated behind transactional inbox/enhancement work.

Added the star-plus item language (`***..  +12`) with ASCII-safe pixel glyphs and centralized progression rules. Gear duplicate/gold costs, skill copy/gem costs, soft power caps, campaign targets and the first-five-chapter new-player carry are documented and CLI-validated. Existing tier saves remain backward-compatible; server-side `EnhancementLevel`/`SkillLevel` migration is still required for uncapped live upgrades.

The server now normalizes old profiles on read/save with `EnhancementLevel`, `SkillLevels`, `EquippedEnhancementLevels` and `ProgressionVersion`; the Unity profile schema reads those fields and falls back safely for legacy local saves. The separate-stack uncapped enhancement transaction remains intentionally open.

Validation: server `npm test` passed all persistence, account, admin and progression suites. Final Windows export `Logs/step-10-20260928-192803.log`; desktop UI 342 checks and touch UI 343 checks passed with zero runtime errors. Full movement/combat/save/reward/inventory/campaign regression passed; the legacy storage startup guard is now clean.

Implemented the reference-led collection redesign: separate equipment/hero and five-column backpack panels, real equipped gear and six current stance skills, visible category tabs and a sort-choice menu. Items open in a dimmed inspection popup with rarity ribbon, large icon, real stat comparisons, story and Equip/Upgrade actions. Back closes the popup first and retains the browsing page; background controls stay blocked. Starter skills have a read-only inspection view. The merchant now uses a separate wide three-card layout with real gold prices and owned counts; inspection precedes Buy.

Dark double-bevel panels, red close controls and tier-only border ornament match the documented reference hierarchy more closely. Existing approved item/hero art is retained. Shop browsing suspends the preview camera; full-screen dimming covers taller aspect ratios and safe-area offsets. No server schema or economy rules changed.

Validation: final Windows export `Logs/step-10-20260927-210718.log`; strict text encoding PASS, 283 authored files. Desktop UI PASS 342 checks and touch-layout PASS 343 checks at four sizes, zero failures/runtime errors. Includes starter-skill text, all 140 item descriptions/lore, category filtering, nested Back, keyboard focus, border effects and actual Buy/Equip/Upgrade against disposable saves. Final native backpack, item popup and merchant captures reviewed. Solo ten-run campaign/save-recovery regression and isolated dedicated account registration/resume regression passed with no runtime errors. Physical Android testing remains open. The 1024px-wide development capture still shows the existing world-camera pixel-reference warning; it is not a UI overflow failure and lower-resolution world rendering remains a device-acceptance item.

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

## Historical build — revision 26, 27 September 2026

Export: `Logs/step-10-20260927-124226.log`. Added a named boss health bar, server-backed ally portrait/health/down-state for current two-player co-op, and a current-room minimap with player/ally/boss markers. Expedition panels hide in town and suspend with inventory. Boss health disappears on clear. Map bounds use real room geometry; maze/fog, public player names and larger-party support remain open.

Gameplay validation: dedicated Leader/Peer each PASS boss/party HUD visibility, boss-clear hiding, self-revive, teammate rescue and saved rewards. Solo combat/presentation and ten-run campaign/equipment/save/pause/defeat regression PASS. These checks preceded the final boss-bar position/legend-colour-only rebuild. Final build UI: 304 touch checks and 303 desktop checks PASS at four resolutions, zero layout failures/runtime errors. Boss damage/clear, solo party hiding and minimap bounds assertions pass. Final top-edge boss/map capture reviewed. No reported runtime errors. Physical Android/network/performance release gates remain open.

Roadmap consolidated into seven major milestones; this HUD foundation completes the first feature slice locally, with six broader milestones still open. See [current roadmap](MVP-ROADMAP.md) and [master plan](MASTER-PLAN.md) for precise scope, tests and next steps. Normal services were not stopped. Reopen the game to load the updated client.

## Historical build — revision 25, 27 September 2026

Export: `Logs/step-10-20260927-102610.log`. Touch movement/aim use fixed backgrounds and separate drag thumbs, bounded travel and independent finger ownership. Release/reset handles finger-up, cancellation, lost focus, application pause, HUD disable and disabled controls. Existing mouse/keyboard controls and server authority remain.

Validation: touch UI 303 checks PASS, desktop UI 302 checks PASS, zero layout failures/runtime errors across four resolutions. Simulated pointer tests cover simultaneous sticks, fixed backgrounds, ownership, independent release, focus loss, inventory reopening and cancellation. Windows touch screenshot reviewed. Solo combat/presentation fixtures and ten-run campaign/equipment/save/pause/defeat regression PASS with no reported runtime errors. Android module/physical device QA and FPS profiling remain pending. Dedicated pair/backend tests last passed revision 24/revision 23 respectively; neither backend nor protocol changed here.

QA: `Tools/Test-UI.ps1 -Touch` writes to `Logs/UILayoutTouch`; normal UI tests retain `Logs/UILayout`. `-touchControls` enables touch layout in a Windows player for inspection. See [master plan](MASTER-PLAN.md) revision 25 for next steps. Normal server was not stopped; reopen the game using **Play Echoes of the Rift.exe** for the updated client.

## Historical build — revision 24, 26 September 2026

Export: `Logs/step-10-20260926-185717.log`. Five My Heroes gameplay videos sampled; see [reference review](Docs/MYHEROES-GAMEPLAY-REFERENCES.md). HUD now uses compact portrait/vitals, slim objectives, a bag icon and three circular skills around a larger weapon attack control, with radial cooldowns. Client characters/projectiles interpolate between server snapshots; camera follow eases while retaining pixel snapping and immediate teleport/room changes. Remote appearance rebuilds are skipped when unchanged; enemy IDs remain stable during a stage. Approved art, 140-item catalogue and gameplay authority remain.

PASS: 302 UI checks across four resolutions, movement presentation fixture, combat/ten-run solo regression and final-build dedicated pair self-revive/teammate rescue/rewards. No reported runtime errors. Solo gameplay tests preceded the final text-height-only rebuild. Backend account/store Node suites last ran in revision 23. Android/device FPS and internet-jitter acceptance remain pending; no measured FPS claim. See [master plan](MASTER-PLAN.md) revision 24 for ordered follow-up.

Restart **Echoes of the Rift Server.exe** and **Play Echoes of the Rift.exe** to load the updated build. Normal services were not stopped automatically.

## Historical build — revision 23, 25 September 2026

Export: `Logs/step-10-20260925-075818.log`. Added 15 weapon entries (spear, boomerang, pistol across five tiers) and four skillbooks: Tempest Volley, Astral Lance, Crimson Cyclone and Solar Hammers. Catalogue now 140 items / 34 books. Hephaestus sells spears, Artemis sells boomerangs/pistols, Athena sells the books. Books currently equip to Q. New items are shop content; campaign drops remain the existing rewards.

Weapons now have directional melee, spear thrusts, ranged/spread/piercing fire and returning throws. Added new illustrated gear/book/skill art using the approved 5% pixel treatment. PC: aim with mouse, hold left click; Q/E/R skills, Tab stance, Space dodge. Mobile aim-stick fires while held (real-device testing pending).

PASS: 298 UI checks, focused new-combat fixtures, solo ten-run campaign/equipment/save regression, dedicated pair self/teammate revives and rewards, store/account Node tests. Restart both **Echoes of the Rift Server.exe** and **Play Echoes of the Rift.exe** so the new catalogue and combat messages match. Existing normal server was not stopped automatically. See MASTER-PLAN.md revision 23 for remaining balancing, art, network and device validation.


## Historical build — revision 22, 25 September 2026

Export: `Logs/step-10-20260925-071121.log`. Approved detailed hero/equipment sheets are integrated, with subtle 5% coarser point sampling in-game; original images remain intact. Nine racial heroes, supported gear families and skill/book emblems now use the new shared art. Existing stats, saved items and pooled effects remain.

260 UI checks and the solo campaign/equipment/save regression passed. Inventory capture reviewed. Remaining gear families, extra hairstyles, boss/environment art and directional animation are not yet redrawn. Creator offers hair palettes and RGB while saved hairstyle choices remain for later authored variants. See MASTER-PLAN.md revision 22 and Assets/_Project/Resources/Illustrated/ART-DIRECTION.md for exact scope and prompts.


## Historical build — revision 21, 24 September 2026

Export: `Logs/step-10-20260924-215701.log`. Reduced motion removed. Trails and hit bursts now reuse capped pools; rarity/material/component lookups are cached, off-screen trails skipped, headless-server visual allocation skipped, and portrait/UI refresh rates reduced without changing gameplay simulation. Neon effects remain active. Existing desktop launcher opens this build.

Validation: 260 UI checks passed, including 12,000 requested trail emissions with a 128-object desktop cap and 0 managed bytes in the warmed 10,000-request loop. Solo campaign/equipment/save regression passed. Both dedicated co-op clients passed self-revive, teammate rescue and reward saving. These are local Windows tests, not an FPS benchmark or Android certification. Android build support/device testing remain outstanding. See MASTER-PLAN.md revision 21 for performance targets and remaining profiling work.


## 24 September 2026 — revision 20, curved dark interface

Replaced patterned frames with dark violet rounded pixel bevels; added separate rarity outlines and bounded perimeter particles/halos on inventory/shop cells and selected item. Icons remain static. Added circular skill controls, inset description and rarity-coloured names. World weapon colour cycling/idle rotation removed; trails scale from tier 3 through 5 and respect reduced motion.

Validation: final export passed 259 UI checks with zero failures/runtime errors, including border-only motion, static icon core, click-through sparks and reduced-motion behavior. Desktop inventory captures at four resolutions and simulated notch were generated; 1280x720 visually reviewed. The first export of this pass also passed ten campaign runs, loot/save recovery, inventory/equipment/fusion, pause/defeat and no-campaign-revive checks. Final refinements only changed item presentation and UI checks. Dedicated account/co-op were not rerun in revision 20; their last full pass is revision 19.

Final export: Logs/step-10-20260924-215029.log. Master plan revised with the user's latest reference direction and explicit remaining asset/character/shop polish. Archived revision 19 in Backups/master-plan-v19.md. Normal server was not stopped or reconfigured.


## 23 September 2026 - separate server operation and startup readiness
- User selected local PC hosting now, with Oracle deployment preparation next. Split desktop client launch from server ownership: Play Lantern Raid starts only the player; Lantern Raid Server starts/stops its owned backend processes through a control window.
- Added account-side health/readiness backed by a private server heartbeat and database query. Readiness expires after 10 seconds without the game process. Splash startup checks block entry behind Connection error / Please try again later; Retry never launches services.
- Export passed: Logs/step-10-20260923-220626.log. Unavailable startup PASS aboveSplash=True blocksEntry=True retry=True runtimeErrors=False. Account registration/restart/reconnect and four-stage central rewards passed with the server running. Visually inspected Logs/Launcher/server-unavailable.png.
- Desktop server controller acceptance passed ready/stop/cleanup. Installed separate desktop executables. Account backend suite also tests readiness status 503/200.
- First build approval timed out before execution; authorized retry succeeded. Corrected a missed test-dispatch edit before the final unavailable-server test; no assertion was weakened.
- Added Server/DEPLOYMENT.md and Caddyfile.example. No cloud resources, DNS, certificates, firewall or port-forwarding changes made. Public transport security, Linux/Arm validation, client release packaging and capacity work remain explicit gates.


## 23 September 2026 - Step 2 local onboarding and accounts
- Added pixel splash/enter screen with persistent reduced-motion option, registration/sign-in, account recovery and sign-out. Authenticated returning players enter with saved appearance; new players proceed to character creation. Visually inspected final registration/account-ready screenshots; fixed orphaned labels after fields were hidden.
- Added loopback-only account service on port 8082, salted scrypt password hashes, case-insensitive usernames, rotating 30-day sessions, two-minute single-use game tickets, recovery-code rotation, HTTP request/concurrency limits and transactional legacy-profile linking. Windows saves only a DPAPI-protected refresh token; no password persistence or password game RPCs.
- Normal game-server authentication consumes a ticket. Legacy device-secret login is restricted to explicitly flagged development test servers. Remote account access is disabled pending HTTPS/encrypted game transport. Existing local/offline files are preserved.
- Backend account tests PASS: registration/uniqueness, wrong password, ticket replay/expiry, refresh rotation/expiry, recovery/revocation, saved legacy loot, ownership rejection, concurrent registration and HTTP rate limit. Corrected an invalid-format receipt in the initial test fixture; production receipt validation was retained.
- Final Windows export PASS: Logs/step-10-20260923-214215.log. Unity registration and process-restart auto-login both PASS device protection, gameplay authentication, four campaign stages and central saved rewards; runtimeErrors=False. Both also deliberately disconnected and passed renewal/reconnection (ACCOUNT_RECONNECT_OK). Logs/Dedicated/Register and Logs/Dedicated/Resume contain the evidence.
- Co-op pair tests passed placeholder self-revive, teammate rescue and saved rewards on both players; solo ten-run campaign/inventory regression passed during this increment.
- Hidden native account captures were black, so those screen reviews used the established camera-render capture workaround; do not count these as native mobile validation.
- Master plan revision 14 and play/server guides updated. Public TLS/transport, mobile secure storage, immediate revocation of active gameplay sessions, distributed abuse controls and actual mobile/gamepad checks remain unfinished. No public account service was deployed.


## 23 September 2026 - text consistency and native layout checks
- Centralized 57 fixed screen labels/messages plus common HUD, town, rankings and inventory formatting. Added UISafeArea and menu focus feedback.
- Added exported-player layout audit across four native window sizes, simulated asymmetric notch, all 121 item comparison/lore descriptions, town and HUD. Keyboard focus exercised. Initial audit found two repeated root causes: equipped-badge and lore-button text rectangles too short; enlarged both and reran without relaxing assertions.
- Final export: Logs/step-10-20260923-190451.log. UI audit PASS checks=253 failures=0 runtimeErrors=False. Evidence: Logs/UILayout/layout-checks.txt and native PNG captures. Previous export in this increment passed solo and dedicated campaign regressions.
- Actual Android/iOS devices, physical gamepad/touch and residual combat formatting remain open. Step 1 is not misrepresented as fully mobile-certified; splash/registration has not been implemented in this increment.


## 23 September 2026 - UI-first item presentation increment
- Consulted the master plan before implementation and reviewed it after validation. Step 1 remains in progress; later account/economy work was not silently advanced.
- Added EnglishUI inventory strings and formatting, selected-item art/border, equipped-gear stat comparisons, clear upgrade requirements, weapon cosmetic milestone hints and separate lore view. Disabled unavailable page/upgrade controls.
- Shared Canvas uses pixelPerfect and Expand scaling; TMP rich-text interpretation and automatic text shrinking are disabled. Actual mobile/safe-area/native scaling checks remain outstanding.
- Export passed: Logs/step-10-20260923-163810.log. Solo inventory/combat/reward suite and dedicated-server four-stage campaign/save suite passed, runtimeErrors=False. Reviewed Logs/Runtime/inventory.png for layout and readability.
- Master plan revision 12 records evidence-informed motivation principles, earned item/campaign cosmetics and fair competitive recognition. These recognition systems are planned, not implemented.


## 23 September 2026 - expanded roadmap review
- Reviewed master plan, current town/community helpers, fusion, device identity and persistence implementation against the new feedback. Archived revision 10 and wrote revision 11 with the requested eight-step order, explicit status and acceptance gates.
- Recorded gold/gem precedence, uncapped upgrade levels separate from rarity, target-level duplicate formula, secure session persistence, chapter tally assumptions, server configuration/reward ledger, operator compensation, media constraints and monetization dependencies.
- This increment changes planning documentation only. No new account, town, art, gem, community or payment feature is claimed implemented; existing executable is unchanged.


## 23 September 2026 - one-click play and inventory browsing
- Added Play Lantern Raid.exe beside the project and on the user's desktop. It starts private persistence, waits for the headless server, opens the game, and cleans up its own processes after the game exits. Duplicate launch protection and readable startup failures are included. Normal saves retain Server/progress.sqlite; acceptance tests use isolated databases.
- Windows rejected the WScript shortcut target. Replaced the unusable shortcut with the actual executable; its installed-path fallback points to this workspace. Rebuild source and command are in Tools/Launcher.cs and Tools/Build-Launcher.ps1.
- Added inventory and shop category filters, sorting by name/tier/quantity, page totals, and empty-category feedback. Inspected the exported inventory screenshot for clipping and readability.
- Windows export passed: Logs/step-10-20260923-131702.log. Solo regression including inventory filtering passed with runtimeErrors=False. Full launcher acceptance passed through authentication, four campaign stages, saved central rewards, server pause and rankings. Verified local TCP 8081 and UDP 7770 were released after exit.
- Review: current playable scope is four solo boss stages, two-player co-op revive/loot, modular heroes, pixel HUD, 121 catalogue entries, inventory/shop/fusion and local dedicated progression. This is a playable local prototype, not a public-service release.
- Next priorities: distinct bosses and animation; human combat/economy balance; real-device network recovery and deployment hardening. Additional spell-slot selection, stat comparisons, purchase confirmations and narrative quests follow. Desktop launcher depends on this project staying at its current location and the installed Node runtime; a portable installer remains future packaging work.

## 23 September 2026 - version 0.2 visual and server overhaul
- Completed master plan revision 9's seven implementation steps: removed cookie utility/public hosting controls; pixel font/frames/HUD/objectives; nine modular races with RGB controls; 121 icon-bearing items including 30 skill books; tier animation/trails; paged inventory/shop/inspection; dedicated game server with private SQLite persistence and reconnect UI.
- Original deterministic 32px art and licensed Tiny5 font are bundled. Race sheets contain layer/style variants; full hand-authored walk/attack atlases and bespoke boss art remain future work. No reference-game assets were copied.
- Fixed issues found during iteration: early authentication hookup, stale stage-result buttons, hidden-window character-preview rendering, equipped badges on the wrong tier, campaign pause not reaching the server, disconnect retry cleanup, and summoned effects surviving room changes.
- Final Windows 0.2 export PASSED: Logs/step-10-20260923-110331.log. Catalogue export: Logs/step-10-20260923-110147.log.
- Persistence unit tests PASSED: credentials, atomic fusion, duplicate reward receipts and invalid transactions (Server/test.mjs).
- Final solo suite PASSED: movement/dodge/immunity/cooldowns, ten campaign loops, exact saved rewards, failed-save recovery, boss-only completion, boss-required completion, inventory UI, catalogue metadata, nine races/RGB, tier effects, no campaign revival, pause/resume and defeat. Logs/Runtime/runtime-smoke.txt, runtimeErrors=False.
- Legacy two-process regression PASSED on host and client: authoritative input, wipe broadcast, remote visuals, revive rules and saved loot. Logs/CoopHost/runtime-smoke.txt and Logs/CoopClient/runtime-smoke.txt, runtimeErrors=False.
- Final dedicated campaign suite PASSED: authenticated client, rejected client loot grants, all four stages, central rewards, server pause and real rankings. Logs/Dedicated/runtime-smoke.txt, runtimeErrors=False.
- Final dedicated two-client suite PASSED on both clients: one placeholder self-revive, one teammate rescue and saved shared loot. Logs/DedicatedPair/Leader/runtime-smoke.txt and Logs/DedicatedPair/Peer/runtime-smoke.txt, runtimeErrors=False.
- Missing-server test PASSED: connection-loss modal and actual reconnect button. Logs/ConnectionFailure/runtime-smoke.txt, runtimeErrors=False.
- Inspected final inventory, rankings and reconnect captures. Live hero previews and stack quantities are visible. Offscreen screenshots do not establish native-window pixel scaling.
- No public server was deployed. One expedition/party per server process, encrypted transport/account recovery, real-device reconnect/latency testing, human balance, broader quests and bespoke animation remain in the next iteration. PLAYTEST.md and Server/README.md explain local play and operational limits.

## 21 September 2026
- Original Step 1: PASSED. Logs/step-01-20260921-202455.log, Unity exit 0, 16 generated/imported sprites.
- Master plan: revision 3 written before further implementation. See MASTER-PLAN.md for scope, sequence and release gates.
- Unity Codex plugin: unity@unity-agent-plugin 0.1.6-beta installed and verified enabled. Official source: https://github.com/Unity-Technologies/unity-agent-plugin . Skills can be read in this session; slash-menu discovery may require a new session per Unity documentation.
- Foundation hardening: Fish-Net pinned to the resolved commit; independent compile entry point and Windows export diagnostic added. Verification pending.
- Foundation verification: PASSED Windows smoke export, Logs/step-01-20260921-203343.log. Initial native-extension messages were not an export blocker. Git repository initialized.
- Step 2: PASSED compile and appearance/identity/aura checks, Logs/step-02-20260921-203723.log. Character prefab, palette material/shader, and preview scene generated. Visual runtime review pending.
- Step 3: PASSED compile and direct timer pause/cancel/completion checks. The timer intentionally covers app-running unscaled time; offline baking is not implemented.
- Step 4: PASSED formula, clamping and finite-division checks, Logs/step-04-20260921-204000.log.
- Step 5: PASSED asset generation and attachment-cap checks including deserialization, Logs/step-05-20260921-204135.log.
- Step 6: PASSED compilation and arena wiring, Logs/step-06-20260921-204534.log. Export/runtime movement/dodge/cooldown/screenshot checks in progress. Not yet a validated playable release.
- Step 6 runtime iteration: PASSED after asynchronous TMP import and runtime shader preservation. Logs/Runtime/runtime-smoke.txt records movement, dodge, invulnerability expiration, cooldown and stance checks. Offscreen screenshots inspected (capture temporarily disables pixel-perfect upscale and renders UI through the camera).
- Step 7: PASSED, Logs/step-07-20260922-214023.log. One preceding Unity startup exit was retried; it was not a script failure.
- Step 8: PASSED compile/Fish-Net code generation and raid model checks. Network transport/gameplay integration still pending.
- Step 9: PASSED, Logs/step-09-20260922-214424.log. Town UI/pause/local wiki helpers implemented. No live forum or advertising service.
- Step 10: PASSED, Logs/step-10-20260922-214956.log. Four-room JSON layout, tile/enemy pooling, solo hub/reward integration and platform build entry points implemented.
- Whole-game review recorded in MASTER-PLAN.md revision 5. Iteration 2 begins with end-to-end runtime checks and networking integration.

## 23 September 2026 — iteration 2 reliability increment
- Co-op integration now exports: Fish-Net scene registration, host/join, two player actors, authoritative input/combat, health/cooldown snapshots and party wipe. Initial scene-ID and early-client-state failures were corrected; client readiness explicitly follows networking lifecycle.
- Added host-originated attack-ring/impact broadcasts and visual-only projectile snapshots on clients. Damage remains authoritative. Unity 6.6 rejected GetInstanceID, so projectiles use explicit monotonic visual IDs instead.
- Local room reward failures now block progression behind Retry reward save. Persist-before-state-change plus a stage reward guard prevents repeated retry callbacks duplicating the reward. Remote reward retry and crash-durable pending rewards remain unfinished.
- Forced-save-failure test passed, including unchanged coins before recovery, the real retry button, repeated callback and persisted coin count.
- A solo movement test failed because hidden-window focus reset synthetic keyboard state. Diagnostic output confirmed released input; test-only focus isolation and held-key events fixed it. No assertion was removed or relaxed.
- Final Windows build PASSED: `Logs/step-10-20260923-054815.log`.
- Final solo runtime PASSED: movement, dodge, immunity expiration, cooldown, stance switch, ten complete scripted runs, reward totals/save reload, failed-save recovery, pause/timer, resume and defeat. `Logs/Runtime/runtime-smoke.txt`, runtimeErrors=False.
- Final two-process co-op PASSED on host and client: connection, authoritative remote input, wipe broadcast and client effect/bolt receipt. `Logs/CoopHost/runtime-smoke.txt` and `Logs/CoopClient/runtime-smoke.txt`, runtimeErrors=False. This is one computer using loopback, not remote-device/latency validation.
- Inspected co-op screenshot: characters and HUD visible; checkerboard environment/static sprites still prototype quality. Offscreen captures use the existing camera workaround and are not proof of native-window pixel scaling.
- Added PLAYTEST.md. Master plan revision 6 records completed work, remaining risks and ordered next gates: revive/reconnect/full co-op win, durable shared rewards, network fairness, presentation, access and release validation.


## 23 September 2026 - campaign, loot and revive acceptance
- Four configured campaign stages now each spawn a boss plus optional smaller enemies. Boss death alone completes a stage; surviving enemies stop simulation while rewards are shown. Boss health scales from 100 to 265. Art and attack pattern assets are still reused.
- Stage JSON specifies reward item and coins. Reward UI names the item and amount. Campaign completion count saves in the same profile transaction as loot and appears in town. Existing fusion/equipment and failed-save retry remain available.
- Campaign death has no revival. The shared public revive action rejects campaign use.
- Added co-op Revive button and deliberately empty ShowReviveAdPlaceholder method. Its callback requests a server-owned self-revive only for the requesting downed player while the party is alive and the stage remains active. No advertisement SDK or external navigation is called.
- Teammate revive remains a three-second nearby action. Successful revives grant two seconds of damage protection, with dodge movement now tracked separately. Full wipes reject placeholder requests and cancel in-progress rescues.
- Final Windows export PASSED: Logs/step-10-20260923-080755.log.
- Final solo suite PASSED: ten boss-only campaign runs with living smaller enemies, all-small-enemies-only cannot clear, exact item/coin totals, saved campaign completion, save failure/retry, campaign revive rejection, movement/dodge/cooldowns, pause/timer and defeat. Logs/Runtime/runtime-smoke.txt, runtimeErrors=False.
- Final two-process co-op suite PASSED on both players: actual client Revive button, host-observed self-revive, teammate rescue, distance cancellation, wipe during rescue, late placeholder rejection and saved stage loot on both players. REVIVE_CHECK reports placeholder=True distanceCancel=True teammate=True wipeCancels=True in Logs/CoopHost/player.log. Both runtime-smoke.txt files PASS with runtimeErrors=False.
- Inspected the downed-player UI screenshot; corrected mixed encoding in two UI scripts and rebuilt/retested. Logs/CoopClient/revive.png records the final overlay.
- Master plan revision 7 reflects the changed campaign rules. Remaining improvements: human difficulty testing, unique boss presentation/patterns, full co-op campaign completion/reconnect and two-machine latency tests, durable retries for failed remote loot persistence.


## 23 September 2026 — Revision 16, player-feedback foundation
- Integrated the supplied Echoes of the Rift directive and all eleven UI references into the active five-step overhaul sequence; preserved earlier economy/server/community requirements.
- Rebranded runtime UI, Unity product/window, code namespaces, shader names, export paths and both launchers. Installed new desktop launchers. Installed repository directory and compatibility protocol/environment/test identifiers remain stable; no destructive directory relocation.
- Added one-time allowlisted Windows storage migration for the prior CookieRaid product path. Editor validation proves no overwrite, protected-token copy, test-token exclusion and no token resurrection after sign-out.
- Configured Built-in native-reference pixel presentation, reduced only hero visual layers to 32px at 720p, preserved actor collider/movement and separate portrait scale. Thin slate frame borders and green HP fill.
- Inventory now suspends town/HUD/status while active and restores them on close; Escape closes inventory before pause. The full single-location state machine remains future work.
- Build: Logs/step-10-20260923-224909.log. Final UI: 255 checks, zero failures. Dedicated Register/Resume: authenticated reconnect and four-stage central rewards passed. DedicatedPair Leader/Peer: one self-revive and one teammate revive each, reward true, no runtime errors. Earlier same-increment solo campaign/loot acceptance passed.
- Visually reviewed native inventory screenshot and combat capture. Finding: original procedural icons and old enemy/environment art still need the higher-detail pass; current inventory and skills layout have not yet been replaced with the reference architecture. Added these limitations and next actions to the master plan.
- Final renamed server-controller startup/shutdown check passed. Final client-only startup rejection/retry passed with the server stopped; test server ports were released.


## 23 September 2026 — Revision 17 town implementation, acceptance interrupted
- Replaced menu town with Rift Haven tilemap, five named NPCs and specialist catalogue filters, proximity/tap/F interaction, four gate destinations, gold display and a 64px position map.
- Added server town input/snapshots while no expedition runs, actor safe-zone state, damage/action rejection and movement bounds. Expedition entry clears safe-zone state; town return clears dungeon actors/effects. Combat HUD controls hide in town. Larger raids, DPS trial and world boss remain unavailable; north separately offers the existing two-player test.
- Added TestTown and development-only TestTownSafetyServerRpc for movement, protection, book filtering and gate checks; updated expedition test waits to distinguish lobby snapshots.
- Build succeeded: Logs/step-10-20260923-230657.log. The subsequent UI audit FAILED: 258 checks, 38 findings, no runtime errors. NPC/gate text boxes overflow at all four tested sizes; east/west gate labels leave safe bounds at 1024x768. Screenshot review also found north-gate/header overlap.
- The sequential test command stopped on the UI failure. Solo/town, dedicated account and dedicated pair checks did not run for revision 17. Earlier passing logs dated 22:47–22:50 are revision 16 evidence only. The user interrupted development and requested documentation reconciliation; no fixes or new tests were performed during that documentation pass.

## 23 September 2026 — Revision 17.1 documentation reconciliation
- Updated MASTER-PLAN, MVP-ROADMAP, PLAYTEST, Server/README and Server/DEPLOYMENT to distinguish implemented, verified and pending work. Replaced the obsolete scaffold-only roadmap, preserving it in Backups/mvp-roadmap-20260921.md. Added a project README as the document entry point.
- Corrected renamed build-data path and stale claim that account recovery was unimplemented. Documented current NPC/gate controls, two-player/one-expedition limit, removed cookie feature, and retained public/mobile deployment gates.
- Next: fix town label height/safe-area/header overlap, rebuild and pass UI, then run town safety/movement, account/reward and co-op revive regression tests before further features.


## 23 September 2026 — Revision 18, town acceptance completed locally
- Increased sign height, kept world labels inside safe screen bounds, reserved north-label space below the header and hid Objectives in town. Fixed the objective drawer's per-frame override so it can remain collapsed during combat.
- First corrected UI build passed text bounds; native 1024x768 review still revealed overlap. Adjusted north sign width/header clearance and reviewed final 1024x768 and 1280x720 captures.
- First local gameplay run found inventoryUI=False: Athena merchant mode persisted after closing. Reset merchant/shop/filter/selection state on close; the full inventory regression now passes.
- Added a development-only two-client readiness barrier before co-op, preventing one test client from starting the raid while its partner is still checking town safety.
- Final export: Logs/step-10-20260923-234542.log. UI PASS 258 checks / zero failures. Local town movement/safety/books/gate checks and ten-run campaign/loot/save/inventory/pause/defeat regression PASS. Dedicated Register/Resume each PASS town movement/safety, reconnect and four-stage server rewards. DedicatedPair Leader/Peer each PASS town checks, one self-revive, one teammate rescue and saved reward. No reported runtime errors.
- Remaining: below-reference camera framing warning at 1024x768, actual mobile/touch and separate-device tests, production art, peer inspection/names and remaining master-plan features. Updated all current project guides to revision 18; historical failures remain in this log.


## 24 September 2026 — Revision 19 visual/gameplay presentation pass
- Incorporated six new My Heroes reference screenshots into the master plan. Redrew original layered hero proportions to a square large head, small tunic/boots, saturated cape/armour and walk bounce; preserved race/RGB choices and upgraded world presentation to 48px at 720p. Creator preview zoom reduced.
- Added bounded, grid-snapped follow camera; expanded town to 33x19 and campaign maps to 29x19 tiles; retained screen-fixed HUD and projected/hiding world labels. Minimap scale and server movement bounds updated.
- Added overhead green/red HP bars to players, all enemies/bosses, network replicas and protected town NPCs.
- Added four original boss silhouettes and distinct radius/windup/recovery combinations (Osiris, Ra, Sobek, Zeus) and deterministic matching client replicas. These are four pattern sets, not four fully unique AI systems.
- Added expanding colour bursts, impact sparks, bright arrow projectiles and afterimages with client snapshot support. Preserved authoritative damage, warning timing and cooldowns.
- Reworked inventory/shop into left-side hero/comparison/upgrade view and 5x4 right-side collection; preserved paging/filtering/owned counts and added merchant headings. No unimplemented equipment slots or fabricated DPS/set bonuses.
- Normal local server ports were occupied. Initial UI/local tests passed; dedicated test startup stopped safely. Added explicit test-only gameplay/account port overrides and test harness environment restoration; normal server and database were not stopped or changed by testing.
- Final build Logs/step-10-20260924-212504.log. PASS: UI 258/0, local ten-run campaign/loot/save/inventory/pause/no-revive, SCROLL_CHECK following=True, BOSS_VARIETY_CHECK types=4, town movement/safety, dedicated Register/Resume reconnect/rewards, and Leader/Peer self-revive=1 teammateRevive=1 reward=True. No reported runtime errors.
- Reviewed final inventory and combat captures. Remaining visual scope: direction-specific authored frames, individual spell choreography, richer item/boss detail, more boss mechanics and six-slot paperdoll migration. Restart the user's old server before playing the new maps.
