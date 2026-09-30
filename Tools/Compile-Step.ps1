param(
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.6.2f1\Editor\Unity.exe',
    [ValidateRange(1, 10)][int]$Step = 1,
    [string]$Method = 'ProjectValidation.CompileCLI'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $UnityPath)) { throw "Unity editor not found: $UnityPath" }
$logFolder = Join-Path $projectRoot 'Logs'
New-Item -ItemType Directory -Path $logFolder -Force | Out-Null
$logPath = Join-Path $logFolder ('step-{0:00}-{1}.log' -f $Step, (Get-Date -Format 'yyyyMMdd-HHmmss'))
$arguments = '-batchmode -nographics -projectPath "{0}" -executeMethod {1} -quit -logFile "{2}"' -f $projectRoot, $Method, $logPath
$run = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (-not $run.WaitForExit(900000)) {
    $run.Kill()
    throw "Unity timed out after 15 minutes. Read $logPath"
}
if (-not (Test-Path -LiteralPath $logPath)) { throw 'Unity did not create a log.' }
$log = Get-Content -LiteralPath $logPath -Raw
if ($run.ExitCode -ne 0 -or $log -match 'error CS\d+|Scripts have compiler errors|Aborting batchmode') {
    throw "Step $Step failed (exit $($run.ExitCode)). Read $logPath"
}
if ($Method -eq 'ProceduralPixelSpriteGenerator.GenerateAllPixelAssetsCLI' -and $log -notmatch 'PIXEL_ASSETS_OK:') {
    throw "Unity did not complete sprite generation. Read $logPath"
}
if ($Method -eq 'ProjectValidation.CompileCLI' -and $log -notmatch 'COMPILE_OK:') {
    throw "Unity did not complete validation. Read $logPath"
}
Write-Output "Step $Step passed. Log: $logPath"
