# AI Workspace Safety and Backup Plan

**Summary:** Operational note for keeping AI tools scoped to this repository, reducing destructive local access, and maintaining recoverable backups beyond GitHub.

## Purpose

Use this document when the question is:

- how to prevent Codex or Claude from touching files outside `G:\Workspace\Unity\Projects\Shogun`
- how to reduce the risk of accidental AI-assisted deletion
- what backup strategy this repo should use beyond GitHub
- what to do first if files disappear

## Current repo state

Already in place:

- [`AGENTS.md`](../../AGENTS.md)
  - limits the maximum write scope to this repo
  - blocks destructive drive-root behavior by policy
- [`.claude/settings.json`](../../.claude/settings.json)
  - denies common destructive shell commands
  - denies `Read(../**)` and `Edit(../**)` to keep Claude inside the repo
- [`CLAUDE.md`](../../CLAUDE.md)
  - points Claude back to the repo rules

Now in place on this machine:

- Codex local config in `C:\Users\georg\.codex\config.toml` now uses:

```toml
approval_policy = "on-request"
sandbox_mode = "workspace-write"

[windows]
sandbox = "unelevated"
```

- Repo-local backup helper:
  - [`tools/backup/Backup-Shogun.ps1`](../../tools/backup/Backup-Shogun.ps1)
  - creates timestamped snapshots of the authored project data only
  - skips rebuildable Unity output by copying an allowlist instead of the whole repo root
- Point-in-time Git safety snapshot:
  - branch: `backup/local-safety-snapshot-2026-03-07`
  - purpose: preserve the specific dirty Unity/editor state that existed when the cleanup work was pushed
  - this is not a rolling backup branch and does not automatically capture later local changes

## Core rules

### Scope

- Open AI sessions against `G:\Workspace\Unity\Projects\Shogun`, not `G:\Workspace`, `G:\`, or a parent folder.
- Treat the repo root as the maximum allowed write scope.
- Do not rely on prompts alone to enforce scope. Keep guardrails in config and repo files.

### Permissions

- Default to bounded local access.
- Prefer read-only for research, review, and diagnostics.
- Prefer workspace-only writes for normal coding work.
- Avoid elevated or full-access modes except for a narrowly understood task that actually needs them.

### Backups

- GitHub is version control, not a complete backup strategy.
- `.gitignore` does not prevent deletion and does not protect files from an AI tool.
- Use a `3-2-1` backup model:
  - 3 copies of important data
  - 2 different media/storage types
  - 1 off-site or cloud versioned copy

## Recommended operating model

### Codex

Recommended default:

- sandbox: bounded workspace write
- approval policy: on-request
- Windows sandbox: unelevated

Recommended special case:

- use read-only for repo review, research, and documentation sessions
- easiest manual switch: `codex -s read-only`

Avoid as a default:

- elevated
- danger-full-access
- sessions started from a parent folder like `G:\Workspace`

### Claude

Recommended default:

- keep using repo-root sessions only
- keep [`.claude/settings.json`](../../.claude/settings.json) committed
- keep destructive shell deny rules and parent-directory read/edit deny rules enabled

### Backup layers

Backup 1: GitHub

- push at meaningful milestones
- prefer small, recoverable commits over large uncommitted diffs
- if there is important messy local state you do not want on `main`, preserve it on a clearly named safety branch before cleanup

Backup 2: local versioned backup on external drive

- use an external SSD or HDD
- run a nightly versioned backup of the repo and any other important local-only project material
- disconnect the external drive when the backup is not actively running
- selected local backup target on this machine: `E:\Backups\Shogun` on `TheBigOne` (`G-TECH ArmorATD`)
- repo helper command example:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup\Backup-Shogun.ps1 -DestinationRoot "E:\Backups\Shogun"
```

Backup 3: off-site versioned copy

- use a cloud or second-machine backup with version history
- do not rely on plain sync alone; versioned restore matters

## Recommended cadence

Do not treat backups as a weekly ritual only. Use both `GitHub` and `Backups` on different rhythms for different failure modes.

### GitHub cadence

- push when a meaningful unit of work is complete
- push at the end of the workday if there are intentional changes worth keeping
- do not let important work sit only locally for days just because the week is not over yet
- if you have several local commits that represent real progress, push them instead of waiting for a round number

Practical rule:

- for active development, expect at least one GitHub push on any day that produces meaningful repo changes

### Local backup cadence

- run the repo backup at least once per week even during a quiet period
- during active development, run it after major sessions or at least every 2 to 3 days
- run it immediately after risky or hard-to-recreate work, even if a recent backup already exists

Mandatory local backup triggers:

- before large refactors or repo reorganizations
- before Unity version, package, or render-pipeline changes
- after important scene, prefab, ScriptableObject, importer, or project-settings work
- after art/content sessions that produced local files not yet safely replicated elsewhere
- before deleting, archiving, or replacing large folders

Practical rule:

- if the answer to "would losing today's local state be painful?" is yes, run the backup now

### Off-site cadence

- let the off-site layer run automatically if possible
- if it is manual, refresh it at least weekly
- refresh it immediately after major milestones if the local backup is the only other copy

### Minimum operating rhythm for Shogun

Use this as the default:

- `GitHub`: at each meaningful milestone, and at the end of any productive day
- `E:\Backups\Shogun`: after risky sessions or every 2 to 3 active workdays, whichever comes first
- off-site versioned backup: automatic if available, otherwise weekly

## What should actually be backed up

Highest priority:

- `Assets/`
- `Packages/`
- `ProjectSettings/`
- `docs/`
- `tools/`
- root project files like `README.md`, `AGENTS.md`, `CLAUDE.md`, `IMPLEMENTATION_PROGRESS.md`
- all Unity `.meta` files

Lower priority or rebuildable:

- `Library/`
- `Temp/`
- `Logs/`
- `obj/`
- `/_Generated/`

## Recovery checklist

If files disappear or an AI tool starts deleting unexpectedly:

1. Stop the AI session immediately.
2. Stop any backup or sync tool that may propagate the deletion.
3. Confirm the exact scope of loss:
   - repo only
   - `G:\Workspace`
   - whole drive
4. Check the Recycle Bin and any cloud version history first.
5. Check `git status` and recent commit history to see what tracked files existed.
6. Restore from the fastest safe source:
   - local undo or recycle bin
   - Git history for tracked files
   - external-drive versioned backup
   - off-site versioned backup
7. Do not run cleanup commands while the scope is still unclear.
8. After restore, tighten the AI session scope and sandbox before resuming work.

## Current status summary

Done:

- repo-scoped safety rules in [`AGENTS.md`](../../AGENTS.md)
- Claude repo-scoped deny rules in [`.claude/settings.json`](../../.claude/settings.json)
- documented MCP and exporter workflow so blind bulk exports are no longer the default
- Codex local config settled on `workspace-write` + `on-request` + Windows `unelevated`
- repo backup script added
- one point-in-time Git safety snapshot branch created for the March 7 local Unity/editor state

Pending:

- choose an off-site versioned backup
- run one real restore drill

## Decision for now

For `Shogun`, the correct near-term setup is:

- repo-root-only AI sessions
- bounded Codex permissions
- Claude constrained by repo settings
- GitHub plus external-drive backup plus off-site versioned copy

Do not treat `.gitignore`, prompts, or GitHub alone as sufficient protection against local deletion.
