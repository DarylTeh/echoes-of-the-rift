# Echoes of the Rift

## Current build - revision 46, 1 October 2026

Player account passwords may now be 5–128 characters. Registration, sign-in and recovery use the same server rule, and the account form shows the five-character minimum.

The clone now includes the playable Windows handoff under [Release/Windows](Release/Windows) plus portable `Release/Play Echoes of the Rift.exe` and `Release/Echoes of the Rift Server.exe` launchers. Start by running `npm ci --prefix Server --ignore-scripts`, then open the server launcher and player launcher. The local admin shortcut is [Open Rift Admin.cmd](Open%20Rift%20Admin.cmd). Runtime databases, logs, `node_modules`, hashes and credentials remain machine-local.

The live-event slice now records server-owned progress per event version and player. Verified campaign reward receipts advance matching active event metrics exactly once, caps and eligible modes are enforced, and milestone claims remain locked until their thresholds are reached. The Events drawer renders the authoritative value, cap and next threshold beside the existing status and inbox flow.

This pass keeps the low-end runtime allocation reductions and records a verified local playable stack alongside the working OCI CLI authentication and provisioned network foundation. Co-op snapshots reuse buffers and cooldown arrays, network presentation caches components, summoned rune targeting is non-allocating, skill cooldown text is refreshed only when it changes, and safe-area layout recalculates only after a display change. Android/iPhone default to Low and standalone defaults to Medium; fixed neon effects remain enabled with a lower low-quality burst cap. Both Always Free VM shapes are currently out of host capacity in Singapore, and public deployment remains gated by Unity activation, a compatible Linux server build, encrypted gameplay transport and measured device/server tests. See [Server/OCI-DEPLOYMENT.md](Server/OCI-DEPLOYMENT.md).

The Events drawer now asks the server for version-pinned event status when a detail card opens. It shows active/scheduled/ended state, eligibility reason, server-recorded claim receipts and authoritative module progress for the exact manifest version. The client never fabricates counters. Layout smoke coverage exercises the detail transition. The next live-ops slice is module-specific reward delivery and event-shop spending.

The UI safety and presentation audit is now the guardrail for new screens and modes: sign out lives only in Settings > Accounts behind confirmation, connection loss offers Reconnect only, and the reduced-motion preference is absent so neon presentation remains active. See [the audit](Docs/UI-SAFETY-AND-PRESENTATION-AUDIT.md) and the master plan.

The town now has a read-only Events drawer backed by the server-timed `/events` feed. It shows active event types, titles, versions and countdowns without granting rewards or trusting the device clock. The recoverable Reward Inbox is wired beside it for delivered rewards.

The Reward Inbox is wired for recoverable server-issued currency and catalogue-item rewards. Claims are idempotent, bounded and profile-synchronised; the client cannot choose reward amounts.

Validation for this review: server `npm test` passes, including version-pinned event-status coverage; the existing Windows export passes desktop UI 342 and touch UI 343 checks with zero failures/runtime errors. Source-level brace checks pass for the optimized C# files. A fresh Unity compile for this slice is currently blocked by the local Editor license and is recorded in `Logs/step-10-20260930-204549.log`.

## Previous build - revision 35, 28 September 2026

Rift Defense is now planned as an optional skill-book tower mode: five-book decks, 5x7 placement, merges, wave mutations, boss counters and a retained hero emergency cast. The full design and server rules are in [RIFT-DICE-DEFENSE.md](Docs/RIFT-DICE-DEFENSE.md).

The future design direction is now prioritized in [FUTURE-DESIGN-BACKLOG.md](Docs/FUTURE-DESIGN-BACKLOG.md): first-ten-minute onboarding, short expedition sessions, readable boss patterns, build synergies, collection recognition, meaningful co-op, safe live operations and real-device quality gates. It is grounded in recent award-winning design patterns and official Apple/Android guidance.

Live-event foundations now use versioned UTC manifests, reusable server templates, idempotent claim receipts, transactional inbox delivery, a server-time feed at `POST /events` and version-pinned player status at `POST /event-status`; see [the event operations guide](Docs/EVENTS-OPERATIONS.md). Unity includes the feed adapter, read-only Events drawer, authoritative status details and recoverable Reward Inbox. The next live-ops slice is module-owned gameplay progress and eligibility.

Progression presentation now separates fixed rarity stars from enhancement: `***..  +12` using font-safe pixel glyphs. `ProgressionRules` documents and validates bounded gear/skill power curves, duplicate/gold/gem costs, campaign targets and first-five-chapter carry. Server and client profiles now carry separate enhancement/skill fields with legacy fallback; the separate-stack uncapped transaction is the remaining economy step. See [the progression model](Docs/PROGRESSION-MODEL.md).

Implemented the reference-led collection redesign: separate equipment/hero and five-column backpack panels, real equipped gear and six current stance skills, visible category tabs and a sort-choice menu. Items open in a dimmed inspection popup with rarity ribbon, large icon, real stat comparisons, story and Equip/Upgrade actions. Back closes the popup first and retains the browsing page; background controls stay blocked. Starter skills have a read-only inspection view. The merchant now uses a separate wide three-card layout with real gold prices and owned counts; inspection precedes Buy.

Dark double-bevel panels, red close controls and tier-only border ornament match the documented reference hierarchy more closely. Existing approved item/hero art is retained. Shop browsing suspends the preview camera; full-screen dimming covers taller aspect ratios and safe-area offsets. Server profile normalization now preserves legacy catalog tiers while adding progression fields.

Validation: final Windows export `Logs/step-10-20260928-192803.log`; strict text encoding PASS, 283 authored files. Desktop UI PASS 342 checks and touch-layout PASS 343 checks at four sizes, zero failures/runtime errors. Server persistence/account/admin/progression/event suites and the empty-body `/events` feed check pass; full movement/combat/save/reward/inventory/campaign regression passed on the existing build. A fresh Unity compile of the new event client adapter is pending the local Editor license being restored. Physical Android testing remains open. The 1024px-wide development capture still shows the existing world-camera pixel-reference warning; it is not a UI overflow failure and lower-resolution world rendering remains a device-acceptance item.

Reference coverage and unknown flows are recorded in [the UI audit](Docs/MYHEROES-UI-AUDIT.md). Core inventory/shop/profile samples and two available wiki pages received a second pass; matchmaking and unobserved social flows are explicitly not claimed as fully verified. Six-equipment/four-skill migration, saved loadouts, complete quest/profile/social screens and production world animation remain open. Six broad roadmap milestones remain open. Reopen **Play Echoes of the Rift.exe** to load this build.

## Historical build - revision 30, 28 September 2026

Progression presentation now separates fixed rarity stars from enhancement, with server/client profile normalization and validation. The event foundation is described above.

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

Export: `Logs/step-10-20260924-215701.log`. The old motion-preference control is absent. Trails and hit bursts now reuse capped pools; rarity/material/component lookups are cached, off-screen trails skipped, headless-server visual allocation skipped, and portrait/UI refresh rates reduced without changing gameplay simulation. Neon effects remain active. Existing desktop launcher opens this build.

Validation: 260 UI checks passed, including 12,000 requested trail emissions with a 128-object desktop cap and 0 managed bytes in the warmed 10,000-request loop. Solo campaign/equipment/save regression passed. Both dedicated co-op clients passed self-revive, teammate rescue and reward saving. These are local Windows tests, not an FPS benchmark or Android certification. Android build support/device testing remain outstanding. See MASTER-PLAN.md revision 21 for performance targets and remaining profiling work.


## Historical build — revision 20, 24 September 2026

Export: `Logs/step-10-20260924-215029.log`.

Validation: final export passed 259 UI checks with zero failures/runtime errors, including border-only motion, static icon core and click-through sparks. Desktop inventory captures at four resolutions and simulated notch were generated; 1280x720 visually reviewed. The first export of this pass also passed ten campaign runs, loot/save recovery, inventory/equipment/fusion, pause/defeat and no-campaign-revive checks. Final refinements only changed item presentation and UI checks. Dedicated account/co-op were not rerun in revision 20; their last full pass is revision 19.
 Dark violet curved frames, quiet slot interiors, rarity borders and tier-scaled perimeter sparks; static item icons; circular skill controls; inset item descriptions. Higher-tier world weapons emit denser trails without colour cycling. The old motion-preference control is intentionally absent; neon ornament and world trails remain active. Existing launcher uses this rebuilt executable.

The earlier revision 19 results below are historical. This presentation-only increment preserves existing account, economy and multiplayer contracts. Catalogue/character asset detail and a full reference-quality shop/paperdoll remain unfinished; see MASTER-PLAN.md revision 20.


## Visual update — revision 19, 24 September 2026

Current export: `Logs/step-10-20260924-212504.log`, `Builds/Windows/EchoesOfTheRift.exe`. Original large-head chibi heroes render at 48px at 720p. Town/dungeons are larger and scroll with a bounded follow camera; the HUD stays fixed. Friendly units/NPCs and all enemies/bosses have overhead HP bars. Four campaign bosses now have separate coloured silhouettes and attack timing/radius patterns. Skill bursts, hit sparks and projectile trails are brighter. Inventory/shop uses a 5x4 right-hand grid, left-side hero/item details and merchant headings.

Passed locally: 258 UI checks, ten campaign runs covering four boss IDs, scrolling, town safety and inventory, dedicated registration/auto-login/reconnect/rewards, and two-client self-revive/teammate rescue. Full directional animation, more unique skill choreography, detailed bespoke icons and broader boss behaviour remain open. This is a visual development pass, not finished production art.

Restart the server and client together to load the new maps; the previous running server was not stopped by development. Test servers now use isolated ports 18081/18082/17770, preserving normal ports 8081/8082/7770 and normal saved progression.

Unity 6000.6.2f1 pixel-art RPG prototype. Updated 23 September 2026, master plan revision 23.

Start **Echoes of the Rift Server.exe**, wait for readiness, then **Play Echoes of the Rift.exe** on the desktop. Keep the installed workspace in its existing location. The client does not start the server automatically; unavailable servers show a blocking splash error.

Latest build: `Builds/Windows/EchoesOfTheRift.exe`, built by `Logs/step-10-20260924-212504.log`. Rift Haven now passes local acceptance: 258 UI checks, local town/campaign regression, dedicated registration/reconnect/reward tests and both co-op revive methods. Real-device/mobile/public-network validation remains pending.

- [Master plan](MASTER-PLAN.md): authoritative requirements, implementation status and immediate next fixes.
- [MVP roadmap](MVP-ROADMAP.md): remaining milestones and release gates.
- [Playtest guide](PLAYTEST.md): launch, controls, town NPCs/gates and known limitations.
- [Server guide](Server/README.md): authority, accounts, persistence and tests.
- [Deployment guide](Server/DEPLOYMENT.md): local PC now, Oracle preparation later.
- [Execution log](EXECUTION-LOG.md): changes and dated evidence; historical passes are not current acceptance.

Current scope: four boss-led campaign stages, two-player co-op, inventory/fusion and local server accounts. No public server, mobile release, 4/8-player raid mode, real ads/purchases, live community publishing, gems or database-managed balance yet. Original art is a prototype pass and still needs the requested visual overhaul.
