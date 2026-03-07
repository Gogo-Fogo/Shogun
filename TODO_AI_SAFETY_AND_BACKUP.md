# TODO: AI Safety and Backup

Working checklist for reducing AI-assisted file-loss risk on `Shogun`.

## Already done

- [x] Add repo safety rules in [AGENTS.md](./AGENTS.md)
- [x] Add Claude repo guardrails in [.claude/settings.json](./.claude/settings.json)
- [x] Add Claude compatibility pointer in [CLAUDE.md](./CLAUDE.md)
- [x] Document the plan in [docs/ai/doc-ops-003-ai-workspace-safety-and-backup-plan.md](./docs/ai/doc-ops-003-ai-workspace-safety-and-backup-plan.md)
- [x] Add a recovery checklist to the repo docs

## Pending

- [ ] Harden Codex local config in `C:\Users\georg\.codex\config.toml`
  - Goal: stop using `[windows] sandbox = "elevated"` as the default.
  - Target: bounded workspace-write for normal coding, read-only for review/research.
- [ ] Add a simple backup script for this repo
  - Goal: copy the important project folders to a chosen backup target without `Library/`, `Temp/`, `Logs/`, or `/_Generated/`.
- [ ] Choose a nightly external-drive backup target
  - Goal: versioned backup on an SSD/HDD that is disconnected when not actively backing up.
- [ ] Choose an off-site versioned backup
  - Goal: restore-capable cloud or second-machine copy, not plain sync only.
- [ ] Run one restore drill
  - Goal: prove that at least one deleted file or folder can be recovered cleanly.

## Not action items

- [x] Keep `.gitignore` focused on version control hygiene only
  - `.gitignore` does not prevent local deletion and should not be treated as a safety control.
- [x] Keep GitHub as one backup layer, not the only one
