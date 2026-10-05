# Play Echoes of the Rift - version 0.2 prototype

## Latest art UAT — revision 70, 5 October 2026

The optimized build passed a focused test for all four boss colors and all four attack phases, including visual recycling. Campaign movement confirms left-facing sprite flip and upward-facing weapon placement; the complete campaign progression/combat/save smoke passed. Desktop and touch layouts passed 350/351 checks with zero failures, and Rift Defense passed all 20 waves, four bosses and 15 towers with core health 100. The source and tracked Windows release build succeeded and have matching executable hashes. See `Logs/BossArtUAT`, `Logs/Runtime`, `Logs/UILayout`, `Logs/UILayoutTouch`, `Logs/BossHeroUAT`, and build log `Logs/step-10-20261005-083707.log`.

## Rift Defense reference-layout iteration — revision 66, 4 October 2026

The screen was reworked into a portrait reference layout with a deck strip, left-side enemy approach, 5x3 skill-book board, second inactive co-op board area, and compact bottom actions. A visible showcase opens through the development player with `-cookieSmoke -showRiftDefense`; the generated capture is `Logs/RiftDefensePreview/rift-defense-preview.png`. The updated build and deterministic 20-wave/four-boss stress test pass with no runtime errors. The inactive partner field makes the current solo scope clear; shared-board co-op is not implemented. This iteration follows the reference composition while retaining Echoes' own sprites; it is not a claim of exact visual or feature parity. Normal-run balance/timing, screen-size acceptance on physical devices, and low-end performance remain open. Research and next gates: [Rift Defense gameplay review](Docs/RIFT-DICE-DEFENSE.md).

## Latest UAT — revision 60, 3 October 2026

The current optimized Windows release passes Unity compile/build, 350 desktop and 351 touch-layout assertions, solo movement/combat/campaign/save/reward regression, isolated account registration/resume through all four server-authoritative campaign stages, and the full server test suite. The test runner waits for the dedicated server readiness heartbeat and isolates account/game ports and data. The current UI has a Campaign / Events / Inbox activity rail and contextual service interaction; the release-mode safe-area simulation is covered. UI parity remains below 90%, and sign-in/recovery form submission, confirmed sign-out/re-login, physical Android touch and low-end-PC frame-time/memory measurement remain open. See [My Heroes reference UAT](Docs/UAT-2026-10-03-MyHeroes.md).

Current player: [Release/Windows/EchoesOfTheRift.exe](Release/Windows/EchoesOfTheRift.exe). Unity build log: `Logs/step-10-20261003-202049.log`. Latest desktop/touch screenshots and runtime results are under `Logs/UILayout`, `Logs/UILayoutTouch`, `Logs/Dedicated/Register` and `Logs/Dedicated/Resume`.

## Previous optimization build - revision 39, 30 September 2026

Revision 39 includes the low-end allocation audit: reusable co-op snapshot buffers, non-allocating rune targeting, cached network components, change-only HUD text and change-only safe-area layout work. Android/iPhone use Low quality defaults and standalone uses Medium. Desktop/touch layout checks remain automated; physical Android and low-end-PC frame-time/memory capture remain open.

Inventory and item inspection now show fixed rarity as font-safe pixel stars and progression as a separate `+number`. Progression rules are centralized and validated for gear, skills, campaign pressure and the new-player carry. Server/client profiles now preserve separate progression fields with legacy fallback; the separate-stack uncapped transaction remains open.

Implemented the reference-led collection redesign: separate equipment/hero and five-column backpack panels, real equipped gear and six current stance skills, visible category tabs and a sort-choice menu. Items open in a dimmed inspection popup with rarity ribbon, large icon, real stat comparisons, story and Equip/Upgrade actions. Back closes the popup first and retains the browsing page; background controls stay blocked. Starter skills have a read-only inspection view. The merchant now uses a separate wide three-card layout with real gold prices and owned counts; inspection precedes Buy.

Dark double-bevel panels, red close controls and tier-only border ornament match the documented reference hierarchy more closely. Existing approved item/hero art is retained. Shop browsing suspends the preview camera; full-screen dimming covers taller aspect ratios and safe-area offsets. Server profile normalization now preserves legacy catalog tiers while adding progression fields.

Validation: final Windows export `Logs/step-10-20260928-192803.log`; desktop UI 342 checks and touch UI 343 checks passed with zero runtime errors. Server persistence/account/admin/progression suites and full movement/combat/save/reward/inventory/campaign regression passed. Physical Android testing remains open.

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


## Historical build — revision 20, 24 September 2026

Export: `Logs/step-10-20260924-215029.log`.

Validation: final export passed 259 UI checks with zero failures/runtime errors, including border-only motion, static icon core, click-through sparks and reduced-motion behavior. Desktop inventory captures at four resolutions and simulated notch were generated; 1280x720 visually reviewed. The first export of this pass also passed ten campaign runs, loot/save recovery, inventory/equipment/fusion, pause/defeat and no-campaign-revive checks. Final refinements only changed item presentation and UI checks. Dedicated account/co-op were not rerun in revision 20; their last full pass is revision 19.
 Dark violet curved frames, quiet slot interiors, rarity borders and tier-scaled perimeter sparks; static item icons; circular skill controls; inset item descriptions. Higher-tier world weapons emit denser trails without colour cycling. Reduced motion suppresses UI ornament and world trail emission. Existing launcher uses this rebuilt executable.

The earlier revision 19 results below are historical. This presentation-only increment preserves existing account, economy and multiplayer contracts. Catalogue/character asset detail and a full reference-quality shop/paperdoll remain unfinished; see MASTER-PLAN.md revision 20.


## Visual update — revision 19, 24 September 2026

Current export: `Logs/step-10-20260924-212504.log`, `Builds/Windows/EchoesOfTheRift.exe`. Original large-head chibi heroes render at 48px at 720p. Town/dungeons are larger and scroll with a bounded follow camera; the HUD stays fixed. Friendly units/NPCs and all enemies/bosses have overhead HP bars. Four campaign bosses now have separate coloured silhouettes and attack timing/radius patterns. Skill bursts, hit sparks and projectile trails are brighter. Inventory/shop uses a 5x4 right-hand grid, left-side hero/item details and merchant headings.

Passed locally: 258 UI checks, ten campaign runs covering four boss IDs, scrolling, town safety and inventory, dedicated registration/auto-login/reconnect/rewards, and two-client self-revive/teammate rescue. Full directional animation, more unique skill choreography, detailed bespoke icons and broader boss behaviour remain open. This is a visual development pass, not finished production art.

Restart the server and client together to load the new maps; the previous running server was not stopped by development. Test servers now use isolated ports 18081/18082/17770, preserving normal ports 8081/8082/7770 and normal saved progression.

Updated for master plan revision 23. Town labels and shop-state recovery are fixed. The current build passes 258 UI checks plus local campaign, town safety, account and co-op regressions. At 1024x768, the development camera still warns about being below its reference resolution; use 1280x720 or larger for the intended framing.

The Windows build is in `Builds/Windows`. Keep the whole folder together.

1. Open **Echoes of the Rift Server.exe** on your desktop and wait until its window says the server is running. Then open **Play Echoes of the Rift.exe**. The game launcher only starts the client; it never starts or stops the server. Keep this workspace in its current location. Node.js 24+ is already installed for the server.
2. Close the game when finished; the server remains available while its control window is open. Use **Stop server** in that window to stop it. If it is stopped, the game shows a connection error above the splash and asks you to try again later. Retry does not start a server. Manual server operation remains available through `Tools/Run-Dedicated.ps1`.
3. Click / Tap to Enter. Create a username (3-24 letters, numbers or underscores) and password (5-128 characters), or switch to Sign in. Copy the recovery code and keep it privately. Enter the adventure; new heroes then choose their race, hair and colours. Existing linked profiles keep their equipment and appearance. Later launches silently restore your account; click Enter adventure to continue.
4. Walk around **Rift Haven** with WASD. Walk close to an NPC or gate, then press F, tap its label or use Interact. The south gate starts solo campaign. The north gate explains that 4/8-player raids are unfinished and offers the explicitly labelled **2-player co-op test** when two players are connected. East (DPS trial) and west (world boss) show unavailable-mode messages. Each campaign stage ends when its boss dies; smaller enemies are optional.
5. Visit Hephaestus for melee/heavy gear, Artemis for ranged/light gear, Helios for magic gear, Asclepius for support accessories and Athena for books. These open filtered catalogues using the existing purchase/equip/fuse system; selling, consumables and passive training are not implemented.

Accounts in this build work on this computer only. Remote account connections are deliberately disabled until HTTPS and encrypted game transport are configured. No public server has been deployed. See `Server/README.md` for server setup, test commands and current deployment limits.

## Controls
- WASD: move. F / Interact: nearby town interaction. Mouse: aim. Left mouse: attack during expeditions. Town code disables attacks, skills, dodge and damage; local and dedicated town protection checks passed.
- Q/E/R: current three skills. Tab: switch stance. Space: dodge.
- I or Bag: inventory and live equipment inspection. Shop / Bag browses the catalogue. Select an item to see its count, description and village quote. Equip / Q equips gear or a skill book to the primary Q slot. Fuse consumes matching duplicates and gold.
- Objectives: collapse/expand the current stage objective.
- Inventory: Show cycles weapons, armour, charms, skills and all items. Sort cycles name, highest tier and largest quantity. Filters also work in the shop.
- Escape: close inventory or the gate dialog; otherwise pause/menu. Solo campaign pause reaches the server and resumes after three seconds. Co-op continues while menus are open.
- Co-op: F near a fallen friend starts a three-second rescue. A downed player can also use Revive; its ad method is deliberately blank and immediately requests revival. Full party wipes cannot revive.
- Campaign: no revives. Return to town after death and begin a new attempt.

The cookie feature has been removed. MP is now a real spell resource: casts cost 10 MP and regenerate at 8 MP/second. New skill effects and prices still need human balance testing.

## Progress
Normal progression is stored in `Server/progress.sqlite`, managed by the server. Windows caches a device-protected renewable login token; it does not save your password. Preserve the server database and your account/recovery information. Registration links a previous central local profile only when the saved device identity proves ownership. Earlier offline profile files remain untouched and are not automatically imported. Sign out is available in town; Recover account on the splash resets a password using the recovery code. Password recovery rotates the code and revokes saved sessions.

Failed central reward saves offer Retry save; repeated receipts cannot duplicate the same stage reward. Rankings show actual saved profiles. The development server supports two connected players and one expedition at a time. Solo campaign requires only one connected player. Town movement is newly replicated, but larger shared hubs and independent party instances are not implemented.

This remains a prototype. Revision 70 adds phased boss attack art and movement-facing for the hero; earlier revisions added the shared utility/effect atlases, dungeon creatures and dark-stone room tiles. Enemies use the world shader's 5% point-sampling pixel quantization. The optimized Windows build and 350/351 desktop/touch UI checks pass, the regular campaign smoke passes movement, progression, inventory, boss-only victory, rewards and recovery checks, and Rift Defense clears 20 waves/four bosses using 15 towers without runtime errors. Further screen polish, multi-stage effects, full visual acceptance, production networking, Android and low-end PC measurements remain open.

See Server/DEPLOYMENT.md for the client/server boundary, private database, home-hosting requirements and Oracle preparation.

## Current test status

Revision 70 validation is pending. Revision 69 passed on the optimized Windows release: campaign progression smoke, 350 desktop and 351 touch layout checks, and the 20-wave/four-boss/15-tower Rift Defense simulation, all without runtime errors. This did not rerun account registration or the full server suite; prior account, campaign and server results are recorded in EXECUTION-LOG.md. Physical-device performance, final visual acceptance and public deployment remain unverified. See MASTER-PLAN.md for the next art and gameplay gates.
# 3 October 2026 — My Heroes town navigation iteration

Revision 60 rebuilt and verified the activity rail/contextual action in the optimized Windows release. The test harness now honors simulated safe-area insets in a non-development player, and the account runner waits for the dedicated-server heartbeat before connecting. Current desktop/touch results are 350/351 with zero layout failures/runtime errors. Old generated Prototype/Smoke exports and duplicate legacy launchers have been removed; current Builds/Windows and historical logs/backups are retained. See [master plan](MASTER-PLAN.md) revision 60 and [UAT notes](Docs/UAT-2026-10-03-MyHeroes.md).
