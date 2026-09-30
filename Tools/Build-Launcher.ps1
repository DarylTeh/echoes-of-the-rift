$ErrorActionPreference='Stop'
$workspace=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /target:winexe /reference:System.Windows.Forms.dll ('/out:'+(Join-Path $workspace 'Play Echoes of the Rift.exe')) (Join-Path $PSScriptRoot 'Launcher.cs')
if($LASTEXITCODE -ne 0){throw 'Launcher compilation failed.'}

& $compiler /nologo /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll ('/out:'+(Join-Path $workspace 'Echoes of the Rift Server.exe')) (Join-Path $PSScriptRoot 'ServerLauncher.cs')
if($LASTEXITCODE -ne 0){throw 'Server launcher compilation failed.'}
