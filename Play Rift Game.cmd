@echo off
setlocal
cd /d "%~dp0"
if not exist "Release\Play Echoes of the Rift.exe" (
  echo The tracked Windows release is missing. Clone the complete repository or build it in Unity first.
  pause
  exit /b 1
)
start "Echoes of the Rift" "%~dp0Release\Play Echoes of the Rift.exe"
