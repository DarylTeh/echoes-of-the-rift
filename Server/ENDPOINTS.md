# Echoes of the Rift endpoints

This file records the addresses that belong in a fresh clone. It contains no credentials or private keys.

## Local Windows development

| Service | Address | Purpose |
| --- | --- | --- |
| Dedicated game server | `127.0.0.1:7770/UDP` | Unity/FishNet gameplay transport |
| Persistence service | `http://127.0.0.1:8081` | Private server persistence API |
| Account/readiness service | `http://127.0.0.1:8082` | Registration, sign-in, recovery and health |
| Local admin Swagger | `http://localhost:8083/allah/api/hehe/v69/docs/` | Loopback-only operator tools |

The checked-in client uses `127.0.0.1` and account port `8082`. Start the server launcher before the player launcher. The admin service is opened through `Open Rift Admin.cmd` and must remain local.

## Authenticated game-service routes

The private persistence service at port `8081` requires the server bearer key for POST requests. Event Bazaar clients call `POST /event-shop-status` with `{id,eventId,eventVersion}` to read server time/state, balance, offer rewards/prices, remaining stock, per-player limits and eligibility. `POST /event-shop-purchase` uses `{id,eventId,eventVersion,offerId,requestId}`; the unique request ID is reused on retries. The purchase route revalidates all status because other players may change shared stock after a status response.

## OCI deployment status

There is currently **no public IP, DNS name or public gameplay endpoint**. Live OCI CLI checks on 2 October 2026 found only the Singapore home region subscribed (one availability domain), no VM and no reserved public IPv4. The VCN/subnet are available. Capacity reports found Always Free A1 Flex and E2.1.Micro out of capacity; paid E5 Flex had capacity but has not been launched. The installed Unity editor lacks Linux Standalone support, so there is no compatible Linux game-server build. The checked-in client remains on loopback.

When a compatible Linux server is deployed, record its reserved IP or DNS name in the deployment environment and a release-specific `StreamingAssets/server.json`. Do not replace the loopback defaults in the repository with a guessed or temporary public address. Keep ports `8081`, `8082` and `8083` private; expose only HTTPS and the final encrypted gameplay transport after the deployment gates in [OCI-DEPLOYMENT.md](OCI-DEPLOYMENT.md) pass.

## Credentials and machine state

OCI config, API-signing private keys, SSH private keys, admin hashes, SQLite data, logs and `node_modules` are intentionally outside Git. A fresh clone creates its own local runtime state after dependency installation.
