# My Heroes gameplay reference review — 26 September 2026

Scope: five publicly available gameplay videos were found and sampled in the browser. These are visual samples, not complete playthrough reviews or frame-time/input-latency measurements. Existing approved Echoes art is retained; the references inform layout and motion, not copied game assets.

| Video | Sample reviewed | Observed visual pattern |
|---|---|---|
| [Maximumandroid — My Heroes: Dungeon Raid, Android/iOS Gameplay](https://www.youtube.com/watch?v=gLHw_H5kPi8) | Around 0:38, ice-room combat | Compact upper-left status; small left objectives; right-edge utility icons; translucent movement control; circular lower-right skills; unobstructed centre. |
| [TanJinGames — Gameplay Walkthrough Part 1 (iOS)](https://www.youtube.com/watch?v=uvUURob7G6w) | Opening boss-combat montage, around 0:02 | Named boss bar at top; bright attacks separate from dark floors; large main attack beneath smaller skills; decimal cooldown labels. |
| [OJVG Channel — Android / iOS Games APK](https://www.youtube.com/watch?v=UNQ49qCSP94) | Around 10:40, green cyclone fight | Large colourful skill silhouettes; small character and overhead vitals remain identifiable; unobtrusive left-side objectives and slim bottom progress strip. |
| [KenYu Games — CBT Japan Gameplay](https://www.youtube.com/watch?v=5zyPn4QnxYg) | Around 4:45, field gameplay | Movement circle lower-left; primary attack larger than surrounding skill circles; consistent edge placement across rooms. |
| [PROAPK — My Heroes: SEA Gameplay Android / iOS](https://www.youtube.com/watch?v=9k_rK9_oSio) | Opening co-op combat montage, around 0:02 | Party portraits upper-left; map upper-right; compact left progress panel; overhead bars on characters; large attack control and smaller skill circles on right. |

## Translation into revision 24

- Smaller 72px portrait and 232x72 vitals replace the previous 112px portrait and 272x112 vitals at the 1280x720 reference canvas. HP/MP drain from the right toward a fixed left edge.
- Three actual active skills retain Q/E/R and their two-stance contract. They form a lower-right cluster around a 128px weapon attack/aim control. Radial cooldown shading supplements the remaining-time numbers. No decorative fake fourth slot is added.
- Inventory becomes a compact icon button at upper-right. The objective drawer is slimmer, remains collapsible and does not cover the central fight.
- Client replicas interpolate over the 50ms snapshot interval. Teleports and room entry snap immediately. The camera eases with a short exponential response before final 1/64-unit pixel snapping. Server gameplay remains authoritative; this adds presentation delay, not input prediction.
- Enemy replica IDs remain stable within a stage so a death cannot reassign another enemy's interpolated view. Unchanged remote appearance data no longer rebuilds the hero layers each snapshot.

## Remaining acceptance / next iteration

- Measure 10-minute crowded combat on a named low-end PC and real Android phone. The reference videos cannot establish our FPS or latency. Android build support/device testing remains outstanding.
- Test two real devices under latency/jitter and packet loss; tune interpolation and camera response from measurements. A fixed 50ms presentation window is the first implementation, not an adaptive jitter buffer or client prediction.
- Revision 26 implements named boss health, authoritative two-player ally portrait/health and a current-room dungeon map. Public player names/inspection, larger-party layout and multi-room topology/fog remain open.
- Revision 25 implements fixed stick backgrounds, cyan drag thumbs and independently owned/cancelled pointers. Windows pointer fixtures pass; physical Android acceptance remains open.
- Finish authored directional walk/attack/hit/death animation and distinct boss/skill animation sheets. Camera/interpolation polish does not complete character animation production.

## Revision 28 - town HUD and account navigation review (27 September)

Revisited [My Heroes: SEA by PROAPK](https://www.youtube.com/watch?v=9k_rK9_oSio), sampling 1:00 and 1:15. At 1:15 the profile/rename overlay has a dimmed lobby, lateral navigation and framed content; the wallet remains a compact icon-and-number row at the top right with backpack and settings on the right edge. Combined with the user's supplied lobby screenshots, these guide our wallet, utility rail, settings tabs and reduced NPC labels. These are sampled frames, not a claim to have watched the entire video this turn.

Read the [Quests UI wiki](https://myheroesofficial.fandom.com/wiki/Quests_(UI)) (lobby access and Adventure/Diary/Goals tabs) and [Skills information](https://myheroesofficial.fandom.com/wiki/Skills_(Information)) (skill books, rarity and ascension). These establish the reference's separated panel navigation; they do not authorize changing our existing skill-slot/economy contract. Reference behavior not yet implemented remains in the master plan. Original icons are drawn for our gold, gems and settings rather than importing the game's asset files.

## Revision 29 - element and interaction audit

See [MYHEROES-UI-AUDIT.md](MYHEROES-UI-AUDIT.md) for the two-pass core-screen comparison, source/version differences, unavailable wiki pages and implementation coverage. Backpack and merchant hierarchy now follow that audit; complete reference parity is still open.

## Revision 30 - reward and outcome presentation

Revision 94 uses the supplied My Heroes “You're received” screenshot as a composition reference for campaign outcomes: keep the dimmed game behind one centered, dark-framed reveal, give the actual item art and rarity color the strongest emphasis, then show saved currency and one forward action. Existing SEA/Dungeon gameplay samples continue to guide the stable edge anchors and uncluttered fight center; none of those sampled timestamps verifies this exact campaign-results interaction. Echoes uses its own item sprites and text. The result card source is awaiting a licensed build and capture review.
