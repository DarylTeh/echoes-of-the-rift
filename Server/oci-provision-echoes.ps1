param(
    [string]$SshPublicKeyPath = 'C:\Users\daryl\.ssh\echoes_oci_ed25519.pub',
    [string]$CurrentPublicIp = '',
    [string]$InstanceName = 'echoes-rift-staging',
    [ValidateSet('VM.Standard.A1.Flex','VM.Standard.E2.1.Micro')]
    [string]$Shape = 'VM.Standard.A1.Flex'
)

$ErrorActionPreference = 'Stop'
$Oci = 'C:\Users\daryl\oci-cli\Scripts\oci.exe'
$ConfigPath = 'C:\Users\daryl\.oci\config'

if (-not (Test-Path $Oci)) { throw "OCI CLI was not found at $Oci" }
if (-not (Test-Path $ConfigPath)) { throw "OCI config was not found at $ConfigPath" }
if (-not (Test-Path $SshPublicKeyPath)) { throw "SSH public key was not found at $SshPublicKeyPath" }

function Get-ConfigValue([string]$Name) {
    $line = Get-Content $ConfigPath | Where-Object { $_ -match "^$Name=" } | Select-Object -First 1
    if (-not $line) { throw "Missing $Name in $ConfigPath" }
    return ($line -split '=', 2)[1].Trim()
}

function Invoke-OciJson([string[]]$Arguments) {
    $text = (& $Oci @Arguments '--output' 'json' 2>&1 | Out-String)
    if ($LASTEXITCODE -ne 0) { throw "OCI command failed: $($Arguments -join ' ')`n$text" }
    $match = [regex]::Match($text, '(?m)^\s*[\{\[]')
    if (-not $match.Success) {
        if ($Arguments -contains 'list') { return [pscustomobject]@{ data = @() } }
        throw "OCI command returned no JSON: $($Arguments -join ' ')`n$text"
    }
    return ($text.Substring($match.Index).TrimStart() | ConvertFrom-Json)
}

function Find-ByName($Items, [string]$Name) {
    return @($Items | Where-Object { $_.'display-name' -eq $Name }) | Select-Object -First 1
}

function New-JsonFile($Value, [string]$Name, [switch]$AsArray) {
    if ($AsArray) { $Value = @($Value) }
    $path = Join-Path ([System.IO.Path]::GetTempPath()) ("echoes-oci-" + $Name + '-' + [guid]::NewGuid().ToString('N') + '.json')
    (ConvertTo-Json -InputObject $Value -Depth 8 -Compress) | Set-Content -LiteralPath $path -Encoding ASCII
    return $path
}

$TenancyId = Get-ConfigValue 'tenancy'
$Region = Get-ConfigValue 'region'
if (-not $CurrentPublicIp) { $CurrentPublicIp = (Invoke-RestMethod -Uri 'https://api.ipify.org?format=text').Trim() }
if ($CurrentPublicIp -notmatch '^\d{1,3}(\.\d{1,3}){3}$') { throw "CurrentPublicIp must be an IPv4 address" }

$vcnName = 'echoes-rift-vcn'
$igwName = 'echoes-rift-igw'
$routeName = 'echoes-rift-public-route'
$slName = 'echoes-rift-private-default'
$subnetName = 'echoes-rift-public-subnet'
$nsgName = 'echoes-rift-nsg'
$publicIpName = 'echoes-rift-reserved-ip'

$vcn = Find-ByName (Invoke-OciJson @('network','vcn','list','--compartment-id',$TenancyId,'--all')).data $vcnName
if (-not $vcn) {
    $vcn = (Invoke-OciJson @('network','vcn','create','--compartment-id',$TenancyId,'--display-name',$vcnName,'--cidr-block','10.42.0.0/16')).data
    Write-Host "Created VCN $($vcn.id)"
} else { Write-Host "Using VCN $($vcn.id)" }

$igw = Find-ByName (Invoke-OciJson @('network','internet-gateway','list','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--all')).data $igwName
if (-not $igw) {
    $igw = (Invoke-OciJson @('network','internet-gateway','create','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--display-name',$igwName,'--is-enabled','true')).data
    Write-Host "Created internet gateway $($igw.id)"
} else { Write-Host "Using internet gateway $($igw.id)" }

$route = Find-ByName (Invoke-OciJson @('network','route-table','list','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--all')).data $routeName
if (-not $route) {
    $rulesFile = New-JsonFile @{ destination = '0.0.0.0/0'; destinationType = 'CIDR_BLOCK'; networkEntityId = $igw.id } 'routes' -AsArray
    try { $route = (Invoke-OciJson @('network','route-table','create','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--display-name',$routeName,'--route-rules',('file://' + ($rulesFile -replace '\\','/')))).data }
    finally { Remove-Item -LiteralPath $rulesFile -Force -ErrorAction SilentlyContinue }
    Write-Host "Created route table $($route.id)"
} else { Write-Host "Using route table $($route.id)" }

$securityList = Find-ByName (Invoke-OciJson @('network','security-list','list','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--all')).data $slName
if (-not $securityList) {
    $egressFile = New-JsonFile @{ protocol = 'all'; destination = '0.0.0.0/0'; destinationType = 'CIDR_BLOCK'; description = 'Allow outbound traffic' } 'egress' -AsArray
    $ingressFile = Join-Path ([System.IO.Path]::GetTempPath()) ("echoes-oci-ingress-" + [guid]::NewGuid().ToString('N') + '.json')
    '[]' | Set-Content -LiteralPath $ingressFile -Encoding ASCII
    try { $securityList = (Invoke-OciJson @('network','security-list','create','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--display-name',$slName,'--egress-security-rules',('file://' + ($egressFile -replace '\\','/')),'--ingress-security-rules',('file://' + ($ingressFile -replace '\\','/')))).data }
    finally { Remove-Item -LiteralPath $egressFile,$ingressFile -Force -ErrorAction SilentlyContinue }
    Write-Host "Created empty-ingress security list $($securityList.id)"
} else { Write-Host "Using security list $($securityList.id)" }

$subnet = Find-ByName (Invoke-OciJson @('network','subnet','list','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--all')).data $subnetName
if (-not $subnet) {
    $securityListIdsFile = New-JsonFile $securityList.id 'security-list-ids' -AsArray
    try { $subnet = (Invoke-OciJson @('network','subnet','create','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--display-name',$subnetName,'--cidr-block','10.42.1.0/24','--route-table-id',$route.id,'--security-list-ids',('file://' + ($securityListIdsFile -replace '\\','/')),'--prohibit-public-ip-on-vnic','false')).data }
    finally { Remove-Item -LiteralPath $securityListIdsFile -Force -ErrorAction SilentlyContinue }
    Write-Host "Created public subnet $($subnet.id)"
} else { Write-Host "Using subnet $($subnet.id)" }

$nsg = Find-ByName (Invoke-OciJson @('network','nsg','list','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--all')).data $nsgName
if (-not $nsg) {
    $nsg = (Invoke-OciJson @('network','nsg','create','--compartment-id',$TenancyId,'--vcn-id',$vcn.id,'--display-name',$nsgName)).data
    $rules = @(
        @{ direction = 'INGRESS'; protocol = '6'; source = "$CurrentPublicIp/32"; sourceType = 'CIDR_BLOCK'; tcpOptions = @{ destinationPortRange = @{ min = 22; max = 22 } }; description = 'SSH from operator address' },
        @{ direction = 'INGRESS'; protocol = '6'; source = '0.0.0.0/0'; sourceType = 'CIDR_BLOCK'; tcpOptions = @{ destinationPortRange = @{ min = 80; max = 80 } }; description = 'HTTP certificate and redirect traffic' },
        @{ direction = 'INGRESS'; protocol = '6'; source = '0.0.0.0/0'; sourceType = 'CIDR_BLOCK'; tcpOptions = @{ destinationPortRange = @{ min = 443; max = 443 } }; description = 'HTTPS account endpoint' },
        @{ direction = 'EGRESS'; protocol = 'all'; destination = '0.0.0.0/0'; destinationType = 'CIDR_BLOCK'; description = 'Allow outbound traffic' }
    )
    $nsgRulesFile = New-JsonFile $rules 'nsg-rules' -AsArray
    try { Invoke-OciJson @('network','nsg','rules','add','--nsg-id',$nsg.id,'--security-rules',('file://' + ($nsgRulesFile -replace '\\','/'))) | Out-Null }
    finally { Remove-Item -LiteralPath $nsgRulesFile -Force -ErrorAction SilentlyContinue }
    Write-Host "Created NSG $($nsg.id) with SSH restricted to $CurrentPublicIp/32"
} else { Write-Host "Using NSG $($nsg.id)" }

$instance = Find-ByName (Invoke-OciJson @('compute','instance','list','--compartment-id',$TenancyId,'--all')).data $InstanceName
if (-not $instance) {
    $ad = ((Invoke-OciJson @('iam','availability-domain','list','--compartment-id',$TenancyId)).data | Select-Object -First 1).name
    $images = (Invoke-OciJson @('compute','image','list','--compartment-id',$TenancyId,'--shape',$Shape,'--operating-system','Canonical Ubuntu','--operating-system-version','24.04','--sort-by','TIMECREATED','--sort-order','DESC','--all')).data
    $image = $images | Sort-Object { $_.'time-created' } -Descending | Select-Object -First 1
    if (-not $image) { throw 'No Ubuntu 24.04 ARM64 image was returned for VM.Standard.A1.Flex' }
    $nsgIdsFile = New-JsonFile $nsg.id 'nsg-ids' -AsArray
    $launchArgs = @('compute','instance','launch','--availability-domain',$ad,'--compartment-id',$TenancyId,'--display-name',$InstanceName,'--shape',$Shape,'--image-id',$image.id,'--subnet-id',$subnet.id,'--nsg-ids',('file://' + ($nsgIdsFile -replace '\\','/')),'--assign-public-ip','false','--boot-volume-size-in-gbs','50','--ssh-authorized-keys-file',$SshPublicKeyPath)
    $shapeConfigFile = $null
    if ($Shape -eq 'VM.Standard.A1.Flex') {
        $shapeConfigFile = New-JsonFile @{ ocpus = 2; memoryInGBs = 12 } 'shape-config'
        $launchArgs += @('--shape-config',('file://' + ($shapeConfigFile -replace '\\','/')))
    }
    try { $instance = (Invoke-OciJson $launchArgs).data }
    finally { Remove-Item -LiteralPath $nsgIdsFile,$shapeConfigFile -Force -ErrorAction SilentlyContinue }
    Write-Host "Launched instance $($instance.id) using $($image.'display-name')"
} else { Write-Host "Using instance $($instance.id) in state $($instance.'lifecycle-state')" }

for ($i = 0; $i -lt 24; $i++) {
    $state = (Invoke-OciJson @('compute','instance','get','--instance-id',$instance.id)).data
    if ($state.'lifecycle-state' -in @('RUNNING','STOPPED','TERMINATED','TERMINATING')) { break }
    Start-Sleep -Seconds 10
}
if ($state.'lifecycle-state' -ne 'RUNNING') { throw "Instance is not running; current state is $($state.'lifecycle-state')" }

$attachments = (Invoke-OciJson @('compute','vnic-attachment','list','--compartment-id',$TenancyId,'--instance-id',$instance.id,'--all')).data
$attachment = $attachments | Where-Object { $_.'is-primary' -eq $true } | Select-Object -First 1
if (-not $attachment) { $attachment = $attachments | Select-Object -First 1 }
$vnic = (Invoke-OciJson @('network','vnic','get','--vnic-id',$attachment.'vnic-id')).data
$privateIp = $vnic.'private-ip'
$publicIp = (Invoke-OciJson @('network','public-ip','create','--compartment-id',$TenancyId,'--lifetime','RESERVED','--display-name',$publicIpName,'--private-ip-id',$privateIp.'id')).data

Write-Host "INSTANCE_ID=$($instance.id)"
Write-Host "VCN_ID=$($vcn.id)"
Write-Host "SUBNET_ID=$($subnet.id)"
Write-Host "NSG_ID=$($nsg.id)"
Write-Host "PUBLIC_IP=$($publicIp.'ip-address')"
Write-Host "SSH_COMMAND=ssh -i $($SshPublicKeyPath -replace '\.pub$','') ubuntu@$($publicIp.'ip-address')"
