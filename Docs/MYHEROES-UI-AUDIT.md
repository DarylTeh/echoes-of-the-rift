# MyHeroes: SEA UI reference audit

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
