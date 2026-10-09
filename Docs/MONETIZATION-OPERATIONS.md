# Monetization modules and rollout plan

Status: design draft, no live store or ad SDK configured. Updated 9 October 2026.

## Player-facing shape

Keep the current MyHeroes-inspired Shop hierarchy. Use **D.Shop** for direct gem packs, **Earn Gems** for voluntary rewarded ads, and keep **Restock** truthful and driven by a server schedule only when one exists. Gear cards continue to use gold and their existing inspect-before-buy flow. Store packs must show the exact gem grant and the platform's localized price. Do not show a crossed-out price, countdown, remaining-stock label, random reward odds or one-time pressure prompt unless the server has real, auditable data that justifies it.

The initial store catalog should use four configurable **consumable** gem-pack entries: Starter, Small, Medium and Large. Their exact product IDs, amounts and store prices are unset until the Play Console product catalog and the player economy are approved. Prices must come from the connected store, not a hard-coded currency string. Pack rewards are deterministic gems; no gear, skills or randomized loot are bundled into the first release. Gems remain a skill-progression currency under [PROGRESSION-MODEL.md](PROGRESSION-MODEL.md).

The ad surface is opt-in and separate from combat. Start with rewarded video only; do not add banners, forced interstitials or automatic ad playback. Proposed initial offer: 5 gems for a completed, server-verified view, limited to three claims per UTC day. The value and cap remain draft economy values and must be confirmed against ad-network policy and playtest data. If an ad is unavailable, the player keeps their current state and can dismiss the offer without penalty.

## Reusable modules

| Module | Responsibility | Required contract |
|---|---|---|
| Store product catalog | Product ID, consumable type, gem quantity, localized display data and availability | One authoritative catalog; UI never embeds product IDs or prices |
| Store connection | Fetch platform products and localized prices; expose initializing, ready, unavailable and error states | Purchase buttons stay disabled until the store reports a valid product and price |
| Purchase receipt verifier | Validate Apple/Google receipts on the server and map a verified product to its configured reward | Reject unknown products, invalid receipts, wrong bundle/app IDs and duplicate transaction IDs |
| Purchase grant ledger | Atomically record verified transaction IDs and add gems | Idempotent retry; durable receipt record and profile balance update commit together |
| Purchase handoff | Keep platform purchase pending until the server confirms the saved gem grant | Only then confirm/finalize the platform order; deferred or failed payments grant nothing |
| Rewarded placement catalog | Placement ID, exact reward, UTC cap and display eligibility | Server policy owns reward and cap; client callback alone is never proof of a completed ad |
| Ad callback verifier | Accept provider server-to-server completion callbacks and validate their signature/secret | Deduplicate by provider completion ID; atomically grant the configured reward and update the daily cap |
| Reward claim/status API | Return server time, remaining claims and last result | Client cannot supply or raise its reward amount or cap; retry safely after network loss |
| Shop presentation | D.Shop and Earn Gems cards, progress feedback, unavailable/retry states | Match the existing framed dark palette and MyHeroes card hierarchy; clearly distinguish real-money and free-reward actions |
| Operations controls | Enable/disable products and placements, caps, provider credentials, test mode and audit views | Staging and production settings are separate; kill switch stops new grants without deleting history |

## Economy and player-safety rules

- Store prices are localized by Apple/Google. Do not present a fixed USD price as universal.
- Use the existing `Gems` field and progression rules. Purchases add currency; they do not bypass campaign gates or grant hidden combat power.
- A rewarded-ad gem grant and a store purchase use different server ledgers and cannot be confused with admin `giveGems` operations.
- Apply account-level daily limits using UTC server time and atomic database updates. Do not trust the device clock.
- Purchase history and ad completion records contain transaction identifiers and status, not full payment-card data or raw credentials.
- Build a staging mode with platform sandbox products and ad test inventory. Production keys and validation secrets come only from environment/secret storage and are never committed.
- Provide purchase failure, pending/deferred, restore/recovery, ad-unavailable and network-retry states. Never sign the player out or erase local UI state when an external service fails.
- Keep the ad action optional and outside active fights. No ad should interrupt movement, skill casts, a boss warning, an active reward reveal or a multiplayer session.

## Implementation gates

1. Install and resolve Unity IAP 5 through Package Manager. Confirm the resolved package version in `Packages/packages-lock.json`.
2. Create the initial consumable products in the target store and provide product IDs plus exact gem rewards. Store prices will be fetched from the platform.
3. Configure server-side Apple/Google receipt validation credentials and the app/bundle identifiers. Add the idempotent verified-purchase ledger before connecting live purchase buttons.
4. Install and resolve LevelPlay Ads Mediation. Select the rewarded-ad optimization goal, target mobile platform, App Key, rewarded unit ID, privacy/consent requirements and server-to-server callback secret.
5. Implement the provider callback, reward ledger, daily cap and status/claim APIs. Test duplicate callbacks, invalid signatures, device clock changes, offline retry and daily-cap edges.
6. Add D.Shop and Earn Gems cards using localized store prices and server-authoritative ad eligibility. Keep Restock disabled unless backed by a real schedule.
7. Run IAP sandbox tests and the LevelPlay device test suite on Android. Add iOS-specific purchase restore and ad/privacy checks only if iOS is selected. Check Windows independently; mobile ads and store billing are not inferred to work on the Windows build.
8. Roll out to internal testers first. Review purchase verification failures, duplicate callbacks, refund handling, opt-in ad completion, complaints and gem balance changes before broad release.

## Current project findings

- `Packages/manifest.json` and `packages-lock.json` do not contain `com.unity.purchasing` or `com.unity.services.levelplay`.
- No `IAPProductCatalog.json`, IAP implementation, LevelPlay initializer or ad callback integration was found.
- The game already stores `Gems` in the player profile, but the persistence service has no store receipt verifier or consumable-purchase ledger and no provider-verified ad reward route.
- The current Unity CLI reports no signed-in license. Package installation and a Unity build therefore need the Editor/Unity account setup restored before client integration can be verified.
- No store product IDs, store prices, LevelPlay App Key, ad unit ID or server callback credentials are configured. Do not fabricate any of these values or expose a client-only gem grant.
