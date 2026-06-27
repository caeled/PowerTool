$udp = New-Object System.Net.Sockets.UdpClient(9)
Write-Host 'Listening on UDP port 9 for magic packet...' -ForegroundColor Cyan
$ep = New-Object System.Net.IPEndPoint([System.Net.IPAddress]::Any, 0)
$bytes = $udp.Receive([ref]$ep)
$udp.Close()
Write-Host "Packet received from $($ep.Address) - $($bytes.Length) bytes" -ForegroundColor Green
$hex = ($bytes | ForEach-Object { $_.ToString('X2') }) -join ' '
Write-Host $hex
$firstSix = $bytes[0..5] | Where-Object { $_ -ne 0xFF }
if ($bytes.Length -eq 102 -and $firstSix.Count -eq 0) {
    Write-Host 'VALID magic packet!' -ForegroundColor Green
} else {
    Write-Host 'Not a standard magic packet' -ForegroundColor Red
}
Read-Host 'Press Enter to close'
