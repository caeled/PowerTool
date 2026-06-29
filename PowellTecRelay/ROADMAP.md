# PowellTecRelay — Roadmap & Known Limitations

This document is part of the repository intentionally. Honest accounting of what
is production-ready, what is known-incomplete, and what is planned next.

---

## Current State (v1.0)

Functional IT support relay service with:
- ASP.NET Core Windows Service on port 5000
- 26 IT tools exposed via PowerShell over HTTP
- Dark-theme web UI (PWA-installable on Android)
- API key authentication on all write endpoints
- Dedicated low-privilege service account (`svc_relay`)
- Remote execution via `Invoke-Command` with user-supplied credentials
- Tested across Windows, macOS (arm64 + x64), and Linux (Flatpak)

---

## Known Limitations Being Addressed

### 1. Transport Security (HTTPS)
**Current:** HTTP only. Credentials, API key, and command output cross the wire in plaintext.  
**Risk:** Anyone on the same network segment can intercept credentials via packet capture.  
**Planned:** TLS via Kestrel certificate binding or a reverse proxy (nginx / Caddy) in front of the relay.

### 2. Static API Key
**Current:** Single shared API key with no expiry, stored in `appsettings.json`.  
**Risk:** No rotation, no per-user accountability, no revocation without a service restart.  
**Planned:** Key rotation support; optionally integrate with Windows Credential Manager or a secrets store.

### 3. No Audit Logging
**Current:** Commands execute and output returns — no record of who ran what, when, or from where.  
**Risk:** No forensic trail if the relay is misused or compromised.  
**Planned:** Structured log per execution: timestamp, source IP, target computer, script hash, success/failure. Written to Windows Event Log and/or a log file with rotation.

### 4. Unrestricted Command Surface
**Current:** The relay accepts and forwards arbitrary PowerShell to `Invoke-Command`. The web UI limits what users *see*, but the API enforces nothing.  
**Risk:** An authenticated caller can POST any script — including destructive or exfiltrating commands.  
**Planned:** Server-side command allowlist (SHA256 hash of approved scripts); anything not on the list is rejected before execution.

### 5. Loopback WinRM Limitation
**Current:** Targeting the relay host itself via the web UI fails due to Windows' loopback WinRM restriction.  
**Planned:** Detect loopback and run directly (no `Invoke-Command` wrapper) when target == relay host.

---

## Planned Features (v1.1+)

| Feature | Notes |
|---------|-------|
| HTTPS / TLS | Kestrel cert or Caddy reverse proxy |
| Audit log viewer | In-page log tab showing recent executions |
| Per-key scoping | Different API keys allowed different tool subsets |
| Command allowlist | Hash-verified script whitelist enforced server-side |
| Session tokens | Short-lived tokens issued after key auth; key not sent on every request |
| Tool result caching | Cache read-only tool output for configurable TTL |
| Android PWA polish | Offline mode, push notifications for long-running commands |

---

## Architecture Notes

The relay is intentionally simple — one process, one port, no database. That simplicity
is a feature for a self-hosted IT tool running on trusted infrastructure. The hardening
items above do not require redesigning the architecture; they layer on top of what exists.

The PowerShell execution core (`PsRunner.cs`) is shared between the Avalonia desktop app
and this relay via a linked source file — one fix propagates to both.
