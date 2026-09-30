# UI safety and presentation audit

Revision 34, 28 September 2026.

This audit covers the current runtime UI, town mode routing, collection/shop screens, combat HUD and the approved pixel-art direction. It is a guardrail for future additions so a new screen cannot quietly reintroduce the account and navigation problems that were removed.

## Decisions that are now fixed

- Sign out exists only under **Settings > Accounts**. It requires a second confirmation, and **Stay in game** is the default selection. Connection-loss screens expose **Reconnect** only; they never expose account actions.
- There is no reduced-motion setting, preference read, or hidden animation-off path. Neon border ornament, world trails and skill effects remain active. Performance is controlled by fixed pools, off-screen culling and refresh budgets instead of a user-facing kill switch.
- Destructive or session-changing actions use a separate confirmation view. Background movement, inventory and combat controls are blocked while a modal owns input. Closing a nested confirmation unwinds to its parent before closing the parent.
- Mode signs and future event entries must open a clearly labelled gate or preview when a mode is unavailable. They must not silently start another mode or spend currency.

## Current implementation review

`GameUI` provides the shared dark, curved, pixel-aligned frame language, safe-area fitting, selected-state colours and keyboard/gamepad focus. `TownHubManager` owns the town HUD and settings overlay. `InventoryModal` owns collection, inspection and shop navigation. `SkillWheelHUD` keeps combat actions in a stable radial layout. `PauseManager` exposes Resume only, so pausing cannot accidentally leave a run.

The town currently routes Campaign, Raids, DPS Trial and World Boss through explicit interaction labels. Campaign and the co-op test are playable; the other destinations are development gates. Rift Defense is specified as an optional skill-book tower mode in [RIFT-DICE-DEFENSE.md](RIFT-DICE-DEFENSE.md), with server validation planned before it becomes selectable.

The approved art direction remains the compact large-head/small-body pixel silhouette, dark violet curved surfaces, stable item icons and tier-only neon border motion. Remaining art work is directional hero animation, differentiated boss/environment sheets and full gear-family coverage; those are content tasks, not reasons to add more UI chrome.

## Regression gates for every new screen or mode

1. The screen has one clear owner for input, a visible close/back route and a safe default focus.
2. Account, economy and other destructive actions are absent from gameplay HUDs and connection-error states.
3. Every label is UTF-8 clean, fits at the supported safe area and uses the shared pixel typography and curved frame components.
4. Animations are bounded and pooled. Item icons and combat warnings stay readable while border effects and high-tier trails provide the visual flourish.
5. The desktop and touch UI smoke suites pass with zero layout failures and zero runtime errors. New live modes also need deterministic server validation and a reconnect/reward-recovery check.

## Next presentation slice

The town now has the first safe drawer pattern through the server-timed Events panel. The next safe visual improvements are richer directional combat frames and authored boss/environment sheets. Keep the settings/account boundary, fixed neon motion policy and explicit mode gates unchanged while those assets are added.
