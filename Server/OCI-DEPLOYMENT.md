# OCI Always Free deployment runbook

## Status

Revision 43, 30 September 2026. OCI authentication is working and the network foundation is provisioned. Both Always Free VM shapes returned out-of-host-capacity in the Singapore home availability domain, so no VM or public IP exists yet; the local Windows stack is playable and no DNS record or public gameplay endpoint has been enabled.

Provisioned network names: `echoes-rift-vcn`, `echoes-rift-igw`, `echoes-rift-public-route`, `echoes-rift-public-subnet`, `echoes-rift-private-default` and `echoes-rift-nsg`. The subnet is `10.42.1.0/24` inside VCN `10.42.0.0/16`. The NSG allows SSH only from the operator address used during provisioning, TCP 80/443 from the internet, and outbound traffic; ports 8081, 8082, 8083 and gameplay remain closed. Re-run [oci-provision-echoes.ps1](oci-provision-echoes.ps1) after capacity returns. For the smaller staging VM, use `-Shape VM.Standard.E2.1.Micro -InstanceName echoes-rift-api`; for the future Unity host, use the default `VM.Standard.A1.Flex`. The script is idempotent for the named resources.

## Target architecture

```text
game client -- HTTPS 443 --> Caddy --> account/readiness 127.0.0.1:8082
game client -- encrypted gameplay transport --> Linux Unity server (future build)
operator -- SSH tunnel --> admin 127.0.0.1:8083
SQLite progress database --> private disk --> consistent backups
```

The persistence API on `127.0.0.1:8081`, account service on `127.0.0.1:8082`, admin Swagger on `127.0.0.1:8083`, and SQLite files stay private. Only the final HTTPS account endpoint and the approved gameplay transport may be public.

## OCI resources

Create the VM in the tenancy's home region. The current Always Free allowance is the equivalent of 2 total OCPUs and 12 GB memory across Ampere A1 Flex instances, 200 GB combined boot/block storage and five volume backups. The A1 shape is the preferred target for the Unity server once a Linux ARM64 build is validated. The AMD E2.1.Micro shape is appropriate for a small Node-only staging service, but its 1 GB memory is not an assumed fit for the Unity server.

Oracle can report temporary A1 capacity errors, and its Always Free documentation warns that idle instances may be reclaimed. This tier is suitable for development and a small test audience, not a production uptime guarantee.

Provision:

1. Select the home region and root compartment.
2. Create one `VM.Standard.A1.Flex` instance with 2 OCPUs and 12 GB memory, an Ubuntu ARM64 or Oracle Linux ARM64 image, and a boot volume sized within the 200 GB total allowance.
3. Create a VCN with a public subnet, internet gateway, route rule and a reserved public IPv4 address.
4. Use a network security group and OS firewall. Permit SSH only from the operator's fixed address, TCP 80/443 for certificate issuance and HTTPS, and the final gameplay port only after encrypted transport is ready. Do not permit 8081, 8082 or 8083 from the internet.
5. Create a DNS record for the account endpoint and wait for it to resolve before enabling automatic HTTPS.

## Runtime gates before provisioning

The checked-in build is Windows x64. It cannot run natively on an Oracle Linux ARM VM. Install Unity Linux server build support, activate Unity on the development machine, produce a Linux ARM64 server build, and run the FishNet transport and native dependency checks before copying anything to OCI. If Linux ARM64 is not supported by the chosen transport, use a compatible x86 Linux shape only after capacity and memory testing; do not rely on emulation for the game server.

The current Tugboat UDP transport is a development transport. The account client permits a remote endpoint only over HTTPS, but HTTPS does not encrypt UDP gameplay. Public play therefore remains blocked until the gameplay transport is authenticated and encrypted or replaced with an appropriate relay.

## Service layout

Use a dedicated non-root account such as `echoes`, with the application under `/srv/echoes`, the database under `/srv/echoes/data`, and logs under `/var/log/echoes`. Store `COOKIE_SERVER_KEY`, catalog settings and the admin credential hash in a root-owned environment file with mode `0600`; never commit a password, private key or OCI credential to the repository.

Run the Node services and the Linux Unity server under `systemd` with `Restart=on-failure`, resource limits, journal rate limits and an explicit working directory. Keep the admin service bound to loopback and access it through an SSH tunnel:

```text
ssh -L 8083:127.0.0.1:8083 echoes@<reserved-ip>
```

Then open the existing admin path locally. Do not expose Swagger or the SQLite database through Caddy.

## OCI CLI key setup

The official OCI CLI is installed for the Windows user at `C:\Users\daryl\oci-cli`, and new terminals receive that directory through the user PATH. The local RSA signing pair is:

```text
Private: C:\Users\daryl\.oci\oci_api_key.pem
Public:  C:\Users\daryl\.oci\oci_api_key_public.pem
Fingerprint: 03:9c:82:68:45:dc:ca:16:ad:15:58:fd:97:75:e9:3d
```

In the OCI Console, open **Profile → User settings → API Keys → Add API Key → Choose public key file**, select the public PEM file above, and add it. Then open a new PowerShell window and run:

```powershell
oci setup config
```

Enter the tenancy OCID, user OCID, home-region identifier and the private-key path when prompted. The CLI configuration belongs in `C:\Users\daryl\.oci\config`; keep it outside the repository. Verify it with a harmless read-only command such as:

```powershell
oci iam region list --output table
```

The generated secret/auth token shown in a separate Oracle dialog is not an OCI CLI signing key. Do not put it in the config file; revoke it if it was exposed. OCI API signing keys are RSA PEM pairs, and the private key stays local.

## Backups and recovery

Run the repository's SQLite-consistent backup command outside the live database directory, retain several rotating copies and copy encrypted backups to private Object Storage. Test restoring a backup into a separate database before calling the service recoverable. Add a reboot test, `/health` monitoring for the account service and a game heartbeat monitor before inviting external players.

## Go-live order

1. Activate Unity and install the required Linux build module.
2. Build and locally run the Linux server with the same Node persistence/account services.
3. Measure memory, CPU, frame time, reconnect, reward recovery and database restore.
4. Provision the OCI VM and private services.
5. Configure DNS, Caddy HTTPS and the narrow firewall rules.
6. Update `StreamingAssets/server.json` in a release client to use the HTTPS account URL and the validated gameplay endpoint.
7. Test fresh registration, reconnect, event rewards, admin audit, VM reboot and backup restore from a separate network.

## Official references

- [OCI Free Tier](https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier.htm)
- [OCI Always Free resources](https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm)
- [Unity license activation](https://docs.unity.com/en-us/engine/6000.6/manual/get-started/install-and-upgrade/licenses-and-activation/license-activation-methods)
- [Unity dedicated server builds](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/dedicated-server/build)
