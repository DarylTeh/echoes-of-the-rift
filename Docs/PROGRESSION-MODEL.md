# Echoes of the Rift progression model

Revision 30, 28 September 2026.

## Display decision

The inventory will show two different signals: a filled/empty five-star row for fixed item rarity, then a `+number` for enhancement. A cell therefore reads, for example, `***..  +12  x4` (asterisks are the font-safe pixel star glyph). The stars are not the level. This avoids the common readability problem where a star count is asked to carry both rarity and duplicate investment.

This follows three useful patterns from the comparison set:

- Clash of Critters uses stars as a visible duplicate-driven rank and groups stars into named bands. That makes a duplicate payoff obvious, but a star-only number becomes unwieldy at high rank. [Clash star progression reference](https://clashofcritters.wiki.gg/wiki/Critters)
- AFK Journey separates equipment quality/rarity from equipment level, ties forge availability to progression, and uses `EX+1` through `EX+25` for a late exclusive track. That keeps a large number readable and lets late levels unlock effects rather than only multiply damage. [AFK Journey equipment reference](https://www.prydwen.gg/afk-journey/guides/beginner-guide)
- IdleOn uses books to raise a skill's maximum level, then assigns points normally; library speed and max-level boosts create long-term goals without requiring every player to grind the same number immediately. [IdleOn Talent Book Library reference](https://www.digitaltq.com/wiki/idleon/talent-book-library)

The catalog `Tier` field remains item identity. The server now normalizes old profiles by adding `EnhancementLevel`, `SkillLevels`, `EquippedEnhancementLevels` and `ProgressionVersion` without changing currencies, counts or `AdminRevision`. The Unity client reads those fields when present and falls back to the legacy tier for older local saves. The actual uncapped enhancement transaction still remains gated until the client supports multiple enhancement-level stacks per catalog item.

## Gear

For an item at enhancement level `L`, the next upgrade consumes:

`duplicateCost(L) = 5 × (1 + floor(L / 100))`

`goldCost(L) = 50 + 8L + 2 × floor(L / 10)^2`

This means levels 1–99 cost 5 duplicates, 100–199 cost 10, 200–299 cost 15, and so on. The first upgrade is cheap enough to teach the loop; the 100-level boundaries are visible long-term goals. Gold remains the common sink and scales more smoothly than the duplicate requirement.

Gear power uses a soft cap instead of an unbounded linear multiplier:

`gearMultiplier(L, rarity) = [1 + 1.8 × (1 − e^(−L/140))] × [1 + 0.10 × rarityIndex]`

At level 100, the level component is about 1.92×; at 200 it is 2.37×; at 500 it is 2.75× and approaches 2.8×. This lets a late player feel stronger without erasing mechanics, build choices or boss phases.

| Gear level | Duplicate cost | Gold cost | Level multiplier |
|---:|---:|---:|---:|
| 1 | 5 | 58 | 1.01× |
| 5 | 5 | 90 | 1.06× |
| 10 | 5 | 132 | 1.12× |
| 50 | 5 | 500 | 1.54× |
| 99 | 5 | 1,004 | 1.91× |
| 100 | 10 | 1,050 | 1.92× |
| 200 | 15 | 2,450 | 2.37× |
| 500 | 30 | 9,050 | 2.75× |

## Skill books

For a skill at level `L`, the next level consumes:

`bookCopies(L) = 1 + floor((L − 1) / 10)`

`gemCost(L) = 5 + 2 × floor((L − 1) / 10) + 5 × floor(L / 25)`

The first ten levels require one copy and five gems. Levels 11–20 require two copies and seven gems. Level 21 starts three copies; level 25 adds the first gem milestone. Level 100 costs ten copies and 43 gems. Skill power uses a gentler soft cap:

`skillMultiplier(L) = 1 + 1.5 × (1 − e^(−L/40))`

The skill track therefore remains meaningful but cannot turn every encounter into a one-button clear. Gems remain skill-book-only, as requested.

| Skill level | Copies | Gems | Skill multiplier |
|---:|---:|---:|---:|
| 1 | 1 | 5 | 1.04× |
| 10 | 1 | 5 | 1.33× |
| 11 | 2 | 7 | 1.36× |
| 20 | 2 | 7 | 1.59× |
| 21 | 3 | 9 | 1.61× |
| 25 | 3 | 14 | 1.70× |
| 50 | 5 | 23 | 2.07× |
| 100 | 10 | 43 | 2.38× |

## Campaign pressure and new-player carry

Campaign target power is `round(120 × 1.12^(chapter − 1))`: 120 at chapter 1, 189 at chapter 5, 333 at chapter 10, 1,034 at chapter 20, 3,210 at chapter 30 and 30,965 at chapter 50. The server should tune actual enemy health/damage around these targets after playtest telemetry, not expose drop-rate formulas to clients.

| Chapter | Target power |
|---:|---:|
| 1 | 120 |
| 5 | 189 |
| 10 | 333 |
| 20 | 1,034 |
| 30 | 3,210 |
| 50 | 30,965 |

New players are carried through guaranteed structure, not permanent hidden damage reduction: start the first usable gear set at `+5`, guarantee at least one gear duplicate in each of the first five chapter clears, grant a free starter skill at level 3, and keep the first five chapters' gear drops weighted toward usable slots. Do not inflate gems or rare skill-book drops. The carry flag ends at chapter 5 and is server-owned.

Late-game plateau: after +200, gear still consumes duplicates and gold but its power curve is close to its ceiling. New chapters should add mechanics, resistances, boss phases and cosmetic milestones rather than only larger health pools. A player who has reached the plateau should still need correct skills, movement and equipment synergies to clear a new campaign.

## Server and migration requirements

The server-side normalization is implemented in `Server/progression-rules.mjs` and runs when profiles are read or saved. Keep the old rarity/tier field for catalog identity. The remaining production step is an authoritative uncapped enhancement transaction with separate stacks, server-owned reward rolls and audited cost validation. During migration, old tier 1–5 stacks map to enhancement levels 1–5 with no power rewrite.
