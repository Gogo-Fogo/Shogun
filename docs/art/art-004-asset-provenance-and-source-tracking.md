# ART-004: Asset Provenance and Source Tracking

**Summary:** Provenance rules for generated and edited art assets so `Shogun` can trace how important visuals were produced and what risk they carry.

## Purpose

Use this note when deciding:

- what metadata should be retained for generated art
- how to document manual edits
- how to distinguish raw generation from production-ready assets
- what information is needed if legal or provenance questions appear later

## Core rule

Every shipped AI-assisted asset should be traceable.

That does not require a huge bureaucracy, but it does require enough metadata to answer:

- where did this asset come from
- what prompt or source was used
- what tool generated it
- what manual edits were applied
- what final production file was actually shipped

## Minimum provenance fields

For each shipped or production-candidate asset, retain:

- asset name
- character or unit name
- source tool
- generation date
- prompt
- seed, if available
- workflow or mode used
- editor used for cleanup
- manual edits performed
- final exported file hash

## Source separation

Keep these concepts distinct:

- raw generated output
- edited source asset
- final exported runtime asset

Do not overwrite raw generation with the cleaned production version if the asset is important enough to revisit later.

## Risk handling rules

Higher-value assets need stronger provenance discipline:

- hero characters
- boss key art
- splash art
- store or marketing visuals

For those, prefer:

- heavier manual editing
- stronger review notes
- clearer retained provenance

## Prompt hygiene

Do not use prompts that intentionally copy protected franchise expression.

Avoid:

- direct character-name imitation
- “make this look exactly like X franchise hero”
- uploading sensitive internal material unless the workflow explicitly requires it and the privacy terms are acceptable

## Review checklist before shipping

Before an AI-assisted asset is treated as production-ready, confirm:

- provenance fields exist
- manual edits are described
- final runtime file is identifiable
- the asset does not obviously imitate protected external expression

## Storage rule

Provenance does not need to live in the gameplay runtime build.

It does need to live somewhere durable in the project’s working materials so the team can trace decisions later.
