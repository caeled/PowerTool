<#
.SYNOPSIS
    Assist Docker Desktop setup on a Windows host: enable WSL2 and download the
    Docker Desktop installer. Does NOT silently install or reboot — it prepares
    the pieces and tells you the interactive steps.

.NOTES
    Run in an ELEVATED PowerShell (Run as Administrator).
    Interim Windows host for DockerServices; Fedora is the eventual home.
#>

[CmdletBinding()]
param(
    [string]$DownloadDir = "$env:USERPROFILE\Downloads"
)

function Write-Step($n, $msg) { Write-Host "`n[$n] $msg" -ForegroundColor Cyan }

# Must be admin
$isAdmin = ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Warning "Please re-run this in an elevated PowerShell (Run as Administrator)."
    return
}

Write-Step 1 "Checking WSL..."
$wslOk = $false
try {
    $null = wsl.exe --status 2>$null
    if ($LASTEXITCODE -eq 0) { $wslOk = $true }
} catch { }

if ($wslOk) {
    Write-Host "    WSL is present. Ensuring default version is 2..." -ForegroundColor Green
    wsl.exe --set-default-version 2 | Out-Null
} else {
    Write-Host "    WSL not detected. Installing (this may require a reboot)..." -ForegroundColor Yellow
    Write-Host "    Running: wsl --install" -ForegroundColor DarkGray
    wsl.exe --install
    Write-Host "    If Windows asks you to reboot, DO SO, then run this script again." -ForegroundColor Yellow
}

Write-Step 2 "Fetching Docker Desktop installer..."
$installer = Join-Path $DownloadDir "DockerDesktopInstaller.exe"
$url = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
if (Test-Path $installer) {
    Write-Host "    Already downloaded: $installer" -ForegroundColor Green
} else {
    try {
        Write-Host "    Downloading to $installer ..." -ForegroundColor DarkGray
        Invoke-WebRequest -Uri $url -OutFile $installer -UseBasicParsing
        Write-Host "    Done." -ForegroundColor Green
    } catch {
        Write-Warning "    Download failed: $($_.Exception.Message)"
        Write-Host "    Get it manually: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
    }
}

Write-Step 3 "Next steps (interactive — I can't do these for you):"
Write-Host @"
    1. Run the installer:  $installer
       - Keep 'Use WSL 2 instead of Hyper-V' CHECKED.
    2. Reboot if prompted, then launch Docker Desktop and accept the terms.
    3. Verify in a normal PowerShell:  docker run --rm hello-world
    4. Then follow SETUP-Windows-Docker.md from step 4 (EmulatorJS).
"@ -ForegroundColor Gray
