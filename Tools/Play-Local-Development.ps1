param([switch]$Test)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$output=Join-Path $projectRoot 'Logs/Launcher'
New-Item -ItemType Directory -Force $output | Out-Null
$owned=@()
try {
    $exe=Join-Path $projectRoot 'Builds/Windows/EchoesOfTheRift.exe'
    if(-not (Test-Path -LiteralPath $exe)){throw 'The game build is missing. Keep the launcher beside the CookieRaid folder.'}
    $node=(Get-Command node -ErrorAction Stop).Source
    $ports=[System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties()
    if(($ports.GetActiveTcpListeners() | Where-Object {$_.Port -eq 8081 -or $_.Port -eq 8082}) -or ($ports.GetActiveUdpListeners() | Where-Object Port -eq 7770)){throw 'A local game server is already running. Close the existing server before using Play Echoes of the Rift.'}
    $env:COOKIE_SERVER_KEY=[Guid]::NewGuid().ToString('N')+[Guid]::NewGuid().ToString('N')
    $env:COOKIE_DB_PATH=if($Test){Join-Path $output ('test-'+[Guid]::NewGuid().ToString('N')+'.sqlite')}else{Join-Path $projectRoot 'Server/progress.sqlite'}
    $owned+=Start-Process $node -ArgumentList 'persistence.mjs' -WorkingDirectory (Join-Path $projectRoot 'Server') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $output 'database.log') -RedirectStandardError (Join-Path $output 'database-error.log')
    $ready=$false
    for($i=0;$i -lt 40;$i++){
        if($owned[0].HasExited){throw 'The save service could not start. See Logs/Launcher/database-error.log.'}
        try{$null=Invoke-RestMethod 'http://127.0.0.1:8081/health' -TimeoutSec 1;$ready=$true;break}catch{Start-Sleep -Milliseconds 250}
    }
    if(-not $ready){throw 'The save service did not become ready.'}
    $serverLog=Join-Path $output ('server-'+[Guid]::NewGuid().ToString('N')+'.log')
    $testFlag=if($Test){'-dedicatedTest'}else{''}
    $server=Start-Process $exe -ArgumentList ('-batchmode -nographics -dedicatedServer {0} -logFile "{1}"' -f $testFlag,$serverLog) -WindowStyle Hidden -PassThru
    $owned+=$server
    $ready=$false
    for($i=0;$i -lt 120;$i++){
        if($server.HasExited){throw 'The game server stopped during startup.'}
        if((Test-Path -LiteralPath $serverLog) -and (Select-String -LiteralPath $serverLog -SimpleMatch 'Local server is started' -Quiet)){$ready=$true;break}
        Start-Sleep -Milliseconds 250
    }
    if(-not $ready){throw 'The game server did not become ready within 30 seconds.'}
    $arguments='-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "'+(Join-Path $output 'player.log')+'"'
    if($Test){$arguments+=' -cookieSmoke -dedicatedClientTest -cookieOutput "'+$output+'"'}
    # Visible player window is intentional: this is the user's play launcher.
    $client=Start-Process $exe -ArgumentList $arguments -PassThru
    $owned+=$client
    if($Test){if(-not $client.WaitForExit(90000)){throw 'Launcher acceptance test timed out.'}}else{$client.WaitForExit()}
    if($client.ExitCode -ne 0){throw 'The game closed unexpectedly. See Logs/Launcher/player.log.'}
    if($Test){$result=Get-Content (Join-Path $output 'runtime-smoke.txt') -Raw;if($result -notmatch '^PASS '){throw $result}}
} catch {
    $_.Exception.Message | Set-Content (Join-Path $output 'error.txt')
    exit 1
} finally {
    foreach($process in $owned){if(-not $process.HasExited){$process.Kill();$process.WaitForExit()}}
}
