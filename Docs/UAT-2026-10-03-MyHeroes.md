# My Heroes reference UAT — 3 October 2026

## Result

The local Windows game/server stack starts and the isolated account-to-campaign flow passes. The packaged UI does **not** meet the requested 90% My Heroes: SEA / Dungeon Raid presentation or navigation match. Current perceived similarity is roughly 50–60% across the screens with usable reference evidence; this is a qualitative review, not a pixel-difference metric. Combat and the backpack retain the broad reference arrangement, while onboarding, town navigation, the item inspector and several screen proportions still diverge materially.

## Test conditions and evidence

- Normal local server was ready on loopback (`/health` returned `ready: true`); the Windows client opened and responded at 1296×759.
- `Tools/Run-Dedicated.ps1 -Account` passed twice (register and resume): `auth=True`, server-only rewards, four campaign stages, pause and rankings passed, with `runtimeErrors=False`. It used a throwaway account and isolated database/game ports, so no player account or normal local database was changed.
- `Tools/Test-UI.ps1` passed 342 layout checks with zero failures and no reported runtime errors at 1280×720. These checks validate bounds and scripted states; they do not establish visual parity.
- The normal client displayed “Please sign in again” after a saved session refresh failed. Its account form is the existing sign-in surface; no account details were entered and no saved session was cleared.
- Fresh isolated captures are under `Logs/Dedicated/Register` and `Logs/Dedicated/Resume`. The normal client-only capture is `Logs/UAT/login-window.png`. Older multi-resolution layout captures are under `Logs/UILayout` and are dated 30 September; treat those as supporting visual evidence, not today's live play capture.
- Comparisons use the user's supplied My Heroes screenshots and previously documented reference frames from [My Heroes: SEA](MYHEROES-GAMEPLAY-REFERENCES.md) and [My Heroes: Dungeon Raid](MYHEROES-GAMEPLAY-REFERENCES.md). The review does not claim a complete re-watch of either game during this UAT.

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

The flow is playable through the automated registration, town, campaign, rewards and ranking checks. The visual/interaction target is **not accepted at 90%**. Keep the existing verified corner anchors, pixel-art presentation, account safety and server-authoritative progression. Next, bring the packaged client current, remove the extra title-entry step, establish a compact My Heroes-style town activity rail, tighten overlapping world labels, then tune combat anchors and replace the partial paperdoll/inspector/shop hierarchy against the existing screen wireframes. Repeat this UAT after the actual release rebuild and review touch controls on a phone; the current desktop result cannot certify Android usability.

## Runtime packaging note

The Windows release was missing Unity's `dstorage.dll` and `dstoragecore.dll`, which prevented startup on the initial attempt. The matching runtime pair from `Builds/Windows` is now included beside the release executable. The two files have matching byte sizes and SHA-256 hashes to their build-payload counterparts.
