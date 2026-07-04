# Docker Services on Windows (interim host) — Docker Desktop + WSL2

Goal: run **EmulatorJS** and **Guacamole** on the Windows NUC now, using Docker
Desktop. The `docker-compose.yml` files are Linux containers and run unchanged
here (WSL2 backend) and later on Fedora — so this host is fully throwaway.

> Migrating to Fedora later = copy the `DockerServices` folder to the Fedora box
> and `docker compose up -d`. Same files, no edits. See `SETUP-Fedora-NUC.md`.

---

## 0. Requirements
- Windows 10 (2004+) or Windows 11, 64-bit.
- CPU virtualization enabled in BIOS/UEFI (Intel VT-x / AMD-V). Most NUCs: on by default. If Docker later complains about virtualization, that's the BIOS toggle.
- ~8 GB free disk to start.

## 1. Enable WSL2 (one command, one reboot)
Open **PowerShell as Administrator**:
```powershell
wsl --install
```
This enables WSL2 + the VM platform and installs a default Linux distro. **Reboot when it asks.**
(If WSL is already present: `wsl --set-default-version 2`.)

## 2. Install Docker Desktop
- Download: https://www.docker.com/products/docker-desktop/  (or run `Install-DockerPrereqs.ps1` in this folder, which fetches the installer for you).
- Run the installer. When asked, keep **"Use WSL 2 instead of Hyper-V"** checked.
- Reboot if prompted, launch **Docker Desktop**, accept the terms, wait for the whale icon in the tray to go steady.

## 3. Verify Docker works
In PowerShell:
```powershell
docker run --rm hello-world
```
Greeting prints = you're done with the hard part.

---

## 4. EmulatorJS (do this first — low stakes, learn Compose)
Copy the `emulatorjs` folder somewhere permanent, e.g. `C:\DockerServices\emulatorjs`, then:
```powershell
cd C:\DockerServices\emulatorjs
docker compose up -d
docker compose logs -f      # Ctrl-C stops watching, container keeps running
```
- Management UI: `http://localhost:3000`  → add a system, upload ROMs
- Play: `http://localhost:80`
- From other devices on the LAN, swap `localhost` for the NUC's IP.
- Edit `TZ=` in the compose file to your timezone first.
- The relative `./config` and `./data` volumes just work on Windows — Docker maps them into the folder.

## 5. Guacamole (remote desktop gateway)
Copy the `guacamole` folder to e.g. `C:\DockerServices\guacamole`, then:

```powershell
cd C:\DockerServices\guacamole

# 5a. Edit docker-compose.yml: set BOTH POSTGRES_PASSWORD lines to the same real password.

# 5b. Generate the DB schema — ONE TIME, before first `up`.
#     WINDOWS GOTCHA: PowerShell's `>` writes UTF-16, which Postgres CANNOT read and
#     the init will silently fail. Force ASCII/UTF-8 with no BOM like this:
New-Item -ItemType Directory -Force -Path init | Out-Null
docker run --rm guacamole/guacamole:1.5.5 /opt/guacamole/bin/initdb.sh --postgresql `
  | Out-File -FilePath init\initdb.sql -Encoding ascii

# 5c. Start it
docker compose up -d
docker compose logs -f
```
- Open `http://localhost:8080/guacamole`
- First login **guacadmin / guacadmin** — change it immediately (top-right → Settings → Preferences).
- Add a connection: Settings → Connections → New → protocol **RDP**, hostname = a Windows target, port 3389, RDP creds. Save, click it, you're remoted in through the browser. That URL is what the app's Remote Desktop page shows.

> If `initdb.sql` looks empty or Postgres logs show schema errors, you hit the UTF-16
> gotcha — delete `data\postgres`, regenerate 5b with `-Encoding ascii`, and `up` again.

---

## 6. Keep it running (the one real Windows limitation)
Docker Desktop needs a **logged-in Windows user session** — unlike Fedora, where Docker
runs headless as a systemd service. For a NUC acting as an always-on server:
- Docker Desktop → Settings → General → **Start Docker Desktop when you sign in**.
- Set the NUC to auto-login that user, and don't sign out (lock is fine).
- This is workable but not ideal — it's the main reason Fedora is the eventual home. On Fedora the containers come up on boot with no session at all.

## 7. Reaching it off-network (later — do NOT skip security)
- Never port-forward these to the internet raw.
- Preferred: **Tailscale** on the NUC + your devices — encrypted, free for personal use; the app then reaches `http://<tailscale-ip>:8080`.
- See memory: project-remote-relay-idea.

## Handy commands (PowerShell, from a compose folder)
```powershell
docker compose ps                 # what's running
docker compose logs -f            # follow logs
docker compose down               # stop (./ volumes persist)
docker compose pull; docker compose up -d   # update images
docker ps                         # all containers
```
