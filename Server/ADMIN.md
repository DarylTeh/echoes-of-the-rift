# Local administrator API

Open http://localhost:8083/allah/api/hehe/v69/docs/ and click **Authorize**. Username: `DarsieJiaoTeh`; use the configured password. The password is not recorded in documentation or source. Only a salted scrypt hash is saved in ignored `Server/admin-auth.json`.

Double-click **Open Rift Admin.cmd** in the parent game folder to start/reopen it. Alternatively run `Tools/Start-Admin.ps1`. Node dependencies can be restored with `npm ci --prefix Server --ignore-scripts`. The service uses `Server/progress.sqlite`; it does not require the game server to be running. Reconnect the game after editing a player.

## Workflow

1. Query accounts or players (search by username/player ID).
2. Read `/api/players/{id}` and copy `profile.AdminRevision`.
3. Select giveGold, setGold, giveGems, setGems, setLevel, giveItem, setItem or deleteItem.
4. Supply a unique requestId, a meaningful reason and expectedRevision. Use catalogue IDs from `/api/catalog` for items.
5. Execute, inspect the updated profile, then query `/api/audit` if needed.

All routes shown by Swagger are relative to `/allah/api/hehe/v69/docs`. Example: `/allah/api/hehe/v69/docs/api/players`. Old `/docs` and unprefixed API routes return 404. Existing game/account service URLs are unchanged.

Retry an uncertain request with the same requestId and identical body: it will not grant twice. A new edit needs a new requestId. A 409 revision conflict requires a fresh profile read; do not blindly overwrite newer gameplay rewards. Setting currency to zero clears it. setItem creates/replaces a stack; quantity zero or deleteItem removes it. Removing the last equipped copy requires explicit `unequip: true`.

## Event calendar

The **Events** group in Swagger is the live-ops calendar. `GET /api/events` shows each current manifest and its server-computed state. Use `POST /api/events` to publish a new manifest version with a future UTC `startAt` and `endAt`, a reusable `type`, configurable `config`, and a `rewards` array. To change an event, publish a higher `version`; active versions are immutable. `POST /api/events/{id}/disable` creates a disabled higher version as a kill switch. The game reads active events and the authoritative timer from the game service `POST /events`; the client does not trust its device clock.

For a real player-facing activity shortcut, set `config.destination` to `campaign` and include `campaign` in `config.eligibleModes`. The server rejects other destinations or a destination that cannot earn event progress. The game then displays **Play Campaign** in that event's details. Leave destination unset for event types without a supported direct route; `rift_defense`, event shops and other modes do not have a launch route yet.

Before publishing, use `POST /api/events/preview` with `{ "manifest": { ... }, "previewAt": "2026-10-03T12:00:00.000Z" }`. The time is optional and must be UTC with a trailing `Z`. Preview validates and normalizes the manifest, reports whether it would be scheduled/active/ended/disabled at that time, shows the next schedule boundary and current published version, and makes no database changes. A preview version must be higher than the current version for that event.

For a recurring event, copy the appropriate template from `Server/events/event-templates.mjs`, change the IDs, dates, text, catalog references and reward limits, validate it in staging, then publish it from Swagger. Keep reward delivery in the server transaction/inbox path; a manifest by itself never grants client-requested rewards.

## Limits and scope

Gold is the stored `Coins` field. Gold/gems support 0–2,000,000,000; character level 1–1,000,000; inventory stacks 0–1,000,000 and the legacy catalog tiers 1–5. Server profile reads now add missing progression fields while preserving those legacy tiers and balances. The client presents the catalog tier as the current enhancement fallback; production uncapped upgrades still require separate-stack transaction validation in [the progression model](../Docs/PROGRESSION-MODEL.md). Character level is currently metadata: it does not unlock campaign stages or modify combat stats. Gem earning/spending, infinite upgrade transactions and live online-profile refresh remain separate roadmap work. Catalogue definitions and account credentials are not editable here. Queries never return player secrets or password hashes.

## Access and operations

The listener binds only to 127.0.0.1, with username/password authentication, a failed-login limit, strict Host/Origin checks and atomic audited edits. Swagger assets are local. Credentials are not persisted by Swagger across reloads. The custom path is organization/obscurity, not a security boundary. Do not publicly forward port 8083 or reverse proxy it as-is; public administration needs HTTPS and restricted operator access (VPN/access gateway). Public game deployment does not require exposing this service.

Run `node Server/backup.mjs` for a consistent SQLite backup before bulk corrections. Logs and the launched process ID are under `Logs/Admin`. To reset credentials, supply temporary `RIFT_ADMIN_USERNAME` / `RIFT_ADMIN_PASSWORD` environment variables to `node Server/admin-auth.mjs`, clear those variables, then restart only the admin process. Keep credentials, SQLite files and backups out of public client packages and source control.
