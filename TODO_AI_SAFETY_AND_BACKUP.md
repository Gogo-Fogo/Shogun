# TODO: AI Safety and Backup

Working checklist for reducing AI-assisted file-loss risk on `Shogun`.

## Already done

- [x] Add repo safety rules in [AGENTS.md](./AGENTS.md)
- [x] Add Claude repo guardrails in [.claude/settings.json](./.claude/settings.json)
- [x] Add Claude compatibility pointer in [CLAUDE.md](./CLAUDE.md)
- [x] Document the plan in [docs/ai/doc-ops-003-ai-workspace-safety-and-backup-plan.md](./docs/ai/doc-ops-003-ai-workspace-safety-and-backup-plan.md)
- [x] Add a recovery checklist to the repo docs

## Pending

- [x] Harden Codex local config in `C:\Users\georg\.codex\config.toml`
  - Default now uses `approval_policy = "on-request"` and `sandbox_mode = "workspace-write"`.
  - Windows sandbox is set to `unelevated`.
  - Use `codex -s read-only` for review/research sessions.
- [x] Add a simple backup script for this repo
  - Script: [`tools/backup/Backup-Shogun.ps1`](./tools/backup/Backup-Shogun.ps1)
  - Behavior: writes timestamped snapshots of the important repo folders without `Library/`, `Temp/`, `Logs/`, `obj/`, or `/_Generated/`.
- [x] Preserve the pre-cleanup local Unity/editor state in Git
  - Branch: `backup/local-safety-snapshot-2026-03-07`
  - Scope: point-in-time safety snapshot only, not an ongoing catch-all backup.
- [x] Choose a nightly external-drive backup target
  - Selected target: `E:\Backups\Shogun` on `TheBigOne` (`G-TECH ArmorATD`)
  - Goal: versioned backup on an SSD/HDD that is disconnected when not actively backing up.
- [ ] Choose an off-site versioned backup
  - Goal: restore-capable cloud or second-machine copy, not plain sync only.
- [ ] Run one restore drill
  - Goal: prove that at least one deleted file or folder can be recovered cleanly.

## Not action items

- [x] Keep `.gitignore` focused on version control hygiene only
  - `.gitignore` does not prevent local deletion and should not be treated as a safety control.
- [x] Keep GitHub as one backup layer, not the only one

## Routine cadence

- [x] Use GitHub for milestone protection, not weekly batching only
  - Push at each meaningful milestone and at the end of any productive day with intentional changes.
- [x] Use the local external-drive backup for disaster recovery, not every tiny commit
  - Run `E:\Backups\Shogun` backups at least weekly during quiet periods.
  - During active work, run them after risky sessions or every 2 to 3 active workdays.
- [x] Treat these as mandatory backup triggers
  - Before large refactors or cleanup.
  - Before Unity/package/render-pipeline changes.
  - After important scene, prefab, ScriptableObject, importer, or project-settings work.
  - Before deleting or replacing large folders.
