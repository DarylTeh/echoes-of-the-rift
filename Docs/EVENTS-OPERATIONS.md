# Live events and production operations

Revision 38, 29 September 2026.

The event system is server-driven. Every event is an immutable, versioned manifest with UTC `startAt` and `endAt`, a reusable `type`, a configurable `config` object, reward definitions and an explicit disabled/kill-switch state. Clients receive the server timestamp and active manifests from `POST /events`; they render countdowns and descriptions but never decide eligibility or progress from the phone or PC clock.

## Patterns to reuse

Clash of Critters uses rotating secondary events that can overlap, including Island Gold Rush, Fishing Contest, Treasure Hunt, Marathon Party, Marathon Star, Zobo Shooter, Cozy Farm, Zobo Buster and Carnival Rush. Its Boss Challenge resets daily, uses a best-level/damage leaderboard and can give bosses special patterns. Daily quests refresh from a randomized table without duplicate goals, while achievements progressively increase and eventually cap. [Events](https://clashofcritters.wiki.gg/wiki/Events), [Boss Challenge](https://www.clashofcritters.net/gameplay/boss-challenge), [Quests](https://clashofcritters.wiki.gg/wiki/Quests)

Brave Frontier combines limited Vortex/event dungeons, login campaigns, resource boosts such as double experience or half energy, recurring Vortex Arena seasons, Frontier Hunter, Raid Battles, Grand Quest, Challenge Arena, mini-games and Event Bazaars. Collaboration runs add a story or special dungeons, exclusive characters/equipment, login rewards, token exchange shops and community/share milestones; examples include Rune Story, Trigun, Final Fantasy Brave Exvius, SMT IV: Apocalypse and Fairy Tail. [Past Special Events](https://bravefrontierglobal.fandom.com/wiki/Past_Special_Events), [Trigun Event Bazaar](https://bravefrontierglobal.fandom.com/wiki/Event_Bazaar/Trigun_Collaboration), [Vortex Arena](https://bravefrontierglobal.fandom.com/wiki/Vortex_Arena)

## Reusable modules

| Module | Reusable behavior | Typical configuration |
|---|---|---|
| Login calendar | One claim per UTC day, catch-up policy, final-day reward | days, rewards, requiresOptIn |
| Token exchange / Event Bazaar | Earn event token from eligible modes, spend in rotating shop | tokenId, eligibleModes, daily cap, shopId |
| Daily quests | Rotating goals with no duplicates, daily reset, reroll policy | goal pool, count, reset hour, rewards |
| Progressive achievements | Repeating milestones that grow until a cap | metric, thresholds, reward table, cap |
| Boss Challenge | Attempts, boss phases, best score, season leaderboard | bossId, attempts, score rule, ladder |
| Raid ladder | Co-op clears, personal and guild milestones, ranked rewards | raidId, tiers, contribution rule |
| Milestone/community goal | Server-wide verified counter unlocks shared rewards | metric, milestones, claim window |
| Event shop | Stock, prices, purchase limits, restock windows | shopId, currency, stock, per-player limits |
| Double-drop / energy modifier | Temporary server multiplier with mode and item allowlists | multiplier, modes, item filters |
| Collaboration pack | Story, login calendar, missions, shop, cosmetics, rerun flag | content IDs, legal end date, rerunnable |
| Tower Defense season | Five-book deck, seeded board, waves, mutations, boss and leaderboard | mapId, deck size, board size, waves, seed policy, attempt cap |
| News/inbox | Announcement, reward mail, expiry, read state and receipt | audience, locale, expiry, payload |

The repository includes reusable templates for login calendars, token exchanges, daily quests, progressive achievements, Boss Challenges, raid ladders, event shops, drop modifiers, community goals, collaboration packs, news/inbox messages and Tower Defense seasons in `Server/events/event-templates.mjs`. `Server/events/event-store.mjs` persists current manifests, historical versions and idempotent claim receipts, and exposes version-pinned status for a player. Reward-bearing claims enqueue bounded currency/item payloads into the SQLite inbox in the same transaction; the private persistence routes `/event-status`, `/inbox`, `/inbox-claim` and `/event-claim` provide status, recovery and idempotent collection. `Assets/_Project/Scripts/Events/EventCountdown.cs`, `EventFeedClient.cs`, `EventStatusClient.cs` and `InboxClient.cs` transport server-supplied event/reward data to the client; `EventDrawerUI` presents descriptions and authoritative status without faking gameplay progress.

## Operating cadence

- Daily: quests, login claims and Boss Challenge attempt reset.
- Weekly: one rotating activity or Event Bazaar restock; overlap with the daily systems.
- Monthly: a festival with token exchange, milestone track, cosmetics and a leaderboard season.
- Quarterly: a collaboration or story pack only after rights, localization, asset expiry and rerun rules are approved.
- Permanent: achievements, player profile cosmetics and a returning-player catch-up track.

## Publish and rollback workflow

1. Create a new manifest version in draft and validate IDs, UTC timestamps, reward bounds, locale coverage and referenced catalog assets.
2. Simulate a full run in staging with accelerated time, duplicate claims, reconnects, clock skew, rollback and reward-inbox recovery.
3. Publish the manifest with a future start time. Never edit an active version in place; create a new version or disable it with the kill switch.
4. Monitor active players, claim failures, token creation/spending, reward delivery latency, duplicate-receipt rejects, economy inflation and leaderboard anomalies.
5. End the event by schedule, freeze the leaderboard, deliver pending rewards through the inbox and retain the manifest/claims for audit. Archive content only after the legal and rerun retention window.

## Production safeguards

- UTC server time is authoritative; clients show the server offset and countdown only.
- Every claim needs `(eventId, eventVersion, playerId, claimKey)` idempotency and an auditable reward receipt.
- Reward grants, token spends and leaderboard scores must be server-side and transactional. Client-submitted progress is a request, never proof.
- Use feature flags and a kill switch for each event module, with a dry-run mode that records intended rewards without granting them.
- Put hard caps on token earning, shop purchases, daily attempts, community-goal contribution and leaderboard score rate.
- Keep event content separate from the base catalog so a rerun cannot mutate historical item definitions or old receipts.
- Require legal approval and explicit sunset/rerun policy for collaboration names, art, music, voice and promotional rewards.
- Localize dates, titles, quest text and reward descriptions; store timestamps as UTC and render in the player's locale.
- Back up the database before publishing a reward-bearing event and test restore before the first live run.

The local Swagger event calendar is now available at `GET/POST /api/events` plus `POST /api/events/{id}/disable`; it validates manifests, keeps prior versions and supports a kill switch. The game service now exposes version-pinned `POST /event-status` for player state, eligibility and server-recorded claim receipts. The client Events drawer includes a server-described detail/status view, and the recoverable Reward Inbox is implemented. The next implementation slice is module-owned gameplay progress and claim eligibility, with the separate-stack enhancement transaction still tracked independently.
