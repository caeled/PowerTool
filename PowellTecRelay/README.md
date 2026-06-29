# PowellTecRelay

ASP.NET Core Windows Service that exposes the PowellTec IT Support Tools as an HTTP API and serves the web UI. Runs on port 5000.

## Installation

Use `Install-RelayService.ps1` — it handles everything in one shot:

```powershell
.\Install-RelayService.ps1 -ServicePassword 'YourStrongPass!' -ApiKey 'your-real-api-key'
```

> **Security note — do not run this service as SYSTEM or a local administrator.**
>
> The relay exposes a PowerShell execution endpoint over HTTP. If the service runs as an
> admin account, anyone who obtains the API key can run arbitrary PowerShell with full
> machine privileges. The install script creates a dedicated `svc_relay` account with no
> admin rights and grants it only what the service needs (read/execute on its own folder,
> log on as a service). A compromised API key under `svc_relay` has limited reach;
> a compromised API key under SYSTEM owns the box.

The script will:
1. Create a `svc_relay` local account with no group memberships
2. Grant it `Log on as a service` only
3. Set `ReadAndExecute` ACL on the relay folder — no write access
4. Write your API key into `appsettings.json`
5. Install (or reconfigure) the Windows Service
6. Open firewall port 5000
7. Start the service

## Updating

Stop the service, overwrite `PowellTecRelay.exe` and `wwwroot\`, then start the service again. The `appsettings.json` (and your API key) are preserved — don't overwrite it.

## API

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/ping` | None | Health check |
| GET | `/` | None | Web UI |
| POST | `/run` | `X-Api-Key` header | Run a PowerShell script |

### POST /run

```json
{ "Script": "Get-Date" }
```

Response:
```json
{ "output": "Sunday, June 29, 2026 ...", "success": true }
```

## Changing the API key

Edit `C:\PowellTecRelay\appsettings.json` and restart the service:

```json
{ "Relay": { "ApiKey": "your-new-key" } }
```
