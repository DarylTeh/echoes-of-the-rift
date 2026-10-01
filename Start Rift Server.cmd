@echo off
setlocal
cd /d "%~dp0"
if not exist "Release\Echoes of the Rift Server.exe" (
  echo The tracked Windows release is missing. Open the repository in Unity and build it first.
  pause
  exit /b 1
)
start "Echoes of the Rift Server" "%~dp0Release\Echoes of the Rift Server.exe"
