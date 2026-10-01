# Echoes of the Rift release package

This folder is the clone-ready Windows handoff. `Windows/` contains the current playable client distribution without debug symbol files. The two launchers start the project scripts relative to this repository, so the folder can be moved or cloned to another Windows machine.

## First launch

1. Install Node.js 24 or newer.
2. From the repository root, run `npm ci --prefix Server --ignore-scripts` once. This installs the local Swagger dependency; `Server/node_modules/` is intentionally excluded from Git.
3. Double-click `Echoes of the Rift Server.exe` and keep its window open.
4. Double-click `Play Echoes of the Rift.exe`.

The server launcher creates `Server/progress.sqlite` and local logs on first run. Those machine-specific files are ignored and are not shared through Git. The admin shortcut is at the repository root as `Open Rift Admin.cmd`; credentials and the admin hash remain local secrets.

If the Windows build is absent, run the Unity build menu or `Tools/Build-Launcher.ps1` after opening the project. The tracked `Windows/` payload is the development build for this revision; Android and Linux builds are separate platform outputs.
