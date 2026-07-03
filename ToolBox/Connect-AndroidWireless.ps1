# Connect-AndroidWireless.ps1 — pair + connect adb wireless debugging via a small form
# Usage: right-click > Run with PowerShell, or:  powershell -Sta -ExecutionPolicy Bypass -File .\Connect-AndroidWireless.ps1
#
# On the tablet first: Settings > Developer Options > Wireless debugging > ON
#   - "Pair device with pairing code" shows the 6-digit CODE and the PAIRING port
#   - The Wireless debugging screen itself shows the MAIN (connect) port
# Ports change every session; IP usually stays the same.

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$adb = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"
$cfgPath = Join-Path $env:APPDATA "PowellTec\android-wireless.json"

# Remember the last IP between runs
$lastIp = "192.168.1."
if (Test-Path $cfgPath) {
    try { $lastIp = (Get-Content $cfgPath -Raw | ConvertFrom-Json).Ip } catch {}
}

function Invoke-Adb {
    param([string[]]$AdbArgs)
    if (-not (Test-Path $adb)) { return "adb.exe not found at:`r`n$adb" }
    try {
        $out = & $adb @AdbArgs 2>&1 | Out-String
        return $out.Trim()
    } catch { return "ERROR: $($_.Exception.Message)" }
}

# ── Form ──────────────────────────────────────────────────────────────────────
$form                 = New-Object System.Windows.Forms.Form
$form.Text            = "Android Wireless Connect"
$form.Size            = New-Object System.Drawing.Size(430, 400)
$form.StartPosition   = "CenterScreen"
$form.FormBorderStyle = "FixedDialog"
$form.MaximizeBox     = $false

function Add-LabeledBox([string]$label, [int]$y, [string]$default) {
    $l = New-Object System.Windows.Forms.Label
    $l.Text = $label; $l.Location = New-Object System.Drawing.Point(12, $y)
    $l.Size = New-Object System.Drawing.Size(120, 20)
    $form.Controls.Add($l)
    $t = New-Object System.Windows.Forms.TextBox
    $t.Location = New-Object System.Drawing.Point(140, ($y - 3))
    $t.Size = New-Object System.Drawing.Size(260, 22)
    $t.Text = $default
    $form.Controls.Add($t)
    return $t
}

$txtIp   = Add-LabeledBox "Tablet IP:"        20  $lastIp
$txtPPort= Add-LabeledBox "Pairing port:"     50  ""
$txtCode = Add-LabeledBox "Pairing code:"     80  ""
$txtMPort= Add-LabeledBox "Main port:"        110 ""

$out = New-Object System.Windows.Forms.TextBox
$out.Multiline = $true; $out.ReadOnly = $true; $out.ScrollBars = "Vertical"
$out.Location = New-Object System.Drawing.Point(12, 180)
$out.Size = New-Object System.Drawing.Size(390, 170)
$out.Font = New-Object System.Drawing.Font("Consolas", 9)
$form.Controls.Add($out)

function Write-Out([string]$msg) {
    $out.AppendText($msg + "`r`n")
}

$btnGo = New-Object System.Windows.Forms.Button
$btnGo.Text = "Pair + Connect"
$btnGo.Location = New-Object System.Drawing.Point(140, 142)
$btnGo.Size = New-Object System.Drawing.Size(130, 28)
$btnGo.BackColor = [System.Drawing.Color]::FromArgb(0,120,212)
$btnGo.ForeColor = [System.Drawing.Color]::White
$form.Controls.Add($btnGo)

$btnFix = New-Object System.Windows.Forms.Button
$btnFix.Text = "Fix (restart adb)"
$btnFix.Location = New-Object System.Drawing.Point(280, 142)
$btnFix.Size = New-Object System.Drawing.Size(120, 28)
$form.Controls.Add($btnFix)

$btnGo.Add_Click({
    $out.Clear()
    $ip = $txtIp.Text.Trim()
    if (-not $ip) { Write-Out "Enter the tablet IP."; return }

    # Save IP for next time
    New-Item -ItemType Directory -Force -Path (Split-Path $cfgPath) | Out-Null
    @{ Ip = $ip } | ConvertTo-Json | Set-Content $cfgPath

    # Pair — only if pairing fields are filled (already-paired devices can skip)
    if ($txtPPort.Text.Trim() -and $txtCode.Text.Trim()) {
        Write-Out ">> adb pair $ip`:$($txtPPort.Text.Trim()) $($txtCode.Text.Trim())"
        Write-Out (Invoke-Adb @("pair", "$ip`:$($txtPPort.Text.Trim())", $txtCode.Text.Trim()))
        Write-Out ""
    } else {
        Write-Out "(skipping pair — no pairing port/code entered)"
    }

    # Connect
    if ($txtMPort.Text.Trim()) {
        Write-Out ">> adb connect $ip`:$($txtMPort.Text.Trim())"
        Write-Out (Invoke-Adb @("connect", "$ip`:$($txtMPort.Text.Trim())"))
        Write-Out ""
        Write-Out ">> adb devices"
        Write-Out (Invoke-Adb @("devices"))
        Write-Out ""
        Write-Out "If connected, the tablet appears in Visual Studio shortly (can take a minute)."
    } else {
        Write-Out "Enter the main port to connect."
    }
})

$btnFix.Add_Click({
    $out.Clear()
    Write-Out ">> adb kill-server"
    Write-Out (Invoke-Adb @("kill-server"))
    Write-Out ">> adb start-server"
    Write-Out (Invoke-Adb @("start-server"))
    Write-Out ""
    Write-Out "Now toggle Wireless debugging OFF/ON on the tablet for fresh ports, then Pair + Connect."
})

[void]$form.ShowDialog()
