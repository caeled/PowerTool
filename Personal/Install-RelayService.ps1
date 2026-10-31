#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Installs or upgrades PowellTecRelay as a Windows Service.

.DESCRIPTION
    Fresh install: creates a local service account (svc_relay), grants minimum
    permissions, writes the API key, copies files, registers and starts the service.

    Upgrade: stops the running service, replaces exe and wwwroot, preserves the
    existing appsettings.json (keeps your API key), then restarts.

    DO NOT run the service as SYSTEM or a local administrator. The relay exposes a
    PowerShell execution endpoint over HTTP. A dedicated low-privilege account limits
    blast radius if the API key is ever compromised.

.PARAMETER SourcePath
    Folder containing the extracted relay artifact (PowellTecRelay.exe + wwwroot\).
    Defaults to the folder this script is in.

.PARAMETER RelayPath
    Install destination. Defaults to C:\PowellTecRelay.

.PARAMETER ServicePassword
    Password for the svc_relay local account. Required for fresh install; optional
    on upgrade (omit to keep the existing password).

.PARAMETER ApiKey
    API key written into appsettings.json. Required for fresh install; omit on
    upgrade to preserve the existing key.

.EXAMPLE
    # Fresh install
    .\Install-RelayService.ps1 -ServicePassword 'REPLACE-WITH-A-STRONG-PASSWORD' -ApiKey 'REPLACE-WITH-YOUR-OWN-API-KEY'

.EXAMPLE
    # Upgrade (preserves API key and password)
    .\Install-RelayService.ps1
#>
param(
    [string]$SourcePath      = $PSScriptRoot,
    [string]$RelayPath       = 'C:\PowellTecRelay',
    [string]$ServicePassword = '',
    [string]$ApiKey          = ''
)

$ServiceName    = 'PowellTecRelay'
$ServiceAccount = 'svc_relay'
$ExeName        = 'PowellTecRelay.exe'
$SourceExe      = Join-Path $SourcePath $ExeName
$DestExe        = Join-Path $RelayPath  $ExeName
$SourceWwwroot  = Join-Path $SourcePath 'wwwroot'
$DestWwwroot    = Join-Path $RelayPath  'wwwroot'
$SettingsPath   = Join-Path $RelayPath  'appsettings.json'
$SourceSettings = Join-Path $SourcePath 'appsettings.json'

# ── Preflight ─────────────────────────────────────────────────────────────────
if (-not (Test-Path $SourceExe)) {
    Write-Error "PowellTecRelay.exe not found at $SourceExe. Point -SourcePath at the extracted artifact folder."
    exit 1
}

$isUpgrade = (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) -ne $null
if ($isUpgrade) {
    Write-Host "Existing service detected — running as UPGRADE." -ForegroundColor Cyan
} else {
    Write-Host "No existing service — running as FRESH INSTALL." -ForegroundColor Cyan
    if (-not $ServicePassword) {
        Write-Error "-ServicePassword is required for a fresh install."
        exit 1
    }
    if (-not $ApiKey) {
        Write-Error "-ApiKey is required for a fresh install."
        exit 1
    }
}

# ── Stop service if running ───────────────────────────────────────────────────
if ($isUpgrade) {
    Write-Host "Stopping $ServiceName..."
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

# ── Create install directory ──────────────────────────────────────────────────
if (-not (Test-Path $RelayPath)) {
    New-Item -ItemType Directory -Path $RelayPath | Out-Null
    Write-Host "Created $RelayPath"
}

# ── Copy exe ──────────────────────────────────────────────────────────────────
Copy-Item -Path $SourceExe -Destination $DestExe -Force
Write-Host "Copied $ExeName to $RelayPath"

# ── Copy wwwroot ──────────────────────────────────────────────────────────────
if (Test-Path $SourceWwwroot) {
    if (Test-Path $DestWwwroot) { Remove-Item $DestWwwroot -Recurse -Force }
    Copy-Item -Path $SourceWwwroot -Destination $DestWwwroot -Recurse -Force
    Write-Host "Copied wwwroot to $RelayPath"
} else {
    Write-Warning "wwwroot not found in source — web UI will not be updated."
}

# ── appsettings.json — preserve existing key on upgrade ──────────────────────
if ($isUpgrade -and (Test-Path $SettingsPath)) {
    Write-Host "Upgrade: preserving existing appsettings.json (API key unchanged)."
    if ($ApiKey) {
        # Caller explicitly passed a new key — update it
        $json = Get-Content $SettingsPath -Raw | ConvertFrom-Json
        $json.Relay.ApiKey = $ApiKey
        $json | ConvertTo-Json -Depth 10 | Set-Content $SettingsPath -Encoding UTF8
        Write-Host "API key updated in appsettings.json."
    }
} else {
    # Fresh install or settings file missing — write from source then set key
    if (Test-Path $SourceSettings) {
        Copy-Item -Path $SourceSettings -Destination $SettingsPath -Force
    }
    if ($ApiKey -and (Test-Path $SettingsPath)) {
        $json = Get-Content $SettingsPath -Raw | ConvertFrom-Json
        $json.Relay.ApiKey = $ApiKey
        $json | ConvertTo-Json -Depth 10 | Set-Content $SettingsPath -Encoding UTF8
        Write-Host "API key written to appsettings.json."
    }
}

if ($isUpgrade) {
    # Upgrade done — start service and exit
    Start-Service -Name $ServiceName
    $state = (Get-Service -Name $ServiceName).Status
    Write-Host ""
    Write-Host "Upgrade complete. Service status: $state" -ForegroundColor $(if ($state -eq 'Running') { 'Green' } else { 'Yellow' })
    if ($state -ne 'Running') { Write-Warning "Service did not reach Running state. Check Event Viewer." }
    exit 0
}

# ── Fresh install only: create service account ────────────────────────────────
$existing = Get-LocalUser -Name $ServiceAccount -ErrorAction SilentlyContinue
if (-not $existing) {
    $secPw = ConvertTo-SecureString $ServicePassword -AsPlainText -Force
    New-LocalUser -Name $ServiceAccount -Password $secPw `
                  -Description 'PowellTecRelay service account — no interactive logon' `
                  -PasswordNeverExpires -UserMayNotChangePassword | Out-Null
    Write-Host "Created local account: $ServiceAccount"
} else {
    Write-Host "Account $ServiceAccount already exists — updating password."
    $secPw = ConvertTo-SecureString $ServicePassword -AsPlainText -Force
    Set-LocalUser -Name $ServiceAccount -Password $secPw
}

# ── Grant "Log on as a service" right ────────────────────────────────────────
$sidStr  = (Get-LocalUser -Name $ServiceAccount).SID.Value
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

# ── ACL: read+execute only ────────────────────────────────────────────────────
$acl  = Get-Acl $RelayPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $ServiceAccount, 'ReadAndExecute', 'ContainerInherit,ObjectInherit', 'None', 'Allow'
)
$acl.AddAccessRule($rule)
Set-Acl $RelayPath $acl
Write-Host "ACL set: $ServiceAccount has ReadAndExecute on $RelayPath"

# ── Register Windows Service ──────────────────────────────────────────────────
$domainAccount = ".\$ServiceAccount"
sc.exe create $ServiceName binPath= $DestExe start= auto obj= $domainAccount password= $ServicePassword | Out-Null
sc.exe description $ServiceName "PowellTec IT Support Tools relay — web UI and PowerShell API on port 5000" | Out-Null
Write-Host "Registered service $ServiceName"

# ── Firewall rule ─────────────────────────────────────────────────────────────
$fwRule = Get-NetFirewallRule -DisplayName "$ServiceName Port 5000" -ErrorAction SilentlyContinue
if (-not $fwRule) {
    New-NetFirewallRule -DisplayName "$ServiceName Port 5000" `
        -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow | Out-Null
    Write-Host "Firewall rule added for port 5000"
} else {
    Write-Host "Firewall rule already exists — skipping."
}

# ── Start service ─────────────────────────────────────────────────────────────
Start-Service -Name $ServiceName
$state = (Get-Service -Name $ServiceName).Status
Write-Host ""
Write-Host "Install complete. Service status: $state" -ForegroundColor $(if ($state -eq 'Running') { 'Green' } else { 'Yellow' })
if ($state -eq 'Running') {
    Write-Host "PowellTecRelay is running at http://<thisserver>:5000" -ForegroundColor Green
} else {
    Write-Warning "Service did not reach Running state. Check Event Viewer for details."
}
