# MyHeroes: SEA UI reference audit

## Strict full-screen audit — 8 October 2026

This pass reviews the current Echoes build against the supplied MyHeroes: SEA/Dungeon Raid screenshots, the existing gameplay-reference notes, and the accessible wiki pages. It covers the implemented UI surfaces found in the Unity scripts and available captures. The implementation still falls well short of the requested 90% resemblance.

### Baseline

**35/100 qualitative similarity** across comparable game surfaces. This is a design-review score, not an automated pixel comparison. The score uses four criteria: layout and hierarchy 35%, navigation and interaction 25%, visual design and art 30%, and coverage of major reference surfaces 10%. The largest penalty is the art system: Echoes has its own small pixel sprites and icons, but they do not have the reference's character proportions, detail density, icon language, item frames, colorful rarity treatment, or finished visual polish. Functional overlap alone does not count as visual similarity.

| Surface | Similarity | Strict assessment |
|---|---:|---|
| Title/splash and account entry | N/A (no direct in-game equivalent) | Current entry is a sparse dark title/account flow. It is readable, but not evidence of MyHeroes lobby style. Account create/sign-in/recovery and server-error states are game-specific. |
| Character creator | 15% | Basic character preview and choices exist; composition and asset treatment do not resemble the dense framed hero/profile screens. |
| Town / lobby | 46% (revision 84 provisional) | The guild hall now has an original detailed pixel backdrop, a compact left activity rail, top-left hero/vitals, upper-right wallet/settings, right-side bag/minimap and world destinations. It still lacks the reference's busy NPC density, quest/diary/friends/guild flows, matching character/icon art and consistent contextual markers. |
| Campaign combat HUD | 58% (revision 85 provisional) | Corner hierarchy, boss bar, objective block, currency, bag and lower-right skill/attack controls are directionally similar. The duplicate center stage banner is removed, leaving the combat lane clear. Panels, minimap, control geometry, iconography, effect integration and original art still differ; desktop captures omit touch-stick presentation. |
| Backpack / equipment | 62% (revision 83 provisional) | The two-column paperdoll plus five-column collection grid, categories, clear equipped state and weapon in the hero's hand are the strongest structural match. Rarity-colored borders and separated star/enhancement/count labels improve scanability; the preview is still plain/small, the slot system is sparse, and the fuller equipment/profile/status composition is missing. |
| Item inspection | 54% (revision 82 provisional) | It is now a centered overlay with a rarity ribbon, selected art, supported stats, effect/comparison inset, upgrade footer and integrated action rail. It still lacks the reference's original art, class-specific data where the game model has none, richer item-specific effects/source detail, and conditional socket presentation. |
| Shop / merchant | 32% | Filter tabs, cards and wallet are present. Current cards are oversized and sparse; they do not follow the reference's multi-section Best Buys/D.Shop/Restock shell, category rail, stock/discount hierarchy, purchase preview and reward reveal flow. |
| Skill loadout / skill collection | 20% | A six-icon stance strip and in-combat skill buttons exist, but there is no complete reference-like owned/equipped skill-book page with a clear loadout region, dense collection, class/quality filters and supported upgrade states. |
| Settings | 25% | Controls and Accounts tabs exist, and sign-out is protected behind Accounts with a confirmation. It is functional but generic and visually separate from the reference's framed, icon-rich game UI. The safer sign-out placement should remain. |
| Events | 18% | A server-timed event list/detail drawer exists and displays eligibility/progress/countdown. It is a basic text list, not the illustrated event/quest cards, reward track, tabs, claim states and destination flow expected from the references. |
| Reward inbox | 20% | Server-backed pending rewards and collect/retry states exist. It is a plain list with no rich item-card/reward preview or polished receipt/reveal treatment. |
| Rankings | 14% | A ranking list and hero preview work, but most of the panel is empty, with little row information or category/filter navigation. |
| Rift Defense | 10% | This is a distinct custom tower-defense mode, not a MyHeroes: SEA screen. Its board, deck and action layout are legible; it should use the shared Echoes visual system, not pretend to be a MyHeroes screen. |
| Pause, results, revive and reward recovery | 12% | The code has functional state-specific actions, but the captured result/revive pages are incomplete and visually bare. Current pass did not capture every branch, so these ratings are provisional. |
| Connection lost / startup unavailable | 10% | Retry/reconnect is present and avoids a dangerous sign-out action. The error card is generic and does not match the reference's surrounding visual language. |

### Town art iteration update — revision 81

The table above preserves the revision 80 audit baseline. Revision 81 adds an original detailed pixel-art guild hall, relocates the specialists to side counters, and places the campaign/raid/trial exits around the room edge. On the same qualitative scale, the town is now provisionally **42/100**: the room composition, vivid floor and counters are closer to the crowded fantasy-lobby feel, but the 2D hero/NPC sprites, game-specific activity rail, icons, minimap and overall UI skin remain visibly different. Whole-game score remains 35/100 until all pages receive a comparable re-review. The 1280x720 implementation capture is `Logs/TownArtIteration/rift-haven.png`; town/campaign interactions and scrolling passed in `Logs/TownArtIteration/runtime-smoke.txt`.

### Item inspection iteration update — revision 82

The inspector now uses a centered raised card, rarity/enhancement ribbon, separated item icon and name, supported gear stats, a dedicated effect/comparison inset, an upgrade footer and an action rail inside the frame. This is a provisional **54/100** for item inspection: its hierarchy, color treatment and action grouping better reflect the observed reference, while the original icon/frame art, narrower amount of supported item data and remaining density differences keep it well below the 90% target. The four-resolution layout suite passed 350 checks with no failures/runtime errors. The reviewed 1280x720 capture is `Logs/ItemInspectorIteration/item-inspector-1280x720.png`; this iteration did not rescore the whole game.

### Inventory identity iteration update — revision 83

Collection and paperdoll borders now take their color from actual item rarity, with enhancement adding a bounded number of perimeter particles. Particle images are created only for tiers that display them. Cell metadata separates rarity marks from enhancement and owned count, and selected item names use the rarity color. The four-resolution suite again passed 350 checks without layout/runtime errors; the reviewed backpack capture is `Logs/InventoryArtIteration/inventory-1280x720.png`. The backpack score is provisionally 62/100; the whole-game score remains unrescored and is not accepted at the requested 90%.

### Town navigation iteration update — revision 84

The oversized rail is now a narrow, labeled vertical menu with Campaign as its highlighted action and Events/Inbox as secondary actions. The large centered town title was removed from over the guild banner, and the movement hint moved away from the campaign gate. All four landscape layout sizes and the town interactions passed in the 350-check player suite; see `Logs/TownNavigationIteration/town-1280x720.png`. Town is provisionally **46/100** on the same qualitative scale. NPC density, missing quest/social screens and art mismatch remain substantial gaps; the whole-game baseline was not rescored.

### Combat HUD iteration update — revision 85

The centered stage banner has been removed from the battle viewport. Stage number/objective remain in the left objective panel, boss status stays top-center, and room progress stays in the right minimap. The clear center better matches the reference's unobstructed combat framing. Four landscape sizes passed the 350-check UI suite, including boss-health and minimap checks; see `Logs/CombatHUDIteration/combat-hud-1280x720.png`. The combat rating is provisionally 58/100; the whole-game score remains unrescored and below the 90% target.

### Important reference surfaces still missing or materially incomplete

The reference material establishes a lobby quest entry with Adventure/Diary/Goals tabs and reward milestones; a multi-tab Friends page; a Guild entry and guild-specific activities; richer profile/avatar/archive pages; dedicated storage/material/transmute/soul-card pages; saved-profile/loadout management; and a shop with distinct offer sections and preview/reward states. Echoes has not demonstrated equivalent complete pages and interaction flows for these surfaces. Do not count an icon, placeholder, server model or drawer label as a finished page.

MyHeroes references also show compact player identity and wallet placement, a busy but readable hub, paperdoll beside a dense item grid, rarity-framed item icons, a raised item-detail view, and combat HUD controls anchored around the screen edges. Echoes currently approximates some of those placements; its sprite set, buttons, panels, font treatment, content density and screen-to-screen consistency remain visibly different.

### Work order to close the gap

1. Create a single art/UI kit: original detailed chibi character set, original item/skill icons, icon silhouettes and rarity frame tiers, beveled dark panels, readable compact pixel typography, wallet chips, tabs and red close controls. Match reference *density and conventions* while retaining original assets and signature shapes.
2. Rebuild the town as a navigable, populated lobby with a clear left activity rail, right utility rail, compact top identity/wallet and clickable character/NPC/portal destinations. Remove floating labels that collide with the world.
3. Recompose combat around clear corner anchors: top-left identity/vitals, top boss bar, left objective/kill info, right minimap/utility, bottom-left mobile movement and bottom-right circular skills/attack. Test touch and keyboard separately.
4. Refine backpack, equipped slots and skill loadout. Increase paperdoll prominence, improve grid density and selection states, and keep inspection actions in the dedicated right rail while adding only supported stats/effects.
5. Build the missing quest, profile, social and shop page flows as real navigable pages, then restyle events, inbox, settings, rankings and results with the same kit.
6. Capture all states at 1280x720 and mobile landscape, verify interaction/return paths, and rescore against the same rubric. Only report progress when evidence supports it.

### Evidence and confidence for this pass

Visually reviewed current layout captures: `Logs/UILayoutBuild/town-1280x720.png`, `combat-hud-1280x720.png`, `inventory-1280x720.png`, `item-inspector-1280x720.png`, `merchant-1280x720.png`, and `settings-1280x720.png`. Also reviewed available title, registration, settings/accounts, rankings, connection-loss, Rift Defense and combat captures in `Logs/Dedicated/Register`, `Logs/ConnectionFailure`, `Logs/RiftDefensePreview`, and `Logs/RuntimeRevision78Release`. The layout suite reports 350 checks passed, 0 layout failures and 0 runtime errors (`Logs/UILayoutBuild/runtime-smoke.txt`). Some secondary screenshots come from the previous release capture set, so their pixel appearance may lag revision 79. Event/inbox were source-reviewed, not separately screen-captured during this pass; results/revive variants remain unverified visually. Confidence is high for the six current layout captures, moderate for other previously captured surfaces and low for uncaptured states.

The accessible [Quests UI wiki](https://myheroesofficial.fandom.com/wiki/Quests_%28UI%29) establishes the Adventure, Diary and Goals tabs; [Friends UI](https://myheroesofficial.fandom.com/wiki/Friends_%28UI%29) documents friends, requests, search, recent co-ops and friend shops; [Guilds UI](https://myheroesofficial.fandom.com/wiki/Guilds_%28UI%29) describes the city-side guild entry and guild activities. The official [My Heroes: Dungeon Raid site](https://heroes.r2game.com/) describes the game as a classic pixel-style barrage RPG. These sources support high-level layout observations, not access to proprietary source art or every regional/version-specific screen.

28 September 2026. Implementation checklist for master-plan revision 30.

Revision 30 note: item presentation now uses a fixed five-star rarity row plus a separate enhancement number, keeping the compact comparison language used by the reference collection screens without making stars represent an unbounded level.

## Evidence and limits

Two passes: first inventory the visible structure; second revisit the relevant frames and compare the hierarchy, labels and interaction states against the supplied screenshots. This is an audit of **observed screens**, not a claim to have enumerated every screen in the commercial game. Still images establish appearance, not hidden gestures, server rules or navigation history. SEA and Dungeon Raid footage can differ by version and progression.

Primary video: [PROAPK, My Heroes: SEA Gameplay Android / iOS](https://www.youtube.com/watch?v=9k_rK9_oSio). Verified paused samples: 1:45 shop reward preview, 1:55 equipped-item popup, 2:10 saved profiles, 2:30 matchmaking confirmation. Combat was observed around 6:25. The 1:45, 1:55 and 2:10 frames were revisited in pass two; rapid seeks with stale decoded frames were excluded from evidence. Matchmaking remains a single-pass observation. Earlier footage shows profile rename with the native text keyboard. The broader five-video source register remains in [MYHEROES-GAMEPLAY-REFERENCES.md](MYHEROES-GAMEPLAY-REFERENCES.md); this audit does not claim five complete video viewings.

Second video: [Maximumandroid, My Heroes: Dungeon Raid gameplay](https://www.youtube.com/watch?v=gLHw_H5kPi8), paused at 1:04.64 and visually revisited for pass two. It confirms the same corner-based combat hierarchy, objective list, minimap, overhead vitals, circular cooldowns and bottom progression strip. It also shows level 10/15 locks instead of the SEA screenshots' 5/10 locks, and stamina/time objectives instead of the SEA kill meter. These version differences must not be silently combined into a supposedly exact UI specification.

Wiki read and cross-checked twice: [Quests UI](https://myheroesofficial.fandom.com/wiki/Quests_(UI)) and [Skills information](https://myheroesofficial.fandom.com/wiki/Skills_(Information)). Quest documentation establishes the Adventure, Diary and Goals tabs and daily milestone chests at 4/8/12 completions. Skill documentation distinguishes learning/ascending books from leveling resources, universal/class skills and rarity. The skills page contains unfinished sections. Backpack, profile, shop, storage, friends and guild navigation targets could not be retrieved through the research tool; their behavior is not inferred from link names.

Screenshot sets supplied by the user: town/combat/skills/shop/avatar set; character/inventory/item set; fourteen-image dark curved UI set. These remain visual evidence even where the wiki is unavailable.

## Element and interaction inventory

| ID / surface | Observed elements and arrangement | Interaction evidence / uncertainty | Development requirement |
|---|---|---|---|
| H1 town identity | Portrait, name ribbon, level/rank at upper left; compact health below portrait | Profile/rename overlay appears in footage; exact portrait hit region not established | Keep identity compact; profile entry must be explicit |
| H2 wallet | Small currency icons, right-aligned values and square plus buttons at upper right | Shop shown separately; destination of each plus not established | Gold/gems icon counters; add plus only with a real destination |
| H3 utilities | Mail, settings, events, gifts/chests, red notification diamonds around upper/right edges | Icons/badges visible; claim conditions unknown | Only signal actionable unread/claimable state; no decorative false badges |
| H4 navigation | Quest/diary/friends/guild rail left; backpack and extra utility buttons right | Backpack opens full overlay in video | Maintain clear center and consistent entry/close placement |
| H5 world | Compact characters, overhead HP, speech/service bubbles, nearby names, portal marker | Movement across scrolling town; interaction range not measurable | Preserve camera movement and proximity interaction; improve authored world separately |
| H6 movement | Large translucent joystick lower left; lower-right attack and circular skills; small dodge/utility | Aim indicator/cancel shown in tutorial | Preserve independent pointer ownership and clamped touch sticks |
| H7 progression | Thin bottom XP strip, chat/system strip, compact objective prompt | Static/progress states visible | Avoid a large instruction banner covering combat |
| B1 backpack shell | Narrow icon sidebar; separate framed paperdoll and dense five-column grid; red X at top right | Backpack and Profiles states share left paperdoll | Two framed columns; backdrop blocks world; Back restores prior layer |
| B2 paperdoll | Hero centered on purple-lit stone alcove; six gear slots around hero; name/level/guild below; weapons and talent below | Equipped boot visible behind popup; stat totals change | Larger live preview and clickable real equipped slots; six-slot migration remains open |
| B3 inventory cell | Dark recessed square, colored rarity rim, centered art, tier/count, elemental badge, equipped indicator | Selected cell/popup and equipped toast visible | Stable icon core; border-only tier particles; readable count/selection |
| B4 filters | Weapon/Gear tabs; class, quality, part filters; capacity counter | Selection menu contents not all observed | Visible category choices and explicit sort options; retain selection/page on popup return |
| B5 side tabs | Backpack, Storage, Material, Transmute, Soul Card, Profiles (varies by reference) | Profiles screen verified at 2:10; other transactions not verified | Implement real destinations incrementally; no inert imitation tabs |
| B6 saved profiles | My Profiles/Recommended Profile tabs, Save Profile, numbered rows, rename/equip icons, locked purchase row | 2:10 paused frame verified; save/apply outcome not sampled | Future authoritative loadout presets; no fake purchase unlocks |
| I1 item popup | Background inventory dimmed, independent raised lavender bevel; rarity ribbon, colored name, large icon; primary stat prominent | 1:55 equip toast and retained backpack verified twice | Floating inspection layer with item selection retained underneath |
| I2 item detail | Tier/enhancement, type/class, damage range/DPS/attack rate, colored stat bonuses, synergy text, lore/source, optional stars/sockets | Different screenshots show conditional sections | Show only supported real stats; distinguish changes vs equipped; no invented DPS |
| I3 item actions | Equip/Unload, Storage, Lock vertical buttons beside popup; price at footer; arrows for adjacent inspection in video | Equip outcome visible; lock/storage outcomes not verified | Equip/upgrade/story actions and explicit close; storage/lock separate future implementation |
| S1 skills | Four large equipped slots left, level/unlock labels; dense collection grid right; filters, wallet, red close | User screenshots establish layout; all drag/assignment gestures unknown | Dedicated loadout screen after four-slot migration; preserve current two three-skill stances meanwhile |
| S2 skill cell | Element corner icon, quantity/level, name, rarity, equipped overlay, stars/upgrade arrows | Static screenshots and wiki concepts agree | Use real book ownership/assignment; avoid misleading upgrade arrows |
| Q1 quests | Adventure/Diary/Goals side tabs; progress header/chests; rows with icon, objective count/bar, reward icons and Go | Wiki establishes tabs and milestones; supplied quest screenshot corroborates | Server-backed objectives/claims, explicit claimed/available state; no copied reward values |
| P1 profile | My Info/Archive sidebar; Avatar/Frame tabs; Owned/Not Owned sections, selected border, preview, permanent/In Use | Avatar screenshot; rename keyboard visible in video | Cosmetic preview before apply; ownership from server |
| P2 rename | Modal input, free-change message, native keyboard, Done/Cancel | Observed in early video sequence; pricing afterward unknown | Keep account controls under Settings > Accounts; sign-out requires confirmation |
| M1 shop shell | Best Buys/D.Shop/Restock top tabs, categories left, large cards right; wallet/X above | 1:45 verified twice | Separate merchant browsing from equipped-item presentation |
| M2 product | Name, stock counter, large art, discount band, previous/current price, currency icon | Screenshot/video consistent; pricing legality/history not established | Real gold price and owned count; no invented discount/stock timers |
| M3 purchase preview | Darkened shop behind centered contents preview, reward icons/counts, prominent action | Free F-Rank Kit preview at 1:45 | Inspect before Buy; preserve selected offer and show authoritative result |
| M4 reward reveal | Item lineup, NEW tags, colorful item highlights, Draw x10/Back | Screenshot only; skip/reveal timing unknown | Future receipt-safe rewards; no random-store mechanics inferred |
| C1 combat status | Compact portrait upper left; timer and kill meter left; minimap upper right with elite markers; boss name/HP/multiplier top | Tutorial and combat samples; supplied boss screenshots | Existing named boss HP and map retained; maze topology remains open |
| C2 combat feedback | Overhead vitals, damage numbers, circular cooldown overlays, translucent controls, bright effects against dark world | Video + screenshots | Budget pooled effects; readability during burst damage; physical device acceptance open |
| R1 matchmaking | Layer selection, player slots, search/cancel, dimmed confirmation with portraits, time-left bar, Leave/Enter | 2:30 frame shows refusal message/confirmation | Future readiness flow; current two-player prototype is not 4/8-player completion |
| U1 unknown flows | Guild management, friends details, mail claims, full settings, commerce receipts, reconnect errors | Not established by reviewed evidence | Research/test these separately; do not claim parity |

## Pass-two conclusions and acceptance

The structural mismatch is larger than the palette mismatch: the old game made item descriptions a permanent left column and reduced the hero to a small preview. The reference prioritizes a paperdoll/grid browsing surface, then adds a separate detail layer. The reference also uses stronger double bevels, a restrained purple/charcoal palette, distinct red close controls and brightly colored item rims. Product cards and saved-profile rows are different layouts, not variations of an inventory list.

Revision 29 implementation order: (1) backpack paperdoll/grid shell and explicit categories; (2) independent inspection layer and back/focus ownership; (3) merchant product cards with real prices; (4) four-size desktop/touch layout and transaction regression. Subsequent work: dedicated skill assignment, six-slot/four-skill data migration, quest/profile screens, authored town scenery and the unverified social flows. Our two-currency economy, server-authoritative grants, existing account protection and campaign rules take precedence over the reference game's mechanics.

Completion requires build/tests and visual capture review. Local layout success does not establish Android performance or complete MyHeroes parity.

## Revision 29 implementation result

Completed the current-contract presentation slice for B1/B3/B4, I1/I3 and M1/M2/M3: paperdoll and grid, explicit categories/sort choices, separate inspector with nested Back, equipped gear/starter-skill inspection and full-width three-card merchant browsing. Real tier, ownership and gear bonuses are shown; no fictional DPS, sockets, prices or stock timers were added. Read-only starter spells do not expose purchase/upgrade actions. Six-slot gear and four-active-skill parity are still open; the visible six-skill strip describes our existing Moon/Ember stances.

Final build `Logs/step-10-20260927-210718.log`. Desktop 342 / touch-layout 343 checks PASS, zero failures/errors. Real popup Buy/Equip/Upgrade tested using isolated saves; native captures at four sizes. Solo campaign and dedicated registration/resume regressions PASS. Screenshot review fixed a merchant/footer overlap and full-screen shade coverage that text-overflow checks alone did not detect. The existing world-camera development warning at 1024px width is still visible; physical mobile performance is not validated by these fixtures.

Next: six-slot/four-skill migration and a dedicated skill assignment page; then server-backed quest/profile flows and the remaining unverified interactions. The complete game's UI has not been exhaustively verified or recreated.
