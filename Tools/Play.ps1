param([switch]$Test)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$output=Join-Path $projectRoot 'Logs/Launcher'
New-Item -ItemType Directory -Force $output | Out-Null
try {
    $exe=Join-Path $projectRoot 'Builds/Windows/EchoesOfTheRift.exe'
    if(-not (Test-Path -LiteralPath $exe)){throw 'Game files were not found.'}
    $arguments='-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "'+(Join-Path $output 'player.log')+'"'
    if($Test){$arguments+=' -cookieSmoke -accountFlowTest -startupUnavailableTest -cookieOutput "'+$output+'"'}
    # The normal player launcher never starts database or game-server processes.
    $client=Start-Process $exe -ArgumentList $arguments -PassThru
    if($Test){if(-not $client.WaitForExit(45000)){$client.Kill();throw 'Startup test timed out.'}}else{$client.WaitForExit()}
    if($client.ExitCode -ne 0){throw 'The game stopped unexpectedly. See Logs/Launcher/player.log.'}
    if($Test){$result=Get-Content (Join-Path $output 'runtime-smoke.txt') -Raw;if($result -notmatch '^PASS '){throw $result}}
} catch { $_.Exception.Message | Set-Content (Join-Path $output 'error.txt');exit 1 }
