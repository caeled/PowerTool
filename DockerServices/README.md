# DockerServices

Container configs + setup for the Docker host. The compose files are Linux
containers and run **unchanged** on either host, so the interim/permanent split
costs nothing:

- **Now (interim):** Windows NUC via Docker Desktop + WSL2 — `SETUP-Windows-Docker.md`
  (helper: `Install-DockerPrereqs.ps1`).
- **Later (permanent):** dedicated Fedora NUC, headless — `SETUP-Fedora-NUC.md`.
- **Migration:** copy this whole folder to the new host, `docker compose up -d`. No edits.

Two services:

| Folder        | What it is                                   | Purpose |
|---------------|----------------------------------------------|---------|
| `emulatorjs/` | Browser-based retro game server (LinuxServer)| Fun + learn Docker Compose here first |
| `guacamole/`  | HTML5 remote desktop gateway (RDP/VNC/SSH)   | The Android/Mac remote-desktop + interactive-shell answer, shown in-app via WebView |

## Order of operations
1. Put Fedora Server on the NUC, install Docker CE — `SETUP-Fedora-NUC.md` steps 0–1.
2. Bring up **EmulatorJS** first (low stakes) — proves Docker works, teaches Compose.
3. Bring up **Guacamole** — same pattern, plus a one-time DB-schema generate step.
4. In the app, the **Remote Desktop** page shows the Guacamole session in a WebView
   (URL set in Settings → Relay/Remote section).

## Why this exists
Closes the two mobile gaps the relay can't:
- **RDP** — the `mstsc` tool is Windows-only (disabled on Android/Mac). Guacamole renders RDP in the browser instead.
- **Interactive shell** — `Enter-PSSession` console is Windows-only (disabled on non-Windows). Guacamole's SSH/console connection covers it. The relay still handles non-interactive PowerShell `/run`.

See memory: `project-guacamole-remote`, `project-remote-relay-idea`.

## Security reminders before ANY off-network exposure
- Change Guacamole's default `guacadmin/guacadmin` immediately.
- Change the Postgres password in the compose file before first run.
- Never port-forward these raw to the internet — front with Tailscale/WireGuard.
