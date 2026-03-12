# DOC-LEGAL-004: Third-Party Reference Material Handling

**Summary:** Copyrighted franchise reference packs may be studied locally for mechanics and presentation analysis, but they should be treated as local-only research material, not as repo-distributed project assets.

## Purpose

Use this note when deciding:

- whether external reference art or extracted franchise assets should live in the repo
- what can be committed to GitHub versus kept local-only
- how to document inspiration work without redistributing copyrighted material
- how to reduce copycat and infringement risk while still learning from reference games

## Core rule

Do **not** treat third-party franchise reference packs as normal project assets.

If the material contains copyrighted game art, extracted UI, icons, cut-ins, card art, APK dumps,
or other unlicensed franchise expression, the default posture is:

- local study only
- not shipped
- not committed
- not uploaded to public GitHub
- not included in releases
- not mirrored through Git LFS as a workaround

## Why this rule exists

There are two separate risks:

1. **Copyright / redistribution risk**
   Even if the material is "for inspiration," storing and publishing thousands of copyrighted franchise files in your public source repo is a redistribution problem, not just a private-reference problem.

2. **Copycat / platform risk**
   App stores and platforms care about whether your shipped game or metadata too closely imitates protected external expression. Internal reference use does not remove that risk.

This matches the repo's existing stance:

- [ART-004](../art/art-004-asset-provenance-and-source-tracking.md): do not intentionally copy protected franchise expression
- [DESIGN-010](../design/design-010-combat-hud-and-battle-ui-specification.md): use `Naruto Blazing` as an interaction reference, not a visual template
- [DOC-LEGAL-001](./doc-legal-001-ai-generated-assets-legal-considerations.md): inspiration should stay at the idea/system level, not protected expression

## Allowed use

These reference packs can be used locally for:

- studying dimensions, proportions, and timing patterns
- naming/taxonomy research (`chain`, `cutin`, `rarecutin`, etc.)
- identifying UI hierarchy patterns
- writing internal notes that describe what the reference is doing
- deriving neutral production rules for Shogun

## Not allowed as normal repo content

Do not commit or publish:

- extracted franchise art packs
- app icons from another game
- character portraits or splash art from another game
- raw cut-ins, UI icons, badges, or card art from another game
- APK/resource dumps
- large scraped screenshot libraries unless you clearly have the right to redistribute them

Do not rely on "it's only reference" as a GitHub or release justification.

## What *should* be committed instead

Commit the interpretation, not the source pack.

Good repo-safe outputs:

- markdown notes describing what was learned
- file manifests (filename, type, dimensions, counts)
- hashes or IDs for traceability
- redrawn neutral diagrams
- your own original mockups inspired by system structure, not copied expression
- small text tables such as "combo strip = 256x64"

This is the right pattern for Shogun's Blazing studies.

## GitHub rule

Public GitHub is the highest-risk place to keep these packs.

Default rule:

- keep third-party franchise reference packs out of the tracked repo
- ignore them in `.gitignore`
- do not attach them to issues, PRs, or releases
- do not use Git LFS to justify keeping them in the public repo

If you absolutely need to share them with collaborators, use a separate private storage location with intentionally limited access and clear provenance notes.

## Local storage rule

If reference packs are useful for active study, keep them in a clearly named **local-only** folder.

Example:

- `docs/NaBlA_References_Inspo/`

That folder may exist in the working tree for local research, but it should remain gitignored and treated like scratch/reference material, not project content.

## AI workflow caution

Avoid uploading third-party franchise packs to generative tools as if they were production source material.

Reasons:

- provider terms may allow retention/review depending on service tier
- it muddies provenance
- it increases derivative-style risk
- it makes later authorship/originality cleanup harder to defend

If a reference image is needed for internal experimentation, prefer:

- your own notes and crops describing the pattern
- original reconstruction prompts based on neutral design language
- your own cleaned mockups rather than direct franchise assets

## Practical Shogun rule for Blazing research

For `Naruto Blazing` specifically:

- keep the asset pack local-only
- commit docs like [ART-012](../art/art-012-combat-cutin-taxonomy-and-shogun-mapping.md)
- commit measurements, counts, and naming conclusions
- do **not** commit the raw Naruto assets themselves
- do **not** ship any asset that reads like direct Naruto expression

## Review checklist

Before pushing, confirm:

- no third-party franchise pack is staged
- `.gitignore` covers the local-only folder
- the repo contains notes, not the raw copyrighted source pack
- any new Shogun production asset is original enough to stand on its own
- prompts and references do not ask tools to imitate specific protected characters or franchise art literally
