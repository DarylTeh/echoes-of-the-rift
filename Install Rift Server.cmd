@echo off
setlocal
cd /d "%~dp0"
where node.exe >nul 2>&1 || (
  echo Node.js 24 or newer is required.
  pause
  exit /b 1
)
npm.cmd ci --prefix Server --ignore-scripts
if errorlevel 1 pause
