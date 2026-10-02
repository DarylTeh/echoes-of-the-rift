# Echoes of the Rift — dedicated server package

## Current build - revision 54, 2 October 2026

The clone-ready handoff is under `../Release/Windows`. Run `npm ci --prefix Server --ignore-scripts` once, then use the portable launchers in `../Release/`. The scripts and launchers prefer this tracked payload and fall back to `Builds/Windows` when developing from a locally generated Unity export. SQLite progress, logs, admin hashes and credentials remain local and ignored. Current account validation accepts passwords from 5 through 128 characters. The shared store uses indexes for expiry, inbox, event and audit hot paths plus a five-second busy timeout.

Event progress is server-owned. Active manifests may define a progress metric, cap and eligible modes; verified campaign reward receipts advance the matching metric once per idempotent receipt. `/event-status` returns version-pinned progress and claim state. Authenticated `POST /event-shop-status` returns current offers, prices, rewards, shared stock, event-token balance, per-player purchase allowance and eligibility reasons without mutating state. Authenticated `POST /event-shop-purchase` revalidates those values transactionally, deducts tokens, reserves stock and queues the reward. Supply a unique `requestId` per purchase attempt and reuse it for retries; the server returns the original receipt without spending twice.

The client/server event status work remains version-pinned. The runtime audit also reduced per-snapshot allocation pressure and repeated UI work for low-end targets. The checked-in client endpoint is loopback (`127.0.0.1`), so the game does not connect to Oracle. A live OCI CLI audit on 2 October found no VM or reserved public IP; Always Free A1/E2 capacity is unavailable in the only subscribed region, Singapore. Paid E5 capacity is available at an estimated US$61.32/month for compute (2 OCPUs/12 GB, before storage/network/tax), but no paid resources have been launched. The installed Unity Editor lacks Linux Standalone support, and no compatible game-server binary exists. See [OCI-DEPLOYMENT.md](OCI-DEPLOYMENT.md) for the full status and decision gate.

The Unity client opens server-described event detail cards and requests version-pinned status from `POST /event-status`; the response reports event state, eligibility and claim receipts. Event progress is advanced from verified campaign receipts. The event-shop status and purchase routes are ready for client integration; the bazaar UI remains behind wireframe feedback.

The award-informed future design priorities are tracked in [FUTURE-DESIGN-BACKLOG.md](../Docs/FUTURE-DESIGN-BACKLOG.md). Rift Defense has a reusable `tower_defense` event template, with server-authoritative deck, seed, wave, score and reward rules planned in [RIFT-DICE-DEFENSE.md](../Docs/RIFT-DICE-DEFENSE.md). Server work follows the player-first order: event visibility and recoverable rewards, event progress, verified combat/build systems, social recognition, then larger live content.

The server owns reusable live-event manifests and UTC schedule boundaries. `POST /events` returns server time and active versioned manifests; SQLite stores historical revisions, idempotent claim receipts and transactional reward inbox entries. The loopback Swagger admin exposes `GET/POST /api/events` and `POST /api/events/{id}/disable` for authoring, staging and kill-switch control. The Unity client presents the read-only event drawer and recoverable Reward Inbox. Clients can now request the shop's current offer/status state before attempting a purchase.

The client now presents rarity stars separately from enhancement. The server normalizes legacy profiles with `EnhancementLevel`, `SkillLevels`, `EquippedEnhancementLevels` and `ProgressionVersion`, while preserving catalog tiers and existing balances. The production work still open is the authoritative uncapped enhancement transaction and audited reward-cost validation described in [the progression model](../Docs/PROGRESSION-MODEL.md).

Implemented the reference-led collection redesign: separate equipment/hero and five-column backpack panels, real equipped gear and six current stance skills, visible category tabs and a sort-choice menu. Items open in a dimmed inspection popup with rarity ribbon, large icon, real stat comparisons, story and Equip/Upgrade actions. Back closes the popup first and retains the browsing page; background controls stay blocked. Starter skills have a read-only inspection view. The merchant now uses a separate wide three-card layout with real gold prices and owned counts; inspection precedes Buy.

Dark double-bevel panels, red close controls and tier-only border ornament match the documented reference hierarchy more closely. Existing approved item/hero art is retained. Shop browsing suspends the preview camera; full-screen dimming covers taller aspect ratios and safe-area offsets. No server schema or economy rules changed.

Validation: final Windows export `Logs/step-10-20260927-210718.log`; strict text encoding PASS, 283 authored files. Desktop UI PASS 342 checks and touch-layout PASS 343 checks at four sizes, zero failures/runtime errors. Includes starter-skill text, all 140 item descriptions/lore, category filtering, nested Back, keyboard focus, border effects and actual Buy/Equip/Upgrade against disposable saves. Final native backpack, item popup and merchant captures reviewed. Solo ten-run campaign/save-recovery regression and isolated dedicated account registration/resume regression passed with no runtime errors. Physical Android testing remains open. The 1024px-wide development capture still shows the existing world-camera pixel-reference warning; it is not a UI overflow failure and lower-resolution world rendering remains a device-acceptance item.

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

Status, 23 September 2026 (master plan 19): current town build passes UI, local gameplay and dedicated account/pair tests, including town movement/protection, reconnect, rewards and revives. This is local acceptance, not public-network or mobile certification.

This version connects clients to a separately launched Fish-Net game server. There is no player-hosting button. The default endpoint is `127.0.0.1`. Normal account connections are currently restricted to loopback; changing the endpoint alone does not enable public account play.

## Run on this Windows development machine

Requirements: Node.js 24+ and the exported Windows game folder.

From a fresh clone, run `npm ci --prefix Server --ignore-scripts`, then open `Release/Echoes of the Rift Server.exe` and `Release/Play Echoes of the Rift.exe`. Alternatively, from the project root run `Tools/Run-Dedicated.ps1`, then launch `Release/Windows/EchoesOfTheRift.exe` (or the editor-generated `Builds/Windows/EchoesOfTheRift.exe`). Keep the server script running. It starts the private SQLite service and a headless game-server process. Stop the script to stop both. The headless game process listens on UDP 7770; the database service listens only on 127.0.0.1:8081.

The script passes a fresh private service key through the child-process environment. It never places that key in the client build. Normal Windows clients store a DPAPI-protected renewable account token locally; inventory, gold, equipment, skill selection and campaign completion live in `Server/progress.sqlite`.

For a remote deployment, implement an HTTPS account endpoint and encrypted gameplay transport, then configure/provision the game and persistence services. No public host has been provisioned by this task. The current transport is a development UDP transport; an authenticated encrypted transport/relay and deployment monitoring are still required before public release.

## Authority and persistence

- Clients submit movement, aim, attack, dodge, skill selection and menu requests.
- The game server clamps input, runs cooldowns/damage/revive/wipe rules and determines boss completion. Revision 17 adds town snapshots while no expedition is running, movement bounds, and safe-zone action/damage rejection; these paths passed local dedicated acceptance tests.
- Inventory requests are checked against the server catalogue. Clients cannot submit replacement profiles or gold balances.
- SQLite transactions implement purchases, equipment, skill-book selection and duplicate fusion. Reward receipts are unique per account/run/stage, so a failed-save retry cannot duplicate a grant.
- Campaign pause reaches the game server; co-op menus never freeze the party.
- Connection failure shows a pixel-art retry modal and a manual Reconnect button.
- Rankings are read from central profiles and include an equipment preview.

The current package supports one campaign or one two-player expedition per game-server process. Matchmaking, multiple simultaneous parties, hostile-network hardening and production deployment remain follow-up work. Recovery-code account recovery is implemented; active gameplay-session revocation remains open. The SQLite service is intentionally private; expose only the game transport appropriate to your deployment.

## Town integration

Five NPCs expose filtered views of the existing catalogue; the backend still validates purchases/equipment/fusion. No new currencies, prices or drop rules were introduced. Four gate destinations are presented, but only the existing solo campaign and explicitly labelled two-player co-op test launch gameplay. There is no 4/8-player matchmaking, DPS trial or world boss implementation.

Normal clients cannot select the safe-zone state. The server sets it when entering/leaving town; client combat controls mirror it. New development-only TestTownSafetyServerRpc checks damage/skill/dodge rejection. Local TestTown also checks movement, Athena filtering and an unavailable gate. These tests passed in revision 18 for both account sessions and both co-op clients.

## Verification

- `node Server/test.mjs`: credentials, atomic fusion, invalid transactions and duplicate receipts.
- `Tools/Test-UI.ps1`: current town build PASS, 258 checks / zero layout findings.
- `Tools/Run-Dedicated.ps1 -Account`: registration, protected session, reconnect and campaign rewards; now also exercises town movement/safety.
- `Tools/Run-Dedicated.ps1 -Test`: private service + headless server + authenticated client; four campaign stages, server-only rewards, pause and rankings.
- `Tools/Run-Dedicated.ps1 -Pair`: private service + headless server + two authenticated clients; placeholder self-revive, teammate rescue and shared saved loot.
- `Tools/Run-Playtests.ps1`: isolated development-only offline visual/campaign checks. The `-Coop` variation is a legacy transport regression fixture; it is not a hosting option in the normal game.

Test-only boss kills are accepted only by a Development build launched with `-dedicatedTest`; the normal server launcher does not enable them. Test profiles and databases are isolated from normal progression. Logs are under `Logs/Dedicated`, `Logs/DedicatedPair` and `Logs/Runtime`.

## Assets and regeneration

`ItemDatabaseGenerator.BuildCLI` creates 115 catalogue entries and 30 skill definitions, plus original pixel icons and Tiny5 font assets. `ProceduralPixelSpriteGenerator.GenerateRaceSheetsCLI` exports nine aligned 32px layer sheets. The runtime uses the same deterministic pixel drawing code for RGB variants and animated weapon frames.

Tiny5 is licensed under the SIL Open Font License; its license is bundled in `Assets/_Project/Resources/Pixel/FONT-LICENSE.txt`. The My Heroes UI and Brave Frontier references guided the requested direction; their game assets are not bundled.


## Local accounts (Step 2)

The persistence process also serves the limited account API on 127.0.0.1:8082. It never binds this API to a public interface. Register/login/recover use salted scrypt password hashes (N=32768, r=8, p=3, 64 MiB allocation limit). Human passwords are not processed with the older fast hash used for random prototype secrets.

Sessions last up to 30 days and rotate when refreshed; joining uses a two-minute single-use ticket. Only its hash is stored in SQLite. Windows persists refresh tokens with current-user DPAPI; passwords are cleared from input controls after submission. Non-Windows platforms have no plaintext fallback and need a platform secure-store implementation before auto-login ships there. Account endpoint rate limiting is process-local (30 requests/minute/IP; at most four concurrent operations), suitable for this loopback prototype, not a distributed production abuse-control system.

Recovery codes are displayed through an explicit Copy recovery code action, never saved in client preferences; the server stores their hashes. Recovery resets the password and invalidates sessions/tickets. Existing gameplay sessions are not forcibly terminated by password recovery in this version; production session revocation enforcement remains open.

Legacy device accounts can be linked on registration only by proving the previous secret and retaining their original player ID. Normal production-server login consumes tickets. Legacy secret login is restricted to the explicit development test-server flag. Do not run -dedicatedTest on public infrastructure.

Validation: `node Server/accounts.test.mjs` exercises credentials, ticket expiry/replay, refresh rotation/expiry, recovery/revocation, legacy ownership/progress, concurrent registration and HTTP rate limits. `Tools/Run-Dedicated.ps1 -Account` registers through the Unity UI, checks encrypted device storage, exits/relaunches for automatic login, reconnects and completes campaign rewards. Account tests use isolated databases and test-only device cache filenames.

Remaining deployment work: TLS/proxy endpoint configuration, encrypted gameplay, Android/iOS secure storage, distributed rate limits, active-session revocation, session management UI, and external security review. This is a working local account increment, not public account-service readiness.

## Startup readiness and deployment preparation

The normal game launcher is client-only. The separate server control window owns and stops its server processes. The public account-side `/health` returns 503 until SQLite is accessible and the game process has sent a recent private heartbeat; readiness expires after 10 seconds without one. The splash checks readiness on every launch and blocks behind a higher-order modal when unavailable. Endpoint fields are in StreamingAssets/server.json. See DEPLOYMENT.md and Caddyfile.example; no cloud resources, certificates or firewall rules were provisioned.
