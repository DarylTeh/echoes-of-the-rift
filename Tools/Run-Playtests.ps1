param([switch]$Coop)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$exe=Join-Path $projectRoot 'Builds/Windows/EchoesOfTheRift.exe'
if(-not(Test-Path -LiteralPath $exe)){throw 'Build the Windows player first.'}
function Start-TestPlayer([string]$role,[string]$flag) {
    $output=Join-Path $projectRoot ('Logs/'+$role)
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    $log=Join-Path $output 'player.log'
    $arguments='-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -cookieSmoke {0} -cookieOutput "{1}" -logFile "{2}"' -f $flag,$output,$log
    return Start-Process -FilePath $exe -ArgumentList $arguments -WindowStyle Hidden -PassThru
}
$players=@()
try {
    if($Coop) { $players+=Start-TestPlayer 'CoopHost' '-cookieCoopHost'; $players+=Start-TestPlayer 'CoopClient' '-cookieCoopClient' }
    else { $players+=Start-TestPlayer 'Runtime' '' }
    foreach($player in $players) {
        if(-not $player.WaitForExit(55000)){throw 'Player test timed out.'}
        if($player.ExitCode -ne 0){throw "Player test failed, exit $($player.ExitCode). Read Logs/*/player.log."}
    }
    $roles=if($Coop){@('CoopHost','CoopClient')}else{@('Runtime')}
    foreach($role in $roles){Get-Content -LiteralPath (Join-Path $projectRoot "Logs/$role/runtime-smoke.txt")}
} finally { foreach($player in $players){if(-not $player.HasExited){$player.Kill()}} }
