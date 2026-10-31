# PowellTec IT Support Tools — AI Collaborators

This file is the shared collaboration contract for every AI system working in this
repository (Muse, ChatGPT, Copilot, and future systems). It replaces the former
CLAUDE.md. The workflow guidance is model-independent: whoever you are, the rules
below are the rules.

## Mission and Vision

PowerTool (PowellTec IT Support Tools) is a cross-platform collection of IT support
utilities for working technicians: remote administration, diagnostics, file
operations, and remote-desktop access that run from a desktop app, a phone, a
browser, or a portable USB stick. It is the operational-infrastructure half of a
shared ecosystem; Everyday Cool is the educational-content half. Both projects use
the same collaboration standards so any AI collaborator can pick up either one.

## Project Principles

- Built for the field: tools must work under real support conditions, including
  offline and portable use.
- Consistency across sessions is critical (see Consistency, below).
- Decisions are written down here, not left in chat history.
- Behavior is documented alongside implementation, so a change in behavior is a
  visible, reviewable event.

## User Needs

- Steve has ADHD/spectrum — consistency across sessions is critical. If a behavior
  changes after context compaction, it is a regression. When Steve flags it, add it
  here and to memory so it doesn't happen again.
- Tools are used by a 30-year IT support veteran and shared with other technicians;
  they must be dependable without babysitting.
- Support discussion: https://groups.google.com/d/forum/powelltooltalk

## Repository Architecture

- `SupportToolApp/` — .NET MAUI support app (Android builds via CI).
- `ToolsAvaloniaApp/` — Avalonia desktop app (Windows/macOS/Linux).
- `CompareApp/` — file/folder comparison utility.
- `SoundCombineApp/` — audio combining utility.
- `PowellTecRelay/` — web/PWA relay component; the Avalonia CI artifact includes the
  full relay publish folder (wwwroot is required, not optional).
- `ToolBox/` — PowerShell remote-administration toolbox: remote directory browsing,
  process management, WinRM toggling, Wake on LAN, Android wireless ADB.
- `DockerServices/` — Docker Compose setups (EmulatorJS, Guacamole HTML5 remote
  desktop); Fedora host path, with a Windows Docker host setup as interim.
- `Personal/` — personal scripts and data; treat contents as private by default.
- Root utilities: `Enable-RemoteManagement.ps1` (WMI/WinRM/DCOM enablement),
  `wol_listen.ps1` (Wake on LAN listener), `BUILD-PORTABLE.cmd` (USB portable
  publish), `com.powelltec.PowellTecTools.desktop` and `com.powelltec.tools.yml`
  (Linux/Flatpak registration).
- Stack: C#/.NET (majority), PowerShell, HTML. CI: GitHub Actions workflows for
  Android, Avalonia, and Mac, plus an artifact-cleanup workflow.

## Git Workflow

After every commit, always:

1. `git push origin master`
2. `gh run list --repo caeled/PowerTool --limit=3` to find the relevant run
3. Post the direct link: `https://github.com/caeled/PowerTool/actions/runs/<id>`

Never ask for permission to push. Never stop at the commit and wait.

## Documentation Standards

- Architecture rationale lives alongside implementation details in this file and in
  the project folders it describes.
- Expected behavior and known regressions are documented explicitly (see Review
  Checklist and the Decisions Log), never left implicit in code alone.
- `TODO.md` is the backlog; shipped items are removed from it and notable ones are
  recorded in the Decisions Log.

## Accessibility Standards

- Interfaces must remain usable by keyboard and readable at a glance under field
  conditions; do not trade clarity for decoration.
- Changes that alter interaction patterns count as behavior changes and follow the
  Consistency rule.

## Provenance Rules

- AI collaboration is disclosed, never hidden. Work done by an AI collaborator is
  committed under the same standards as any other work.
- Every consequential choice gets a Decisions Log entry: what changed, why, and
  what it affects. Future collaborators read the log before re-litigating a
  settled choice.

## Review Checklist

- [ ] Builds pass in GitHub Actions; run link posted (see Git Workflow).
- [ ] Behavior changes are intentional, documented, and checked against the
      Consistency rule.
- [ ] Decisions Log updated for any settled question or behavior change.
- [ ] `TODO.md` updated: completed items removed, new follow-ups added.
- [ ] No build output, session data, or secrets committed (see `.gitignore`).

## Historical Decisions Log

- 2026-07-01 — Actions storage quota hit 100% (0.5 GB free tier). Added
  cleanup-artifacts workflow: keep the newest 3 artifacts per name, delete older;
  runs after each build and daily.
- 2026-07-01 — CLAUDE.md established the git workflow (push after every commit,
  post the Actions run link, never stop at the commit) and the consistency rule.
- 2026-07-04 — Windows Docker host setup added as an interim alongside the Fedora
  path for DockerServices.
- 2026-10-31 — CLAUDE.md converted to this model-neutral AI-COLLABORATORS.md, per
  the Everyday Cool / PowerTool recommendations; workflow and consistency rules
  carried over unchanged. A README.md remains to be created (the repo has never
  had one).

## Known Future Goals

- v1.1 (from TODO.md): PWA file-copy tool needs browse buttons — P1 (source) and
  P2 (destination) file/folder pickers via the Browser File System Access API or a
  typed UNC path helper; the desktop app already has native pickers via
  StorageProvider.
- Create a README.md with a project overview (GitHub currently shows none).
- Shared ecosystem: PowerTool as operational infrastructure, Everyday Cool as the
  educational platform, one collaboration standard (this file's counterpart lives
  in the everyday-cool repository) for Muse, ChatGPT, Copilot, and future systems.
