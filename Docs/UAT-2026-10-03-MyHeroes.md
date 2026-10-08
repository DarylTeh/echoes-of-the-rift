# My Heroes reference UAT — 3 October 2026

## Follow-up — 9 October 2026, Moon/Ember skill assignment

The hero side now distinguishes the three Moon and three Ember positions. A player selects the destination on the paperdoll, then opens an owned book from the right-side collection; its detail card names the destination and gives the actual spell cooldown, power and mechanic. Local profile saves and dedicated-server transactions both store the selected slot. The server suite's skill transaction now checks slots 1–6, ownership, and removing books from assigned positions. Unity has no active license on this machine, so the new backpack interaction and screenshots remain unverified; the UI smoke test is queued for the next licensed build. No similarity score increase or Android-device certification is claimed.

## Follow-up — 9 October 2026, Shop browsing

The Shop now separates Best Buys, Catalog and Restock, with category navigation for All, Weapons, Armour, Charms and Skills. Best Buys filters out gear already owned by the account; the Catalog lists available items; Restock explains that no global schedule is configured. Price bands no longer block the card's inspect action. The first desktop and simulated-touch runs passed 357/358 layout and flow checks, but visual review found a category-rail/card collision and a RESTOCK/wallet collision at some sizes. The final source correction addresses those positions; Unity could not rebuild or rerun after that correction because the CLI reports no active license. The retained `Logs/ShopIteration/Desktop/merchant-catalog-1280x720.png` is from before the correction. Treat final Shop alignment as unverified until a licensed build is captured at 1280×720 and 1024×768. No Android-device test or 90% reference-parity claim is made.

## Follow-up — 8 October 2026, hero profile summary

Added the saved class and level to the paperdoll and replaced the generic gear-only totals with live Offense and Survival values plus secondary equipped-gear bonuses. The account model still has only three gear slots, so the panel does not imply unimplemented slots or profile fields. The four-resolution desktop suite passed 352 checks and simulated touch passed 353 with no layout failures or runtime errors; new assertions compare the displayed identity/stats to the saved profile and CharacterStats. Reviewed capture: `Logs/ReferenceLayoutIteration/ProfileDesktop/inventory-1280x720.png`. This remains a layout/progression improvement, not a 1:1 art match or physical Android certification.

## Follow-up — 8 October 2026, backpack spacing and metadata

Rebalanced the backpack's paperdoll and item grid to near-equal panel widths and full-height framing. Rarity stars now remain visible at cell size, and enhancement/count text occupies its own row; the inspector ribbon sits inside the centered detail card. The four-resolution desktop suite passed 351 checks and simulated touch passed 352, both with zero layout failures or runtime errors. These checks include cell label/icon separation, purchase/equip/upgrade transactions, equipped-item removal from the collection, paperdoll state and weapon-in-hand. The visually reviewed 1280×720 captures are `Logs/ReferenceLayoutIteration/ReviewedDesktop/inventory-1280x720.png` and `item-inspector-1280x720.png`. This corrects specific spacing defects but does not establish literal 1:1 or 90% similarity; the art and wider page flows remain notably different, and physical Android use remains unverified.

## Follow-up — 8 October 2026, high-tier gear effects

Rarity borders now gain a pulsing tier-colored rim and more/faster perimeter sparks (4/10/16 at tiers 3/4/5); enhanced gear adds a bounded number of extra motes. Equipped tier-4/5 weapons create brighter orbiting echoes and a larger attack spark fan, with higher tiers using larger aura/trail budgets under the existing mobile pool cap. The effect keeps item art/text still and drops decoration safely if the pool saturates. Desktop 350-check and simulated-touch 351-check UI suites passed. Full gameplay smoke passed movement, combat, tier fusion/aura/burst behavior, save/recovery and ten campaign runs with `runtimeErrors=False`. Mythic inspector capture: `Logs/GearVFXIteration/FinalDesktop2/item-inspector-1280x720.png`; gameplay smoke: `Logs/GearVFXIteration/Gameplay/runtime-smoke.txt`. No physical Android/low-end-PC frame-time result is claimed. This visual pass adds no ad or real-money SDK; future rewarded gems should be opt-in, transparent and server-verified.

## Follow-up — 8 October 2026, mobile landscape combat controls

Grouped touch-mode stance swap, three numbered skills, dodge and the primary attack into a compact two-row right-thumb cluster; desktop keeps the Q/E/R, Tab and Space keyboard hints and existing arrangement. The lower-left movement stick and open center lane remain unobstructed. The optimized Windows build passed 351 simulated touch-layout checks and 350 desktop checks with zero layout failures or runtime errors. Both 1280×720 captures were visually reviewed: `Logs/TouchControlIteration/combat-hud-1280x720.png` and `Logs/TouchControlIteration/Desktop/combat-hud-1280x720.png`. These scripted checks do not certify physical Android usability. The overall presentation remains below the requested 90% MyHeroes similarity.

## Follow-up — 8 October 2026, item inspection

Rebuilt the raised item popup as a centered card with a rarity/enhancement ribbon, selected icon, readable name/stats, separate effect/comparison and upgrade areas, and an integrated action rail. Reviewed captures at 1280×720, 1024×768, 1920×1080 and 1600×900 after correcting a title/icon collision and heading alignment. The packaged Windows player passed the 350-check UI suite with zero layout failures or runtime errors; the existing test fixture exercises purchase and equip actions. This improves the item-detail hierarchy but does not establish the requested 90% whole-game match. The item still uses original Echoes art and only displays fields supported by the current game data. Evidence: `Logs/ItemInspectorIteration/item-inspector-1280x720.png`, `runtime-smoke.txt`, and `layout-failures.txt`.

## Follow-up — 8 October 2026, inventory rarity and cell metadata

Item borders now use the asset's rarity color while enhancement only adds bounded perimeter motes. Spark/halo UI objects are allocated only for tiers that display those effects. Collection cells show the rarity band separately from enhancement and owned count, and the selected item name follows actual rarity. The Windows build passed 350 UI checks, including disposable-save Buy/Equip/Upgrade operations, with zero layout failures or runtime errors. The new inventory capture was reviewed at 1280×720; the suite also captured 1024×768, 1920×1080 and 1600×900. The tested build was synced to tracked `Release/Windows` after the game and local server were stopped. Evidence: `Logs/InventoryArtIteration/inventory-1280x720.png`, `runtime-smoke.txt`, and `layout-failures.txt`.

## Follow-up — 8 October 2026, town navigation

The oversized three-card town rail was reduced to a narrow vertical menu with a highlighted Campaign action and secondary Events/Inbox entries. Removed the centered title that obscured the guild-hall banner and relocated the movement hint away from the campaign gate. Four landscape sizes passed the 350-check UI suite; town/campaign movement, merchant stock, raid/trial/world-boss gates and Events/Inbox navigation passed without runtime errors. The visually reviewed town capture is `Logs/TownNavigationIteration/town-1280x720.png`. The fresh tested player was synced to tracked `Release/Windows`. Remaining gaps include character/NPC density, missing quest/social screens, art and the combat HUD.

## Follow-up — 8 October 2026, campaign combat HUD

Removed the large stage-name banner from the center of the battle view because stage number/objectives are already shown in the left panel. Boss name/health remains top-center, with room progress in the right minimap. The 350-check four-resolution player suite passed with no layout/runtime failures, including boss-health tracking, minimap bounds and stage progression. The 1280×720 combat capture was reviewed at `Logs/CombatHUDIteration/combat-hud-1280x720.png`, and the tested build was synced into tracked `Release/Windows`.

## Current verification — revision 62

The account form now has release-player UAT for failed Sign in and failed Recover account submissions. Each was submitted through its visible mode and primary action; both displayed the service’s error while keeping the user unauthenticated. The recovery capture confirms the new-password label and recovery-code input. Registration still passes through copy/acknowledgement, town entry and four campaign stages. Desktop UI passed 350 checks, touch-layout passed 351, and full server `npm test` passed. Build/release assembly SHA-256: `D7DB0BE44A6300955FA5FB306B9809DBEB4F0B39937797F646BE1F12BAC29563`.

The account test process reports and skips saved-session resume when DPAPI returns Win32 error 2. Successful Unity login/recovery and a confirmed sign-out/re-login cycle remain unverified. Screenshots: `Logs/Dedicated/Register/sign-in-rejected.png` and `recovery-rejected.png`. No physical Android or low-end-PC test was performed.

## Previous verification — revision 61

The current optimized Windows release now presents separate Create account, Sign in and Recover account choices; password labels follow the selected form. After successful authentication, inactive mode choices disappear. New-account/recovery responses require the one-time recovery code to be copied and the player to acknowledge saving it before Enter Adventure is enabled. The isolated registration flow verified the blocked state, copy action, acknowledgement and entry, then passed the server-authoritative four-stage campaign, rewards, pause and rankings.

Unity compile/build passed (`Logs/step-10-20261003-231819.log`), desktop layout passed 350 checks, touch-layout passed 351 checks, and full server `npm test` passed. `Release/Windows` and `Builds/Windows` contain the same assembly hash. The account test process could not validate protected-session persistence: Windows DPAPI `CryptProtectData` returned Win32 error 2. The game did not fall back to storing a plaintext token. `Run-Dedicated.ps1 -Account` reported and skipped only saved-session resume, then passed registration and gameplay. Validate refresh persistence from a normal foreground player session. No physical Android or low-end-PC test was performed.

The current non-development Windows player was rebuilt from source and copied into the tracked `Release/Windows` handoff. Rift Haven now includes the Campaign / Events / Inbox left activity rail, right-side Backpack and Settings, and an in-range contextual service/gate action. The rail and minimap remain pinned to the display edges. Release-mode layout tests now exercise simulated safe-area insets instead of silently bypassing them.

## Previous verification — revision 60

Unity compile and optimized Windows export pass (`Logs/step-10-20261003-202049.log`). The previous package passed 350 desktop and 351 touch-mode layout checks, the solo movement/combat/save/reward regression, isolated registration and resume through authenticated four-stage server-authoritative campaign rewards, pause and rankings, and the full server `npm test` suite. Account tests use a throwaway database and isolated ports. No physical Android or low-end-PC measurement was performed.

The refreshed UI remains **below the requested 90% reference match**. The activity hierarchy is clearer, but the town remains sparse and onboarding, item inspection, paperdoll detail, shop presentation and result hierarchy still diverge. The review does not claim full visual parity from layout assertions. Current captures are under `Logs/UILayout`, `Logs/UILayoutTouch`, `Logs/Dedicated/Register` and `Logs/Dedicated/Resume`.

## Result

The local Windows game/server stack starts and the isolated account-to-campaign flow passes. The packaged UI does **not** meet the requested 90% My Heroes: SEA / Dungeon Raid presentation or navigation match. Current perceived similarity is roughly 50–60% across the screens with usable reference evidence; this is a qualitative review, not a pixel-difference metric. Combat and the backpack retain the broad reference arrangement, while onboarding, town navigation, the item inspector and several screen proportions still diverge materially.

## Test conditions and evidence

- Normal local server was ready on loopback (`/health` returned `ready: true`); the Windows client opened and responded at 1296×759.
- Current: `Tools/Run-Dedicated.ps1 -Account` passed registration through recovery acknowledgement and entry, then four campaign stages, central rewards, pause and rankings (`auth=True`, `runtimeErrors=False`). It used a throwaway account and isolated database/game ports. DPAPI returned Win32 error 2 in the hidden test process, so the runner reported and skipped session resume. Revision 60's prior `-Account` registration/resume result is retained below as historical evidence, not current validation.
- `Tools/Test-UI.ps1` passed 350 layout checks with zero failures and no reported runtime errors. `Tools/Test-UI.ps1 -Touch` passed 351. These are scripted desktop-player layout checks; they do not establish visual parity or certify physical Android touch behavior.
- The normal client displayed “Please sign in again” after a saved session refresh failed. Its account form is the existing sign-in surface; no account details were entered and no saved session was cleared.
- Fresh isolated captures are under `Logs/Dedicated/Register` and `Logs/Dedicated/Resume`; current multi-resolution layout captures are under `Logs/UILayout` and `Logs/UILayoutTouch`.
- Comparisons use the user's supplied My Heroes screenshots and previously documented reference frames from [My Heroes: SEA](MYHEROES-GAMEPLAY-REFERENCES.md) and [My Heroes: Dungeon Raid](MYHEROES-GAMEPLAY-REFERENCES.md). The review does not claim a complete re-watch of either game during this UAT.

## Extended account, startup and recovery pass

The additional checks ran on 3 October using unused test-only ports. The existing user-facing server and local account database stayed untouched.

| Path | Status | Evidence / remaining limit |
|---|---|---|
| Branded splash / boot loading | **Missing** | Source creates `SplashScreenUI` directly over the game scene. There is no separate logo/brand splash scene or first-load progress screen. The account screen's small “Connecting to server…” prompt is the only startup progress feedback. |
| Service unavailable at launch | **Pass** | `Tools/Play.ps1 -Test` with an unused account port: `PASS unavailable aboveSplash=True blocksEntry=True retry=True runtimeErrors=False`. Screenshot: `Logs/Launcher/server-unavailable.png`. Retry kept the blocking state and never entered the game. |
| Create account | **Pass; session-cache verification limited** | Isolated release registration reached the account-ready state, hid the inactive account modes, blocked entry before recovery handling, then passed copy, explicit “I have saved my code” acknowledgement, and authenticated entry. The isolated hidden process received DPAPI Win32 error 2; it did not save a token, and session resume is unverified in this run. |
| Sign in | **API pass; UI rejection tested** | Server suite passed valid login and wrong-password rejection. The release UI now submits an invalid sign-in through its visible form and displays the error. Successful Unity sign-in remains untested. |
| Recover account | **API pass; UI rejection tested** | Server suite passed recovery, replacement recovery-code issue and revocation of prior sessions. The release UI submits invalid recovery details and displays the server error; its recovery-code and new-password fields are visible and correctly labeled. Successful Unity recovery remains untested. |
| Account-ready state | **Pass for registration** | Inactive Create account / Sign in / Recover account choices are removed. The copy action reveals an explicit saved-code acknowledgement; Enter Adventure stays disabled until acknowledgement. Login and recovery UI submission remain partial UAT paths. |
| Settings / account safety | **Pass for navigation and cancel; incomplete settings scope** | Fresh screenshot `Logs/Dedicated/Register/settings-accounts.png` shows sign-out inside Settings > Accounts. The confirmation defaults to “Stay in game”; the automated flow canceled it and verified the token/session remained valid. The controls tab and account tab pass layout checks at desktop/touch modes and four sizes. Controls are a static key map; audio, display, remapping and touch settings are absent. The actual confirm action was not invoked on a player account. |
| Sign out / return to sign in | **API pass; UI partial** | The API suite passed logout and session revocation. The Unity runtime test intentionally only exercised the safe cancel path; confirmed sign-out, return-to-title, then manual re-login remains to be verified with a disposable profile. |
| In-game connection loss | **Pass** | `Tools/Test-ConnectionFailure.ps1` used an unused test-only game port: `PASS missing server modal=True reconnectButton=True runtimeErrors=False`. Screenshot: `Logs/ConnectionFailure/connection-lost.png`. Reconnect is offered and no account/sign-out action appears. |
| Transitions and loading | **Partial / missing** | The isolated gameplay flow reaches the town and four campaign stages. No dedicated loading/transition screen or progress indicator was found; `Connecting to server…` and account-service text are brief status labels. Scene transitions should communicate what is loading and offer a clear retry if delayed. |
| Character creation and game entry | **Partial** | The layout suite exercises the creator, while the account-to-campaign smoke path directly confirms the creator in automation. A human first-time player has not yet been observed choosing appearance and entering town end to end. |

The full local `npm test` suite passed, including account registration, case-insensitive name uniqueness, wrong-password rejection, 5-character minimum, login, recovery, logout, ticket expiry/replay, refresh rotation, session revocation, concurrent registration and rate limits. This proves API behavior, not the matching Unity form behavior. `Tools/Test-UI.ps1` and `Tools/Test-UI.ps1 -Touch` respectively passed 342 and 343 checks; neither replaces hands-on testing on a touch device.

## Screen-by-screen findings

| Surface | UAT result | Gap against reference |
|---|---|---|
| First launch / account | Registration and resume are exercised against the current Windows release. | My Heroes' reviewed play flow opens in its lobby/game context. The separate “Click / Tap to Enter” step delays sign-in and does not match that hierarchy. Sign-in and recovery screens still need direct end-to-end UI submissions. |
| Town | Profile/vitals and wallet occupy familiar top corners; the live capture shows shop NPCs and gate interaction. | My Heroes has persistent left activity navigation and a denser lobby around the hero. Rift Haven is mostly empty floor with labels floating over world actors; some labels overlap the identity HUD, the hero and the interaction prompt. Utility navigation is too sparse and scattered. |
| Combat | The packaged layout has a boss bar, objectives, minimap, skill circles and a prominent weapon action. Scripted four-stage campaign flow passes. | My Heroes keeps the center clearer, places movement at lower left, groups compact objectives at left, and puts the weapon/skills on the right. Rift Haven's objective panel and keyboard-labelled controls are desktop-shaped and visually louder; touch spacing/parity has not been validated on an Android device. |
| Backpack | The paperdoll-plus-grid silhouette and five-column grid are recognizable. | Only three equipment slots are presented; the reference paperdoll is denser, with more gear locations and a fuller item collection. The skill shortcut is not a complete loadout/assignment screen. |
| Item detail | A distinct inspector opens over the collection and exposes equip/upgrade/story actions. | Its `+++++ +5` ribbon is unclear, the popup wastes space, supported comparison data is thin, and actions are detached at the side. Use a compact rarity-star row plus enhancement number and present only real item data in a tighter panel. |
| Shop | Catalogue filters, large item art and currency prices are present. | It reads as a three-card inventory variation. The reference has a dedicated Best Buys / D.Shop / Restock hierarchy, stock counts and clear offers. The event-specific equivalent remains unverified. |
| Results / ranking | Ranking panel opens and the automated flow records four-stage rewards. | The captured ranking overlay occupies almost the full game view and leaves substantial unused space. Reward, replay/next-stage and return-to-town actions need one clearer outcome-first screen. |

## Decision and next implementation slice

The flow is playable through the verified registration and campaign path, and UI rejection states are tested. Successful Unity sign-in/recovery, saved-session resume and confirmed logout→re-login still need normal foreground UAT. The visual/interaction target is **not accepted at 90%**. Finish those account paths and fresh first-time creator completion. Keep service-unavailable and connection-lost retries separate and non-destructive.

After those flow corrections, establish the compact My Heroes-style town activity rail, tighten overlapping world labels, tune combat anchors and complete the paperdoll/inspector/shop/result hierarchy against the existing screen wireframes. Repeat this UAT after the actual release rebuild and review touch controls on a phone; the current desktop result cannot certify Android usability.

## Runtime packaging note

The Windows release was missing Unity's `dstorage.dll` and `dstoragecore.dll`, which prevented startup on the initial attempt. The matching runtime pair from `Builds/Windows` is now included beside the release executable. The two files have matching byte sizes and SHA-256 hashes to their build-payload counterparts.
