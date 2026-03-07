# Shogun: Flowers Fall in Blood Documents

This is the canonical documentation root for the repository.

This folder is organized for two audiences:

- `ai/` contains the AI-first set as Markdown companions, plus the retained original `.docx` for `DOC-REF-004` and any Markdown-only internal engineering notes.
- `recruiter/` contains the recruiter-ready PDFs with matching canonical filenames.

Supporting file:

- `PROJECT_CONTEXT_INDEX.md` is the routing file. Read it first.

## Canonical naming

All files use stable document IDs in the filename:

- `doc-gdd-*` for product design
- `doc-eng-*` for engineering
- `doc-data-*` for telemetry and data collection
- `doc-ops-*` for implementation manifests
- `doc-legal-*` for legal and compliance
- `doc-ref-*` for comparative/reference documents

Markdown and PDF companions share the same base filename whenever both versions exist.

## Current state

- Every recruiter PDF now has a matching Markdown companion in `ai/`.
- `DOC-REF-004` keeps both the original `.docx` and an AI-friendly `.md` companion in `ai/`.
- Some canonical engineering notes may exist only in `ai/` when no recruiter PDF is needed.
- Internal operational notes like Unity MCP workflow guidance may exist only in `ai/` when no recruiter PDF is needed.

## Suggested usage

1. Start with `PROJECT_CONTEXT_INDEX.md`.
2. Open the relevant file in `ai/`.
3. Open the matching file in `recruiter/` only if you need the PDF version.
