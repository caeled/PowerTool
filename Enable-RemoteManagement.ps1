#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Enables remote WMI, Computer Management, Event Viewer, and PowerShell remoting.
    Run this on BOTH machines (dev box and PowellServer).
#>

Write-Host "Enabling remote management firewall rules..." -ForegroundColor Cyan

# --- WMI (needed for System Info, Computer Management) ---
Write-Host "`n[WMI]" -ForegroundColor Yellow
Enable-NetFirewallRule -DisplayGroup "Windows Management Instrumentation (WMI)"
Write-Host "  WMI firewall rules enabled." -ForegroundColor Green

# --- DCOM (needed for MMC snap-ins like Computer Management, Event Viewer) ---
Write-Host "`n[DCOM / COM+]" -ForegroundColor Yellow
Enable-NetFirewallRule -DisplayName "COM+ Network Access (DCOM-In)" -ErrorAction SilentlyContinue
Enable-NetFirewallRule -DisplayGroup "COM+ Network Access" -ErrorAction SilentlyContinue
Write-Host "  DCOM firewall rules enabled." -ForegroundColor Green

# --- Remote Event Log (needed for Event Viewer remote) ---
Write-Host "`n[Remote Event Log]" -ForegroundColor Yellow
Enable-NetFirewallRule -DisplayGroup "Remote Event Log Management"
Write-Host "  Remote Event Log firewall rules enabled." -ForegroundColor Green

# --- Remote Service Management (bonus — useful for the tool) ---
Write-Host "`n[Remote Service Management]" -ForegroundColor Yellow
Enable-NetFirewallRule -DisplayGroup "Remote Service Management" -ErrorAction SilentlyContinue
Write-Host "  Remote Service Management firewall rules enabled." -ForegroundColor Green

# --- WinRM / PowerShell Remoting (needed for Run Remote Command) ---
Write-Host "`n[WinRM / PowerShell Remoting]" -ForegroundColor Yellow
Enable-PSRemoting -Force -SkipNetworkProfileCheck
Set-Item WSMan:\localhost\Client\TrustedHosts -Value "*" -Force
Write-Host "  WinRM enabled. TrustedHosts set to * (any machine)." -ForegroundColor Green

# --- Remote Registry (bonus) ---
Write-Host "`n[Remote Registry]" -ForegroundColor Yellow
Set-Service RemoteRegistry -StartupType Automatic
Start-Service RemoteRegistry -ErrorAction SilentlyContinue
Enable-NetFirewallRule -DisplayGroup "Remote Registry" -ErrorAction SilentlyContinue
Write-Host "  Remote Registry service started." -ForegroundColor Green

# --- Summary ---
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host " Done. Services enabled on this machine:" -ForegroundColor Cyan
Write-Host "   WMI            - System Info remote query" -ForegroundColor White
Write-Host "   DCOM/COM+      - Computer Management MMC" -ForegroundColor White
Write-Host "   Remote Evt Log - Event Viewer remote" -ForegroundColor White
Write-Host "   WinRM          - Run Remote Command (PS)" -ForegroundColor White
Write-Host "   Remote Registry- Registry browsing" -ForegroundColor White
Write-Host "`n Run this script as Administrator on BOTH machines." -ForegroundColor Yellow
Write-Host "================================================`n" -ForegroundColor Cyan
