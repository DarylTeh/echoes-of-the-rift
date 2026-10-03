# My Heroes reference UAT — 3 October 2026

## Iteration 59 — town navigation source update

Rift Haven source now has a compact left-side activity rail for Campaign, Events and Inbox; Backpack and Settings remain on the right. The old always-visible `Interact [F]` button is replaced with a contextual right-side action shown only in range, while the `F` key remains active. The rail, minimap and contextual action use edge anchoring so they can adapt across wide and narrow aspect ratios. The town runtime smoke test now checks that the activity rail, Campaign action and contextual action are present.

**Not visually accepted yet.** The installed Unity 6.0 editor could not compile or rebuild because it reported `No valid Unity Editor license found` (exit 198). Therefore no fresh player screenshot or desktop/touch run includes this iteration; prior 342/343 layout passes refer to the old packaged build. The actual target comparison must wait for a licensed rebuild, with new landscape and portrait captures. This iteration is structurally closer to the reference's fixed left navigation and contextual interactions, but the 90% visual target cannot be claimed from source alone.

## Result

The local Windows game/server stack starts and the isolated account-to-campaign flow passes. The packaged UI does **not** meet the requested 90% My Heroes: SEA / Dungeon Raid presentation or navigation match. Current perceived similarity is roughly 50–60% across the screens with usable reference evidence; this is a qualitative review, not a pixel-difference metric. Combat and the backpack retain the broad reference arrangement, while onboarding, town navigation, the item inspector and several screen proportions still diverge materially.

## Test conditions and evidence

- Normal local server was ready on loopback (`/health` returned `ready: true`); the Windows client opened and responded at 1296×759.
- `Tools/Run-Dedicated.ps1 -Account` passed twice (register and resume): `auth=True`, server-only rewards, four campaign stages, pause and rankings passed, with `runtimeErrors=False`. It used a throwaway account and isolated database/game ports, so no player account or normal local database was changed.
- `Tools/Test-UI.ps1` passed 342 layout checks with zero failures and no reported runtime errors at 1280×720. These checks validate bounds and scripted states; they do not establish visual parity.
- `Tools/Test-UI.ps1 -Touch` passed 343 touch-layout checks with zero failures and no reported runtime errors. This exercises the touch presentation mode in the desktop test build; it is not a physical Android-device test.
- The normal client displayed “Please sign in again” after a saved session refresh failed. Its account form is the existing sign-in surface; no account details were entered and no saved session was cleared.
- Fresh isolated captures are under `Logs/Dedicated/Register` and `Logs/Dedicated/Resume`. The normal client-only capture is `Logs/UAT/login-window.png`. Older multi-resolution layout captures are under `Logs/UILayout` and are dated 30 September; treat those as supporting visual evidence, not today's live play capture.
- Comparisons use the user's supplied My Heroes screenshots and previously documented reference frames from [My Heroes: SEA](MYHEROES-GAMEPLAY-REFERENCES.md) and [My Heroes: Dungeon Raid](MYHEROES-GAMEPLAY-REFERENCES.md). The review does not claim a complete re-watch of either game during this UAT.

## Extended account, startup and recovery pass

The additional checks ran on 3 October using unused test-only ports. The existing user-facing server and local account database stayed untouched.

| Path | Status | Evidence / remaining limit |
|---|---|---|
| Branded splash / boot loading | **Missing** | Source creates `SplashScreenUI` directly over the game scene. There is no separate logo/brand splash scene or first-load progress screen. The account screen's small “Connecting to server…” prompt is the only startup progress feedback. |
| Service unavailable at launch | **Pass** | `Tools/Play.ps1 -Test` with an unused account port: `PASS unavailable aboveSplash=True blocksEntry=True retry=True runtimeErrors=False`. Screenshot: `Logs/Launcher/server-unavailable.png`. Retry kept the blocking state and never entered the game. |
| Create account | **Pass, with UX defects** | Isolated Unity registration and account-ready screens were captured. The flow links a profile and displays a recovery-code instruction. It defaults to registration and places the title-entry button on a separate step. The code is only copied through a button; there is no explicit “copied” confirmation step before entry. |
| Sign in | **API pass; UI partial** | Server suite passed valid login and wrong-password rejection. Source shows the UI mode toggle and routes its submit action to `login`, but the runtime harness did not submit credentials through that mode or capture its final state. The toggle is ambiguously named “Register / Sign in.” |
| Recover account | **API pass; UI partial** | Server suite passed recovery, replacement recovery-code issue and revocation of prior sessions. The UI exposes username, recovery code and password fields, but the runtime harness did not submit this form end to end or capture the reset result. The new-password field is labelled only “Password.” |
| Account-ready state | **UX defect** | After successful registration, “Register / Sign in” and “Recover account” remain bright and visible even though their callbacks return without action once `account != null`. Hide or disable these actions after success; keep Copy recovery code and Enter adventure as the only active choices. |
| Settings / account safety | **Pass for navigation and cancel; incomplete settings scope** | Fresh screenshot `Logs/Dedicated/Register/settings-accounts.png` shows sign-out inside Settings > Accounts. The confirmation defaults to “Stay in game”; the automated flow canceled it and verified the token/session remained valid. The controls tab and account tab pass layout checks at desktop/touch modes and four sizes. Controls are a static key map; audio, display, remapping and touch settings are absent. The actual confirm action was not invoked on a player account. |
| Sign out / return to sign in | **API pass; UI partial** | The API suite passed logout and session revocation. The Unity runtime test intentionally only exercised the safe cancel path; confirmed sign-out, return-to-title, then manual re-login remains to be verified with a disposable profile. |
| In-game connection loss | **Pass** | `Tools/Test-ConnectionFailure.ps1` used an unused test-only game port: `PASS missing server modal=True reconnectButton=True runtimeErrors=False`. Screenshot: `Logs/ConnectionFailure/connection-lost.png`. Reconnect is offered and no account/sign-out action appears. |
| Transitions and loading | **Partial / missing** | The isolated gameplay flow reaches the town and four campaign stages. No dedicated loading/transition screen or progress indicator was found; `Connecting to server…` and account-service text are brief status labels. Scene transitions should communicate what is loading and offer a clear retry if delayed. |
| Character creation and game entry | **Partial** | The layout suite exercises the creator, while the account-to-campaign smoke path directly confirms the creator in automation. A human first-time player has not yet been observed choosing appearance and entering town end to end. |

The full local `npm test` suite passed, including account registration, case-insensitive name uniqueness, wrong-password rejection, 5-character minimum, login, recovery, logout, ticket expiry/replay, refresh rotation, session revocation, concurrent registration and rate limits. This proves API behavior, not the matching Unity form behavior. `Tools/Test-UI.ps1` and `Tools/Test-UI.ps1 -Touch` respectively passed 342 and 343 checks; neither replaces hands-on testing on a touch device.

## Screen-by-screen findings

| Surface | UAT result | Gap against reference |
|---|---|---|
| First launch / account | Flow works in the isolated test; a title screen precedes the account form. | My Heroes' reviewed play flow opens in its lobby/game context. The separate “Click / Tap to Enter” step delays sign-in and does not match that hierarchy. The packaged password label says `15+ characters`, while current source and account rules specify `5+`; rebuild the Windows release from current source. |
| Town | Profile/vitals and wallet occupy familiar top corners; the live capture shows shop NPCs and gate interaction. | My Heroes has persistent left activity navigation and a denser lobby around the hero. Rift Haven is mostly empty floor with labels floating over world actors; some labels overlap the identity HUD, the hero and the interaction prompt. Utility navigation is too sparse and scattered. |
| Combat | The packaged layout has a boss bar, objectives, minimap, skill circles and a prominent weapon action. Scripted four-stage campaign flow passes. | My Heroes keeps the center clearer, places movement at lower left, groups compact objectives at left, and puts the weapon/skills on the right. Rift Haven's objective panel and keyboard-labelled controls are desktop-shaped and visually louder; touch spacing/parity has not been validated on an Android device. |
| Backpack | The paperdoll-plus-grid silhouette and five-column grid are recognizable. | Only three equipment slots are presented; the reference paperdoll is denser, with more gear locations and a fuller item collection. The skill shortcut is not a complete loadout/assignment screen. |
| Item detail | A distinct inspector opens over the collection and exposes equip/upgrade/story actions. | Its `+++++ +5` ribbon is unclear, the popup wastes space, supported comparison data is thin, and actions are detached at the side. Use a compact rarity-star row plus enhancement number and present only real item data in a tighter panel. |
| Shop | Catalogue filters, large item art and currency prices are present. | It reads as a three-card inventory variation. The reference has a dedicated Best Buys / D.Shop / Restock hierarchy, stock counts and clear offers. The event-specific equivalent remains unverified. |
| Results / ranking | Ranking panel opens and the automated flow records four-stage rewards. | The captured ranking overlay occupies almost the full game view and leaves substantial unused space. Reward, replay/next-stage and return-to-town actions need one clearer outcome-first screen. |

## Decision and next implementation slice

The flow is playable through automated registration, resume, town, campaign, rewards and ranking checks. The visual/interaction target is **not accepted at 90%**. First make one coherent entry flow: branded splash with an honest loading state, then explicit Sign in / Create account / Recover account screens, followed by character creation only when needed. After registration, show and confirm recovery-code handling, then enter the game; after logout, return to Sign in. Add UI automation for successful/failed login, successful/failed recovery, confirmed logout and fresh first-time creator completion. Keep service-unavailable and connection-lost retries separate and non-destructive.

After those flow corrections, establish the compact My Heroes-style town activity rail, tighten overlapping world labels, tune combat anchors and complete the paperdoll/inspector/shop/result hierarchy against the existing screen wireframes. Repeat this UAT after the actual release rebuild and review touch controls on a phone; the current desktop result cannot certify Android usability.

## Runtime packaging note

The Windows release was missing Unity's `dstorage.dll` and `dstoragecore.dll`, which prevented startup on the initial attempt. The matching runtime pair from `Builds/Windows` is now included beside the release executable. The two files have matching byte sizes and SHA-256 hashes to their build-payload counterparts.
