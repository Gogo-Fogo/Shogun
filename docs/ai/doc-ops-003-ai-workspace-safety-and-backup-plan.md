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

Still pending on this machine:

- Codex local config in `C:\Users\georg\.codex\config.toml` is still using:

```toml
[windows]
sandbox = "elevated"
```

That is too permissive for normal day-to-day project work.

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
- approval policy: conservative/on-request or equivalent guarded mode

Recommended special case:

- use read-only for repo review, research, and documentation sessions

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

Backup 2: local versioned backup on external drive

- use an external SSD or HDD
- run a nightly versioned backup of the repo and any other important local-only project material
- disconnect the external drive when the backup is not actively running

Backup 3: off-site versioned copy

- use a cloud or second-machine backup with version history
- do not rely on plain sync alone; versioned restore matters

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

Pending:

- harden Codex local config away from elevated mode
- add a simple repo backup script
- choose and test a nightly external-drive backup target
- choose an off-site versioned backup
- run one real restore drill

## Decision for now

For `Shogun`, the correct near-term setup is:

- repo-root-only AI sessions
- bounded Codex permissions
- Claude constrained by repo settings
- GitHub plus external-drive backup plus off-site versioned copy

Do not treat `.gitignore`, prompts, or GitHub alone as sufficient protection against local deletion.
