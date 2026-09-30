$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$output=Join-Path $projectRoot 'Logs/ConnectionFailure'
New-Item -ItemType Directory -Force $output | Out-Null
$exe=Join-Path $projectRoot 'Builds/Windows/EchoesOfTheRift.exe'
$player=Start-Process -FilePath $exe -ArgumentList ('-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -cookieSmoke -dedicatedClientTest -connectionFailureTest -cookieOutput "{0}" -logFile "{1}"' -f $output,(Join-Path $output 'player.log')) -WindowStyle Hidden -PassThru
try {
    if(-not $player.WaitForExit(35000)){throw 'Connection-failure test timed out.'}
    Get-Content (Join-Path $output 'runtime-smoke.txt')
    if($player.ExitCode -ne 0){throw 'Connection-failure UI test failed.'}
} finally {if(-not $player.HasExited){$player.Kill()}}
