#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Installs PowellTecRelay as a Windows Service under a dedicated low-privilege account.

.DESCRIPTION
    Creates a local service account (svc_relay) with no admin rights, grants it only
    the permissions the relay needs, then installs and starts the service under that account.

    DO NOT run the service as SYSTEM or a local administrator. The relay exposes a
    PowerShell execution endpoint over HTTP. If it runs as an admin account, a compromised
    API key gives an attacker full control of the machine. A dedicated low-privilege account
    limits the blast radius to only what svc_relay can do.

.PARAMETER RelayPath
    Folder containing PowellTecRelay.exe. Defaults to C:\PowellTecRelay.

.PARAMETER ServicePassword
    Password for the svc_relay local account. Required â€” choose a strong password.

.PARAMETER ApiKey
    API key written into appsettings.json. Change this from the default before deploying.

.EXAMPLE
    .\Install-RelayService.ps1 -ServicePassword 'S3cur3P@ss!' -ApiKey 'my-secret-key'
#>
param(
    [string]$RelayPath      = 'C:\PowellTecRelay',
    [Parameter(Mandatory)]
    [string]$ServicePassword,
    [Parameter(Mandatory)]
    [string]$ApiKey
)

$ServiceName    = 'PowellTecRelay'
$ServiceAccount = 'svc_relay'
$ExePath        = Join-Path $RelayPath 'PowellTecRelay.exe'

# â”€â”€ Preflight â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
if (-not (Test-Path $ExePath)) {
    Write-Error "PowellTecRelay.exe not found at $ExePath. Copy the relay files first."
    exit 1
}

# â”€â”€ 1. Create service account â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# A dedicated account with no group memberships means a compromised process
# cannot escalate or access other local resources.
$existing = Get-LocalUser -Name $ServiceAccount -ErrorAction SilentlyContinue
if (-not $existing) {
    $secPw = ConvertTo-SecureString $ServicePassword -AsPlainText -Force
    New-LocalUser -Name $ServiceAccount -Password $secPw `
                  -Description 'PowellTecRelay service account â€” no interactive logon' `
                  -PasswordNeverExpires -UserMayNotChangePassword | Out-Null
    Write-Host "Created local account: $ServiceAccount"
} else {
    Write-Host "Account $ServiceAccount already exists â€” skipping creation."
}

# â”€â”€ 2. Grant "Log on as a service" right â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
$sidStr = (Get-LocalUser -Name $ServiceAccount).SID.Value
$tempInf = "$env:TEMP\logon_as_service.inf"
@"
[Unicode]
Unicode=yes
[Version]
signature="`$CHICAGO`$"
Revision=1
[Privilege Rights]
SeServiceLogonRight = *$sidStr
"@ | Set-Content $tempInf -Encoding Unicode
secedit /configure /db "$env:TEMP\secedit.sdb" /cfg $tempInf /quiet
Remove-Item $tempInf -Force
Write-Host "Granted SeServiceLogonRight to $ServiceAccount"

# â”€â”€ 3. ACL: service account gets read+execute on the relay folder only â”€â”€â”€â”€â”€â”€â”€â”€
# No write access â€” the service does not need to modify its own files.
$acl  = Get-Acl $RelayPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $ServiceAccount, 'ReadAndExecute', 'ContainerInherit,ObjectInherit', 'None', 'Allow'
)
$acl.AddAccessRule($rule)
Set-Acl $RelayPath $acl
Write-Host "ACL set: $ServiceAccount has ReadAndExecute on $RelayPath"

# â”€â”€ 4. Write API key into appsettings.json â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
$settingsPath = Join-Path $RelayPath 'appsettings.json'
if (Test-Path $settingsPath) {
    $json = Get-Content $settingsPath -Raw | ConvertFrom-Json
    $json.Relay.ApiKey = $ApiKey
    $json | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8
    Write-Host "API key written to appsettings.json"
} else {
    Write-Warning "appsettings.json not found at $settingsPath â€” set the API key manually."
}

# â”€â”€ 5. Install or reconfigure the Windows Service â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
$domainAccount = ".\$ServiceAccount"   # local account in sc.exe format
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($svc) {
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe config $ServiceName obj= $domainAccount password= $ServicePassword | Out-Null
    Write-Host "Updated existing service to run as $domainAccount"
} else {
    sc.exe create $ServiceName binPath= $ExePath start= auto obj= $domainAccount password= $ServicePassword | Out-Null
    sc.exe description $ServiceName "PowellTec IT Support Tools relay â€” serves web UI and PowerShell API on port 5000" | Out-Null
    Write-Host "Created service $ServiceName"
}

# â”€â”€ 6. Firewall rule â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
$fwRule = Get-NetFirewallRule -DisplayName "$ServiceName Port 5000" -ErrorAction SilentlyContinue
if (-not $fwRule) {
    New-NetFirewallRule -DisplayName "$ServiceName Port 5000" `
        -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow | Out-Null
    Write-Host "Firewall rule added for port 5000"
} else {
    Write-Host "Firewall rule already exists â€” skipping."
}

# â”€â”€ 7. Start the service â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Start-Service -Name $ServiceName
$state = (Get-Service -Name $ServiceName).Status
Write-Host ""
Write-Host "Service status: $state"
if ($state -eq 'Running') {
    Write-Host "PowellTecRelay is running at http://<thisserver>:5000" -ForegroundColor Green
} else {
    Write-Warning "Service did not reach Running state. Check Event Viewer for details."
}
