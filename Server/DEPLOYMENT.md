# Echoes of the Rift — server ownership and deployment

## Current build - revision 45, 1 October 2026

The repository now ships a clone-ready Windows development payload in `../Release/Windows` with portable player/server launchers in `../Release/`. This is a convenience handoff, not a public deployment artifact: install Node dependencies locally, keep SQLite and admin credentials out of source control, and use the documented Linux/encrypted-transport gates before exposing services.

Production readiness now follows the player-first backlog in [FUTURE-DESIGN-BACKLOG.md](../Docs/FUTURE-DESIGN-BACKLOG.md): reliability and recovery, telemetry, real-device performance and safe live operations are release gates alongside features.

Live-event operations are now server-timed. Event manifests are versioned and stored in SQLite; `POST /events` returns `serverTime` plus active events, `POST /event-status` returns version-pinned player eligibility and claim receipts, and the loopback Swagger admin can publish or disable event versions. See [EVENTS-OPERATIONS.md](../Docs/EVENTS-OPERATIONS.md) for the reusable module catalogue, UTC scheduling rules, staging workflow, rollback and production safeguards. Before public deployment, add module-owned progress persistence, monitoring, restore drills and a separate production admin access boundary.

Revision 43 includes working OCI CLI authentication and the provisioned VCN, subnet, route, security list and restricted NSG in [OCI-DEPLOYMENT.md](OCI-DEPLOYMENT.md). The local Node services and dedicated Windows server/client pass their startup health checks. The current Windows x64 executable still cannot run on an Oracle Linux ARM VM, and OCI reports no host capacity for the A1 or E2 Always Free shapes in Singapore, so a compatible Linux build, encrypted gameplay transport and measured target-device/server tests remain required before public release.

Revision 30 adds server profile normalization for authoritative enhancement/skill fields while preserving legacy catalog tiers. Before public deployment, add the separate-stack enhancement transaction, server-owned reward rolls and audited cost validation; the migration and formulas are tracked in [Docs/PROGRESSION-MODEL.md](../Docs/PROGRESSION-MODEL.md).

Implemented the reference-led collection redesign: separate equipment/hero and five-column backpack panels, real equipped gear and six current stance skills, visible category tabs and a sort-choice menu. Items open in a dimmed inspection popup with rarity ribbon, large icon, real stat comparisons, story and Equip/Upgrade actions. Back closes the popup first and retains the browsing page; background controls stay blocked. Starter skills have a read-only inspection view. The merchant now uses a separate wide three-card layout with real gold prices and owned counts; inspection precedes Buy.

Dark double-bevel panels, red close controls and tier-only border ornament match the documented reference hierarchy more closely. Existing approved item/hero art is retained. Shop browsing suspends the preview camera; full-screen dimming covers taller aspect ratios and safe-area offsets. Server profile normalization preserves old balances and catalog identity.

Validation: final Windows export `Logs/step-10-20260928-192803.log`; server persistence/account/admin/progression suites, desktop UI 342 checks, touch UI 343 checks and full movement/combat/save/reward/inventory/campaign regression passed with no runtime errors. Physical Android testing remains open.

Reference coverage and unknown flows are recorded in [the UI audit](../Docs/MYHEROES-UI-AUDIT.md). Core inventory/shop/profile samples and two available wiki pages received a second pass; matchmaking and unobserved social flows are explicitly not claimed as fully verified. Six-equipment/four-skill migration, saved loadouts, complete quest/profile/social screens and production world animation remain open. Six broad roadmap milestones remain open. Reopen **Play Echoes of the Rift.exe** to load this build.

## Historical build - revision 28, 27 September 2026

Fixed malformed UTF-8 punctuation throughout the authored UI and repaired affected project documentation. Added a strict UTF-8/mojibake build gate plus runtime visible-label checks; `.editorconfig` specifies UTF-8. The gold/safe-zone, level/gems, movement/interact and gate strings no longer contain the stray accented A.

Reworked the reference-led HUD: cached pixel coin, faceted cyan gem and cog icons; top-right wallet counters, level below the upper-left portrait, backpack down the right edge, a separate safe-zone subtitle, smaller merchant service bubbles with nearby-only names, and matching inventory/shop currency headers. Boss/map spacing was adjusted around the wallet. Dark rounded surfaces remain; wallet labels update only when values change. Original approved hero/item art and combat contracts remain intact.

Moved sign-out to Settings > Accounts with a separate confirmation and Stay in game selected by default. The overlay blocks movement and bag interaction. Back first cancels confirmation, then closes settings and restores controls. Controls and Accounts tabs are available; no sign-out action remains on the town HUD.

Validation: Windows export `Logs/step-10-20260927-201608.log`; source encoding gate PASS (282 authored files). Desktop layout 319 checks and touch layout 320 checks passed across four sizes, zero failures/runtime errors, before the final NPC-height-only correction. Final build touch checks also PASS 320 with zero failures/runtime errors; final town capture confirms bubbles clear NPC faces. Authenticated registration and resume runs passed account cancellation (same token and connection), reconnect, campaign, server-only rewards, pause and rankings with no runtime errors. Native town, inventory, combat and Accounts/confirmation screenshots reviewed. Physical Android input/performance acceptance remains open.

References: [MyHeroes review and wiki links](../Docs/MYHEROES-GAMEPLAY-REFERENCES.md). This implements the HUD/settings feedback, not a complete recreation of MyHeroes. Remaining visual work includes authored town structures/NPC differentiation, directional animation, richer paperdoll and item comparisons. The broader six open milestones remain in the master plan; server economy/drop configuration is still the next backend slice. Reopen **Play Echoes of the Rift.exe** to load the rebuilt game.

## Historical build — revision 27, 27 September 2026

Implemented localhost Swagger administration at `http://localhost:8083/allah/api/hehe/v69/docs/`, with username/password authorization backed by an ignored salted scrypt hash. The parent-folder **Open Rift Admin.cmd** launcher starts/reopens it. See [admin guide](ADMIN.md) for query/edit workflow. Player/account/catalog queries, give/set gold and gems, set level, inventory stack creation/replacement/removal, equipped-copy protection, atomic audit history, idempotent request IDs and revision conflicts are implemented. Existing game/account URLs are unchanged; admin access remains loopback-only.

Gems, character Level and AdminRevision persist with legacy-save defaults. The town displays level/gems; large stack counts use compact labels. Catalogue exports include names. Reconnect refreshes admin edits; character level currently does not affect combat stats/campaign unlocks. Uncapped upgrades, DB-configurable economy/drop rules, achievements/inbox and live profile push remain open. A reconnect teardown guard prevents the expedition ally HUD from querying an uninitialized network object.

Validation: all three Node suites pass (persistence, accounts, admin), including wrong-password/Host/Origin rejection, old-route removal, duplicate grant prevention, revision conflicts and durable audit/profile reopen. Swagger renders in the browser at the requested localhost URL. Desktop UI passed 305 checks, zero layout failures/errors, before the teardown-only fix. Final Windows export: `Logs/step-10-20260927-153710.log`; dedicated registration/resume/reconnect regression PASS for both runs, including authentication, server-only grants, four campaign stages, central rewards, pause and rankings; no runtime errors. Production player rewards were not changed during tests; a consistent database backup was created first.

Review/next: finish milestone 4 with database balance/drop-rule versions and gem progression, then safe uncapped upgrades and achievement/inbox recovery. Six broad workstreams remain open; this admin foundation does not complete the entire economy milestone. Android and public-deployment acceptance remain pending.

## Historical build — revision 26, 27 September 2026

Export: `Logs/step-10-20260927-124226.log`. Added a named boss health bar, server-backed ally portrait/health/down-state for current two-player co-op, and a current-room minimap with player/ally/boss markers. Expedition panels hide in town and suspend with inventory. Boss health disappears on clear. Map bounds use real room geometry; maze/fog, public player names and larger-party support remain open.

Gameplay validation: dedicated Leader/Peer each PASS boss/party HUD visibility, boss-clear hiding, self-revive, teammate rescue and saved rewards. Solo combat/presentation and ten-run campaign/equipment/save/pause/defeat regression PASS. These checks preceded the final boss-bar position/legend-colour-only rebuild. Final build UI: 304 touch checks and 303 desktop checks PASS at four resolutions, zero layout failures/runtime errors. Boss damage/clear, solo party hiding and minimap bounds assertions pass. Final top-edge boss/map capture reviewed. No reported runtime errors. Physical Android/network/performance release gates remain open.

Roadmap consolidated into seven major milestones; this HUD foundation completes the first feature slice locally, with six broader milestones still open. See [current roadmap](../MVP-ROADMAP.md) and [master plan](../MASTER-PLAN.md) for precise scope, tests and next steps. Normal services were not stopped. Reopen the game to load the updated client.

## Historical build — revision 25, 27 September 2026

Export: `Logs/step-10-20260927-102610.log`. Touch movement/aim use fixed backgrounds and separate drag thumbs, bounded travel and independent finger ownership. Release/reset handles finger-up, cancellation, lost focus, application pause, HUD disable and disabled controls. Existing mouse/keyboard controls and server authority remain.

Validation: touch UI 303 checks PASS, desktop UI 302 checks PASS, zero layout failures/runtime errors across four resolutions. Simulated pointer tests cover simultaneous sticks, fixed backgrounds, ownership, independent release, focus loss, inventory reopening and cancellation. Windows touch screenshot reviewed. Solo combat/presentation fixtures and ten-run campaign/equipment/save/pause/defeat regression PASS with no reported runtime errors. Android module/physical device QA and FPS profiling remain pending. Dedicated pair/backend tests last passed revision 24/revision 23 respectively; neither backend nor protocol changed here.

QA: `Tools/Test-UI.ps1 -Touch` writes to `Logs/UILayoutTouch`; normal UI tests retain `Logs/UILayout`. `-touchControls` enables touch layout in a Windows player for inspection. See [master plan](../MASTER-PLAN.md) revision 25 for next steps. Normal server was not stopped; reopen the game using **Play Echoes of the Rift.exe** for the updated client.

## Historical build — revision 24, 26 September 2026

Export: `Logs/step-10-20260926-185717.log`. Five My Heroes gameplay videos sampled; see [reference review](../Docs/MYHEROES-GAMEPLAY-REFERENCES.md). HUD now uses compact portrait/vitals, slim objectives, a bag icon and three circular skills around a larger weapon attack control, with radial cooldowns. Client characters/projectiles interpolate between server snapshots; camera follow eases while retaining pixel snapping and immediate teleport/room changes. Remote appearance rebuilds are skipped when unchanged; enemy IDs remain stable during a stage. Approved art, 140-item catalogue and gameplay authority remain.

PASS: 302 UI checks across four resolutions, movement presentation fixture, combat/ten-run solo regression and final-build dedicated pair self-revive/teammate rescue/rewards. No reported runtime errors. Solo gameplay tests preceded the final text-height-only rebuild. Backend account/store Node suites last ran in revision 23. Android/device FPS and internet-jitter acceptance remain pending; no measured FPS claim. See [master plan](../MASTER-PLAN.md) revision 24 for ordered follow-up.

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

Export: `Logs/step-10-20260924-215029.log`. Dark violet curved frames, quiet slot interiors, rarity borders and tier-scaled perimeter sparks; static item icons; circular skill controls; inset item descriptions. Higher-tier world weapons emit denser trails without colour cycling. The old motion-preference control is intentionally absent; neon ornament and world trails remain active. Existing launcher uses this rebuilt executable.

The earlier revision 19 results below are historical. This presentation-only increment preserves existing account, economy and multiplayer contracts. Catalogue/character asset detail and a full reference-quality shop/paperdoll remain unfinished; see MASTER-PLAN.md revision 20.


## Visual update — revision 19, 24 September 2026

Current export: `Logs/step-10-20260924-212504.log`, `Builds/Windows/EchoesOfTheRift.exe`. Original large-head chibi heroes render at 48px at 720p. Town/dungeons are larger and scroll with a bounded follow camera; the HUD stays fixed. Friendly units/NPCs and all enemies/bosses have overhead HP bars. Four campaign bosses now have separate coloured silhouettes and attack timing/radius patterns. Skill bursts, hit sparks and projectile trails are brighter. Inventory/shop uses a 5x4 right-hand grid, left-side hero/item details and merchant headings.

Passed locally: 258 UI checks, ten campaign runs covering four boss IDs, scrolling, town safety and inventory, dedicated registration/auto-login/reconnect/rewards, and two-client self-revive/teammate rescue. Full directional animation, more unique skill choreography, detailed bespoke icons and broader boss behaviour remain open. This is a visual development pass, not finished production art.

Restart the server and client together to load the new maps; the previous running server was not stopped by development. Test servers now use isolated ports 18081/18082/17770, preserving normal ports 8081/8082/7770 and normal saved progression.

Status, 23 September 2026 (revision 18): Rift Haven passed local UI, town/server, account and co-op regressions. No public endpoint, firewall rule or cloud deployment changed. Separate-device and mobile acceptance remain pending.

Current choice: run the server separately on this PC; prepare Oracle next. No public IP, DNS, firewall rules or cloud resources were configured by this change.

## What players install

The game executable, Unity runtime, art/audio, UI, endpoint configuration and settings remain local. Windows also stores a protected login-session token. These are required for rendering/input and automatic sign-in. Phones will need their platform secure store.

Players do not receive Server/progress.sqlite, its journal files, backend source folder, service credentials, recovery database or operator tools. They never connect directly to SQLite. The game server validates combat and progression, and the backend commits rewards. Local cached display values are not authoritative.

The current development executable shares server code for testing. A public package still needs a non-development client build, server-only build separation, database-configured balance and removal of testing content. This increment changes startup/ownership; it does not certify the whole project for public release.

## Run locally now

1. Open **Echoes of the Rift Server.exe** on the desktop. Wait for its window to say the server is running.
2. Open **Play Echoes of the Rift.exe**. It launches only the client, checks server readiness, then permits registration/sign-in.
3. Close the game without stopping the server if you want it to stay available. Use **Stop server** or close the server control window to stop the server processes.

If no server is available, the client shows **Connection error / The server is unavailable. Please try again later.** above the splash, with Retry connection and Quit. Retry checks again without starting a server. Readiness requires both SQLite and a recent heartbeat from the actual game process; it is not just a successful web-server response. A stale game heartbeat expires after 10 seconds.

Current private services: game UDP 7770, persistence HTTP 127.0.0.1:8081, account/readiness HTTP 127.0.0.1:8082. HTTP is permitted only for same-PC development. Server data remains in Server/progress.sqlite. Keep backups outside the live database directory using SQLite-consistent backup tooling; do not copy only the main file during a write.

The client endpoint is configured in EchoesOfTheRift_Data/StreamingAssets/server.json in the build:

```json
{"address":"127.0.0.1","accountUrl":"http://127.0.0.1:8082"}
```

The two fields separate gameplay and account endpoints. HTTPS account URL configuration is prepared, but remote game-account access is still deliberately blocked until encrypted gameplay is implemented. Do not treat a changed address as completed internet deployment.

## What is needed before internet access

- A server machine kept online, compatible Unity server build, Node.js 24+, database storage and backups.
- A domain/subdomain and DNS pointing to a reachable public IP.
- HTTPS for the account API, using a maintained reverse proxy and automatic certificate renewal. A Caddy template is included for later deployment.
- Authenticated encrypted gameplay transport or an appropriate relay; the current Tugboat UDP setup is a development transport. A reverse proxy for the account API does not encrypt UDP gameplay.
- Host firewall plus router/cloud rules for only the chosen public services. Normally HTTPS TCP 443 and, for standard certificate/redirect setup, TCP 80; the gameplay port depends on the final transport. Current 7770/UDP is a development reference, not permission to expose it now.
- Keep persistence 8081 and the database completely private. Account 8082 remains loopback behind the HTTPS proxy. Never forward 8081/8082 or publish the SQLite file.
- Service restart supervision, monitoring, rate limiting at the edge, log rotation, backup restoration tests and a release client configured for the server.
- Capacity work: currently one expedition/two players per game process. The new town uses that same two-player process; a larger public town and independent parties still need session/instance routing work.

## Local PC with forwarded ports

Fastest to validate this exact Windows build, but power, Windows restarts, sleep, your upload bandwidth and home connectivity determine availability. First obtain a reachable public address from your ISP; ordinary router forwarding cannot traverse an upstream CGNAT without an ISP change or a suitable relay/tunnel. Reserve the server's LAN address, then configure only the final secure service ports. Test from an external network, not just your own Wi-Fi. No port forwarding is needed to play on this same PC.

Do not leave a personal desktop open to arbitrary database/admin connections. A separate machine or VM and an isolated service account are preferable for public hosting. Router/firewall changes are a later explicit deployment step, after encrypted transport is ready.

## Oracle Cloud preparation

Oracle is a better separation from your personal computer, but Always Free is suitable for experiments rather than a promise of production uptime. Current official documentation lists AMD E2.1.Micro (1 GB memory) and Arm A1 allowance equivalent to 2 OCPUs/12 GB total; verify the entitlement in your tenancy at provisioning time. Shapes can be out of capacity and idle free instances can be reclaimed. Do not rely on older 4-OCPU/24-GB tutorials.

The existing server executable is Windows x64. It will not run natively on an Oracle Linux Arm VM. Only Windows/WebGL build support is installed here today. Before choosing a shape, install the appropriate Unity server build support, produce a Linux build matching the selected CPU architecture, verify Fish-Net/native dependencies and measure memory/CPU under load. Do not assume the 1-GB micro is adequate for this Unity server, or that an x64 build can run on A1 Arm unchanged.

Once a compatible build is verified:
1. Provision the VM in your home region with persistent storage and a public address.
2. Set up a VCN/subnet, internet gateway/route, narrowly scoped NSG/security-list rules and OS firewall. Restrict administrative access to your own addresses or a bastion.
3. Install Node and the matched game-server runtime; run both under a non-root service account with restart supervision.
4. Keep the database and private API on the server, outside the web root; provision backups and monitoring.
5. Point DNS at the VM, configure HTTPS and the encrypted gameplay transport, and then update the client endpoint.
6. Test fresh registration, restart/reconnect, reward recovery and multiple real client devices before inviting players.

SQLite can remain the first server-owned database; choosing Oracle hosting does not require migrating to Oracle Database. Revisit the database and process architecture when concurrency/load requires it.

## References checked 23 September 2026

- Oracle Always Free: https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm
- Oracle network security: https://docs.oracle.com/en-us/iaas/Content/Network/Concepts/securityrules.htm
- Unity server builds: https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/dedicated-server/build
- Caddy automatic HTTPS: https://caddyserver.com/docs/automatic-https
- Tugboat transport: https://fish-networking.gitbook.io/docs/fishnet-building-blocks/transports/tugboat
