$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$release=Join-Path $projectRoot 'Release'
New-Item -ItemType Directory -Force -Path $release | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /target:winexe /reference:System.Windows.Forms.dll ('/out:'+(Join-Path $release 'Play Echoes of the Rift.exe')) (Join-Path $PSScriptRoot 'Launcher.cs')
if($LASTEXITCODE -ne 0){throw 'Launcher compilation failed.'}

& $compiler /nologo /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll ('/out:'+(Join-Path $release 'Echoes of the Rift Server.exe')) (Join-Path $PSScriptRoot 'ServerLauncher.cs')
if($LASTEXITCODE -ne 0){throw 'Server launcher compilation failed.'}
