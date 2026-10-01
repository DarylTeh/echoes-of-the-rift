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

## OCI deployment status

There is currently **no public IP, DNS name or public gameplay endpoint**. The OCI VCN and networking foundation were created, but both Always Free VM shapes were out of capacity in Singapore, so no VM or reserved public IP exists yet. The checked-in Windows client cannot run as an Oracle Linux ARM server.

When a compatible Linux server is deployed, record its reserved IP or DNS name in the deployment environment and a release-specific `StreamingAssets/server.json`. Do not replace the loopback defaults in the repository with a guessed or temporary public address. Keep ports `8081`, `8082` and `8083` private; expose only HTTPS and the final encrypted gameplay transport after the deployment gates in [OCI-DEPLOYMENT.md](OCI-DEPLOYMENT.md) pass.

## Credentials and machine state

OCI config, API-signing private keys, SSH private keys, admin hashes, SQLite data, logs and `node_modules` are intentionally outside Git. A fresh clone creates its own local runtime state after dependency installation.
