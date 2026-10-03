# Echoes of the Rift release package

This folder is the clone-ready Windows handoff. `Windows/` contains the playable client distribution and Unity runtime files, including `dstorage.dll` and `dstoragecore.dll`, which must ship beside the player executable. The current executable is older than the Unity source tree: its account screen still says “15+ characters” although current source/server policy is “5+”. Rebuild the player with the licensed Unity Editor before treating the package as the latest UI revision. The two launchers start the project scripts relative to this repository, so the folder can be moved or cloned to another Windows machine. The complete Unity project and server source are one level above this folder; endpoint and cloud status are in `../Server/ENDPOINTS.md`.

## First launch

1. Install Node.js 24 or newer.
2. From the repository root, run `npm ci --prefix Server --ignore-scripts` once. This installs the local Swagger dependency; `Server/node_modules/` is intentionally excluded from Git.
3. Double-click `Echoes of the Rift Server.exe` and keep its window open.
4. Double-click `Play Echoes of the Rift.exe`.

The server launcher creates `Server/progress.sqlite` and local logs on first run. Those machine-specific files are ignored and are not shared through Git. The admin shortcut is at the repository root as `Open Rift Admin.cmd`; credentials and the admin hash remain local secrets.

If a current Windows build is needed, run the Unity build menu or `Tools/Build-Launcher.ps1` after opening the project in the licensed Editor. Android and Linux builds are separate platform outputs.
