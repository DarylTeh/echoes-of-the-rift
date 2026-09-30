param([switch]$NoBrowser)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$serverRoot=Join-Path $projectRoot 'Server'
$output=Join-Path $projectRoot 'Logs/Admin'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$port=if($env:RIFT_ADMIN_PORT){[int]$env:RIFT_ADMIN_PORT}else{8083}
$basePath="/allah/api/hehe/v69/docs"
$url="http://localhost:$port$basePath/"
$healthUrl="http://127.0.0.1:$port$basePath/health"
$running=$false
try {$health=Invoke-RestMethod $healthUrl -TimeoutSec 2;$running=$health.service -eq 'echoes-local-admin'} catch {}
if(-not $running){
    if(-not(Test-Path -LiteralPath (Join-Path $serverRoot 'node_modules/swagger-ui-dist/swagger-ui-bundle.js'))){throw 'Run npm ci --prefix Server --ignore-scripts from CookieRaid once to install the local Swagger assets.'}
    $process=Start-Process -FilePath (Get-Command node).Source -ArgumentList 'admin.mjs' -WorkingDirectory $serverRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $output 'admin.log') -RedirectStandardError (Join-Path $output 'admin-error.log')
    $process.Id | Set-Content -LiteralPath (Join-Path $output 'admin.pid')
    for($attempt=0;$attempt -lt 30;$attempt++){
        if($process.HasExited){throw 'Admin server exited. See Logs/Admin/admin-error.log.'}
        try {$health=Invoke-RestMethod $healthUrl -TimeoutSec 1;if($health.service -eq 'echoes-local-admin'){$running=$true;break}} catch {}
        Start-Sleep -Milliseconds 200
    }
    if(-not $running){throw 'Admin page did not become ready.'}
}
Write-Output "Admin API ready: $url"
Write-Output 'Click Authorize and enter your configured admin username and password. Reconnect the game after editing a profile.'
if(-not $NoBrowser){Start-Process "$url"}

