---
description: "Create a feature specification (preset override)"
scripts:
  sh: scripts/bash/create-new-feature.sh "{ARGS}"
  ps: scripts/powershell/create-new-feature.ps1 "{ARGS}"
---

## User Input

```text
$ARGUMENTS
```

## Language Policy (neutral/adaptive)

- Respond in the same language as the user's request.
- If language is unclear, preserve the predominant language of the artifact being edited.
- Do not enforce a single global language.

Given the feature description above:

1. **Create the feature branch** by running the script:
   - Bash: `{SCRIPT} --json --short-name "<short-name>" "<description>"`
   - The JSON output contains BRANCH_NAME and SPEC_FILE paths.

2. **Read the spec-template** to see the sections you need to fill.

3. **Resolve security memory path once** (in this strict order):
   - `.specify/presets/secure-engineering-kit/memory/security/`
   - `presets/secure-engineering-kit/memory/security/`
   - `.specify/memory/security/` (legacy compatibility only)
   - Use only the first existing path.

4. **Detect component type and select security profiles**:
   - Always include `secure-core.md`.
   - Include `api.spec.md`, `mobile.spec.md`, `web.spec.md` only when applicable.
   - If component type is ambiguous, stop and ask for clarification before continuing.

5. **Create operational security summary** at `FEATURE_DIR/security-context.md`:
   - `FEATURE_DIR` is the parent folder of `SPEC_FILE`.
   - This file is mandatory for downstream steps.
   - Include at minimum:
     - resolved security path;
     - selected profiles;
     - applicable REQ list (only IDs + short title);
     - OWASP/WSTG/MASVS traceability references;
     - required evidence per REQ;
     - unresolved items as `NEEDS CLARIFICATION`.
   - Do not duplicate full profile content; summarize and reference.

6. **Write the specification** to SPEC_FILE, replacing the placeholders in each section
   (Overview, Requirements, Acceptance Criteria) with details from the user's description.

7. **Ensure consistency**:
   - `spec.md` must contain the selected profiles and match `security-context.md`.
   - If a security REQ appears in `spec.md`, it must appear in `security-context.md`.
