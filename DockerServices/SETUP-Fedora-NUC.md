# Docker Services on a Fedora NUC — from blank machine

Goal: stand up Docker on a dedicated NUC, run **EmulatorJS** (fun, low-stakes — learn
Compose here first) and then **Guacamole** (the Android/Mac remote-desktop gateway).

Everything here is copy-paste. Do EmulatorJS first; Guacamole is the same pattern.

---

## 0. Install Fedora Server on the NUC
- Grab **Fedora Server** (not Workstation — headless box) from https://fedoraproject.org/server/
- Flash to USB (Fedora Media Writer, Rufus, or `dd`), boot the NUC from it, install.
- During install: set a hostname (e.g. `dockernuc`), create your user, enable SSH.
- After first boot, note its IP: `ip a` (look for the 192.168.x.x). Call it `<HOST-IP>` below.
- From your Windows box you can then SSH in: `ssh youruser@<HOST-IP>`

> Note: Fedora ships **Podman**, not Docker. We install real **Docker CE** below so every
> tutorial/command online matches exactly. Don't use the built-in `podman` for this.

---

## 1. Install Docker CE on Fedora
```bash
# Remove any podman/old docker bits that conflict
sudo dnf remove -y docker docker-client docker-common podman runc 2>/dev/null

# Add Docker's official repo
sudo dnf -y install dnf-plugins-core
sudo dnf config-manager --add-repo https://download.docker.com/linux/fedora/docker-ce.repo

# Install Docker Engine + Compose plugin
sudo dnf install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Start it and enable on boot
sudo systemctl enable --now docker

# Let your user run docker without sudo (log out/in after this)
sudo usermod -aG docker $USER
newgrp docker

# Verify
docker run --rm hello-world
```
If `hello-world` prints its greeting, Docker works. That's the hard part done.

---

## 2. EmulatorJS (do this one first)
```bash
mkdir -p ~/docker/emulatorjs && cd ~/docker/emulatorjs
# copy the emulatorjs/docker-compose.yml from this folder to here, then:
docker compose up -d
docker compose logs -f      # Ctrl-C to stop watching; container keeps running
```
- Management UI: `http://<HOST-IP>:3000`  → add a system, upload ROMs
- Play: `http://<HOST-IP>:80`
- ROMs also droppable directly into `~/docker/emulatorjs/data/<system>/`
- Edit `TZ=` in the compose file to your timezone first.

Once this works, you understand Compose. Guacamole is the same three commands.

---

## 3. Guacamole (the remote-desktop gateway)
```bash
mkdir -p ~/docker/guacamole && cd ~/docker/guacamole
# copy the guacamole/docker-compose.yml here first, then:

# 3a. Edit the compose file: change BOTH POSTGRES_PASSWORD lines to the same real password.

# 3b. Generate the DB schema (one time, BEFORE first up):
mkdir -p init
docker run --rm guacamole/guacamole:1.5.5 /opt/guacamole/bin/initdb.sh --postgresql > init/initdb.sql

# 3c. Start everything
docker compose up -d
docker compose logs -f
```
- Open `http://<HOST-IP>:8080/guacamole`
- First login: **guacadmin / guacadmin** — change this password IMMEDIATELY (top-right → Settings → Preferences).
- Add a connection: Settings → Connections → New Connection → protocol **RDP**, hostname = your Windows target, port 3389, and RDP username/password. Save.
- Click the connection → you're remoted in, in the browser. That URL is what the app's Remote Desktop page will show.

### Locking to a single target (the "one entry point" goal)
Guacamole connections live server-side. For the app we'll mint a **pre-authorized
session token** scoped to one connection via Guacamole's REST API, so the tablet only
ever gets a URL — never a host list or creds. (Design step for when we wire the app;
notes to follow. For now, a saved connection + direct URL is enough to test.)

---

## 4. Make it reachable off-network (later, do NOT skip security)
- Never port-forward 8080 raw to the internet.
- Preferred: **Tailscale** on the NUC + your devices — encrypted, zero config, free for personal use. The app then reaches `http://<tailscale-ip>:8080` from anywhere.
- See memory: project-remote-relay-idea (same encryption thread as the relay).

---

## Handy Docker commands
```bash
docker compose ps                 # what's running (from a compose folder)
docker compose logs -f <service>  # follow logs
docker compose down               # stop (data in ./ volumes persists)
docker compose pull && docker compose up -d   # update to newer images
docker ps                         # all running containers
docker system df                  # disk usage
```
