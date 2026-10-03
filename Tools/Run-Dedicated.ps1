param([switch]$Test,[switch]$Pair,[switch]$Account,[switch]$RegistrationOnly,[string]$ControlDirectory,[int]$OwnerProcessId=0)
if($Account){$Test=$true}
if($Pair){$Test=$true}
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$serverRoot=Join-Path $projectRoot 'Server'
$exe=Join-Path $projectRoot 'Release/Windows/EchoesOfTheRift.exe'
if(-not(Test-Path -LiteralPath $exe)){$exe=Join-Path $projectRoot 'Builds/Windows/EchoesOfTheRift.exe'}
$output=if($Pair){Join-Path $projectRoot 'Logs/DedicatedPair'}else{Join-Path $projectRoot 'Logs/Dedicated'}
New-Item -ItemType Directory -Force $output | Out-Null
$oldKey=$env:COOKIE_SERVER_KEY
$oldDb=$env:COOKIE_DB_PATH
$env:COOKIE_SERVER_KEY=[Guid]::NewGuid().ToString('N')+[Guid]::NewGuid().ToString('N')
$env:COOKIE_DB_PATH=if($Test){Join-Path $output ('test-'+[Guid]::NewGuid().ToString('N')+'.sqlite')}else{Join-Path $serverRoot 'progress.sqlite'}
$taskEnvironment=@{}
foreach($name in @('COOKIE_DB_PORT','COOKIE_ACCOUNT_PORT','COOKIE_DB_URL','RIFT_TEST_GAME_PORT','RIFT_TEST_ACCOUNT_PORT')){$taskEnvironment[$name]=[Environment]::GetEnvironmentVariable($name)}
$taskDbPort=if($Test){18081}else{8081}
$taskAccountPort=if($Test){18082}else{8082}
$taskGamePort=if($Test){17770}else{7770}
$env:COOKIE_DB_PORT=[string]$taskDbPort
$env:COOKIE_ACCOUNT_PORT=[string]$taskAccountPort
$env:COOKIE_DB_URL="http://127.0.0.1:$taskDbPort"
if($Test){$env:RIFT_TEST_GAME_PORT=[string]$taskGamePort;$env:RIFT_TEST_ACCOUNT_PORT=[string]$taskAccountPort}
$owned=@()
try {
$ports=[System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties()
if(($ports.GetActiveTcpListeners() | Where-Object {$_.Port -in $taskDbPort,$taskAccountPort}) -or ($ports.GetActiveUdpListeners() | Where-Object Port -eq $taskGamePort)){throw 'A local server is already using the required ports.'}
    $owned+=Start-Process -FilePath (Get-Command node).Source -ArgumentList 'persistence.mjs' -WorkingDirectory $serverRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $output 'database.log') -RedirectStandardError (Join-Path $output 'database-error.log')
    $ready=$false
    for($attempt=0;$attempt -lt 30;$attempt++){try{$null=Invoke-RestMethod ("http://127.0.0.1:$taskDbPort/health");$ready=$true;break}catch{Start-Sleep -Milliseconds 200}}
    if(-not $ready){throw 'Persistence service failed to start.'}
    $flag=if($Test){'-dedicatedTest'}else{''}
    $owned+=Start-Process -FilePath $exe -ArgumentList ('-batchmode -nographics -dedicatedServer {0} -logFile "{1}"' -f $flag,(Join-Path $output 'server.log')) -WindowStyle Hidden -PassThru
    if($Test){
        Start-Sleep -Seconds 3
        if($Account){
            $ready=$false
            for($attempt=0;$attempt -lt 60;$attempt++){
                try{$health=Invoke-RestMethod ("http://127.0.0.1:$taskAccountPort/health") -TimeoutSec 1;if($health.ready){$ready=$true;break}}catch{}
                Start-Sleep -Milliseconds 500
            }
            if(-not $ready){throw 'Dedicated game server did not report its readiness heartbeat.'}
        }
        $clients=@()
        $roles=if($Account){if($RegistrationOnly){@('Register')}else{@('Register','Resume')}}elseif($Pair){@('Leader','Peer')}else{@('Client')}
        foreach($role in $roles){
            if($Account -and $role -eq 'Resume' -and -not $RegistrationOnly){
                $registrationResult=Get-Content (Join-Path (Join-Path $output 'Register') 'client.log') -Raw -ErrorAction SilentlyContinue
                if($registrationResult -match 'protectedSession=False'){Write-Output 'SKIP session resume: Windows protected-session storage was unavailable during registration.';continue}
            }
            $clientOutput=if($Pair -or $Account){Join-Path $output $role}else{$output}
            New-Item -ItemType Directory -Force $clientOutput | Out-Null
            $pairFlags=if($Account){if($role -eq 'Resume'){'-accountFlowTest -accountResume'}else{'-accountFlowTest'}}elseif($Pair){if($role -eq 'Leader'){'-dedicatedPair -pairLeader'}else{'-dedicatedPair'}}else{''}
            $client=Start-Process -FilePath $exe -ArgumentList ('-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -cookieSmoke -dedicatedClientTest {2} -cookieOutput "{0}" -logFile "{1}"' -f $clientOutput,(Join-Path $clientOutput 'client.log'),$pairFlags) -WindowStyle Hidden -PassThru
            $owned+=$client
            $clients+=@{Process=$client;Output=$clientOutput}
            if($Account){if(-not $client.WaitForExit(90000)){throw "Account flow timed out"};Start-Sleep -Seconds 2}
        }
        foreach($entry in $clients){
            if(-not $entry.Process.WaitForExit(90000)){throw 'Dedicated client test timed out.'}
            Get-Content (Join-Path $entry.Output 'runtime-smoke.txt') -ErrorAction SilentlyContinue
            if($entry.Process.ExitCode -ne 0){throw 'Dedicated client failed. Inspect Logs/Dedicated or Logs/DedicatedPair.'}
        }
    }else{
        if($ControlDirectory){
            $ready=$false
            for($i=0;$i -lt 60;$i++){try{$health=Invoke-RestMethod ("http://127.0.0.1:$taskAccountPort/health") -TimeoutSec 1;if($health.ready){$ready=$true;break}}catch{};Start-Sleep -Milliseconds 500}
            if(-not $ready){throw 'Game server did not become ready.'}
            Set-Content -LiteralPath (Join-Path $ControlDirectory 'ready') -Value 'ready'
        }
        Write-Output 'Dedicated server running on UDP 7770. Private persistence on 127.0.0.1:8081. Stop this script to stop both.'
        while(-not $owned[-1].HasExited){
            if($ControlDirectory -and (Test-Path -LiteralPath (Join-Path $ControlDirectory 'stop'))){break}
            if($OwnerProcessId -gt 0 -and -not (Get-Process -Id $OwnerProcessId -ErrorAction SilentlyContinue)){break}
            Start-Sleep -Milliseconds 500
        }
    }
} finally {
    foreach($process in $owned){if(-not $process.HasExited){$process.Kill();$process.WaitForExit()}}
    $env:COOKIE_SERVER_KEY=$oldKey
    $env:COOKIE_DB_PATH=$oldDb
    foreach($name in $taskEnvironment.Keys){[Environment]::SetEnvironmentVariable($name,$taskEnvironment[$name])}
}
