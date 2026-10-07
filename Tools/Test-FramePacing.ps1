param([string]$Executable)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
if($Executable){$exe=$Executable;if(-not(Test-Path -LiteralPath $exe)){throw "Game executable not found: $exe"}}
else{$exe=Join-Path $projectRoot 'Release/Windows/EchoesOfTheRift.exe';if(-not(Test-Path -LiteralPath $exe)){$exe=Join-Path $projectRoot 'Builds/Windows/EchoesOfTheRift.exe'};if(-not(Test-Path -LiteralPath $exe)){throw 'Build the Windows player first.'}}
$output=Join-Path $projectRoot 'Logs/FramePacing'
New-Item -ItemType Directory -Force $output | Out-Null
$log=Join-Path $output 'player.log'
$arguments='-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -cookieSmoke -framePacingTest -cookieOutput "{0}" -logFile "{1}"' -f $output,$log
$player=Start-Process -FilePath $exe -ArgumentList $arguments -WindowStyle Hidden -PassThru
try{
    if(-not $player.WaitForExit(45000)){throw 'Frame-pacing test timed out.'}
    $result=Get-Content -LiteralPath (Join-Path $output 'frame-pacing.txt') -Raw -ErrorAction SilentlyContinue
    if(-not $result){throw 'The player did not produce a frame-pacing result.'}
    Write-Output $result.Trim()
    if($player.ExitCode -ne 0 -or $result -notmatch '^PASS '){throw 'Frame-pacing target was missed; inspect Logs/FramePacing/player.log.'}
}finally{if(-not $player.HasExited){$player.Kill()}}
