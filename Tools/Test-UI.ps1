param([switch]$Touch)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$output=Join-Path $projectRoot $(if($Touch){'Logs/UILayoutTouch'}else{'Logs/UILayout'})
$touchFlag=if($Touch){'-touchControls'}else{''}
New-Item -ItemType Directory -Force $output | Out-Null
$exe=Join-Path $projectRoot 'Release/Windows/EchoesOfTheRift.exe'
if(-not(Test-Path -LiteralPath $exe)){$exe=Join-Path $projectRoot 'Builds/Windows/EchoesOfTheRift.exe'}
$player=Start-Process $exe -ArgumentList ('-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -cookieSmoke -uiLayoutTest {2} -cookieOutput "{0}" -logFile "{1}"' -f $output,(Join-Path $output 'player.log'),$touchFlag) -WindowStyle Hidden -PassThru
try {
    if(-not $player.WaitForExit(90000)){throw 'UI checks timed out.'}
    $result=Get-Content (Join-Path $output 'runtime-smoke.txt') -Raw
    Write-Output $result
    if($result -notmatch '^PASS '){Get-Content (Join-Path $output 'layout-failures.txt') | Select-Object -Unique -First 15;throw 'UI layout checks failed.'}
} finally {if(-not $player.HasExited){$player.Kill()}}
