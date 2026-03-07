# Project Document & Telemetry Index

**Summary:** Canonical manifest of legal and telemetry docs for our Unity gacha RPG (AI assets used). Lists each document’s purpose, owner, filenames, formats, acceptance, versioning, retention, evidence, and CI checks.

## Required Documents

- **ToS/EULA:** App license and user rules. Owner: Dev. Files: `tos.md`/`tos.html`. Format: MD/HTML. Acceptance: Yes (clickwrap). Version: Semantic (keep history). Retention: Indefinite (retain old versions). Evidence: 3rd-party licenses, asset purchases. CI: hash-check on release.
- **Privacy Policy:** Data use/transfers. Owner: Dev. File: `privacy.html`. Format: HTML. Acceptance: Display only. Version: Semver. Retention: Indefinite. Evidence: Data maps, vendor DPAs. CI: validate existence.
- **Refund Policy:** Virtual goods refund rules. Owner: Dev. File: `refund.md`. Format: MD. Acceptance: N/A. Version: Change log. Retention: Until market launch + 1y. Evidence: Store invoices. CI: link check.
- **Third-Party Notices (Credits):** License attributions. Owner: Dev. File: `notices.md`/`notices.html`. Format: MD/HTML. Acceptance: N/A. Version: Updated with asset changes. Retention: Keep current. Evidence: License texts. CI: license validation.
- **AI-Use Disclosure:** Use of PixelLab/Gemini. Owner: Dev. File: `ai_disclosure.md`. Format: MD. Acceptance: N/A. Version: Single. Retention: Indefinite. Evidence: Tool ToS snapshots. CI: source check.
- **Age/Gating Policy:** Age limits (COPPA). Owner: Dev. File: `age_policy.md`. Format: MD. Acceptance: Display. Version: As needed. Retention: Indefinite. Evidence: COPPA compliance logs. CI: none.
- **Cookie/Tracking Notice:** Web privacy notice (if needed). Owner: Dev. File: `cookies.md`. Format: MD/HTML. Acceptance: N/A or consent pop-up. Version: As needed. Retention: Indefinite. Evidence: None. CI: none.
- **DMCA Takedown Procedure:** IP infringement contact. Owner: Dev. File: `dmca.md`. Format: MD. Acceptance: N/A. Version: Single. Retention: Indefinite. Evidence: Agent registration. CI: none.
- **Data Processing Addendum (DPA):** For B2B processors. Owner: Dev. File: `DPA.docx`. Format: DOCX. Acceptance: Counterparty. Version: Semver. Retention: As long as active contracts. Evidence: Signed DPAs. CI: none.
- **Telemetry Event Taxonomy:** List of events. Owner: Dev. File: `telemetry_taxonomy.md`. Format: MD. Acceptance: N/A. Version: Semver. Retention: Indefinite. Evidence: Data model docs. CI: schema validation.
- **JSON Schemas:** Schemas for each event. Owner: Dev. Files: `event_*.json` (e.g. `event_session_start.json`, `event_gacha_pull.json`). Format: JSON. Acceptance: N/A. Version: Semver. Retention: Archive old. Evidence: Version control. CI: JSON lint.
- **Server Audit Log Schema:** DB tables. Owner: Dev. File: `audit_schema.sql`. Format: SQL. Acceptance: N/A. Version: Semver. Retention: Archive by retention policy. Evidence: Migrations. CI: SQL syntax check.
- **Retention Policy:** Data retention by type. Owner: Dev. File: `retention.sql`. Format: SQL or MD. Acceptance: N/A. Version: As needed. Retention: Policy itself indefinite. Evidence: Legal citations. CI: none.
- **Consent API Spec:** Endpoints to log consent. Owner: Dev. File: `consent_api.json`. Format: JSON (OpenAPI). Acceptance: N/A. Version: Semver. Retention: Archive old. Evidence: API docs. CI: schema lint.
- **Provenance/Asset Ledger Spec:** Tracking AI asset origins. Owner: Dev. File: `provenance.md`. Format: MD. Acceptance: N/A. Version: As needed. Retention: Indefinite. Evidence: Prompt logs, hashes. CI: none.
- **CI Snapshot Workflow:** Automation for docs. Owner: DevOps. File: `ci-snapshot.yml`. Format: YAML. Acceptance: N/A. Version: Semver. Retention: Archive old. Evidence: CI logs. CI: on push/tags.

## Documents → Features

- **TOS/EULA:** Governs IAP/gacha, accounts, co-op rules, UGC conduct.  
- **Privacy:** Covers analytics, tracking, AI usage, cross-border data.  
- **Refund/Cookie:** IAP refunds (store policies), web analytics.  
- **Credits/DMCA:** Asset IP and UGC management.  
- **AI Disclosure:** Transparency on AI-generated content.  
- **Age/Gating:** Enforce 18+ for Gemini API; COPPA.  
- **Telemetry Docs:** All game analytics (sessions, purchases, match).  
- **Consent API/Provenance:** Legal logging of consents and AI asset origins.

## Metadata Fields

Recommended fields for all documents: `doc_id`, `title`, `owner`, `version`, `locale`, `published_at`, `effective_at`, `sha256`, `storage_uri`, `is_material_change`.

## Workflows

**CI Snapshot (Mermaid):** on `release-*` tag → fetch external TOS (PixelLab, Unity, etc) + our docs → save to `/legal/snapshots` + version commit.

**Consent Flow (Mermaid):** App launch → Show Privacy & TOS → User Accept/Reject → Log `consent_acceptance` event → Enable full features.

### Example CI snippet (GitHub Actions YAML)

```yaml
name: SnapshotLegal
on: push: tags: [ 'release-*' ]
jobs:
  snapshot:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Save Terms of Service
        run: curl -L "https://pixelab.ai/termsofservice" -o compliance/pixelab_tos.html
      - name: Save GPT Terms
        run: curl -L "https://ai.google.com/terms" -o compliance/gemini_terms.html
      - name: Commit snapshots
        run: git add compliance/* && git commit -m "Snapshot external legal" || echo "No changes"
```

## Example Output Files

- **Markdown:** `tos.md`, `privacy.md`, `provenance.md`, etc.  
- **HTML:** `privacy.html`, `notices.html`.  
- **JSON:** `event_session_start.json`, `consent_api.json`.  
- **SQL:** `audit_schema.sql`, `retention.sql`.  
- **C# Snippets:** `TelemetryManager.cs` (async batching).  
- **YAML:** `.github/workflows/ci-snapshot.yml`.  

**Assumptions:** Global release (GDPR/CCPA/COPPA apply); standard Apple/Google IAP; no user-generated artwork. All files version-controlled; acceptance logged.