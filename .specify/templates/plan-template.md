# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]

**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]

**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]

**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]

**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]

**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]

**Project Type**: [e.g., library/cli/web-service/mobile-app/compiler/desktop-app or NEEDS CLARIFICATION]

**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]

**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]

**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

<!--
  ACTION REQUIRED: Verify compliance with all 10 constitutional principles.
  Violations MUST be justified in the Complexity Tracking table.
-->

### Principle I: Clean Architecture
- [ ] Layer dependencies are correct (Domain ← Application ← Infrastructure/Api)
- [ ] No cross-layer shortcuts

### Principle II: CQRS via Cortex.Mediator
- [ ] Commands and Queries are properly separated
- [ ] No MediatR usage

### Principle III: Test-First TDD
- [ ] TDD workflow planned (Red → Green → Refactor)
- [ ] Coverage target ≥ 95% for new code

### Principle IV: No ORM
- [ ] Using Dapper + SqlKata only (no Entity Framework)

### Principle V: Observability
- [ ] Logging via OlimpiaIT.Logging.Serilog planned

### Principle VI: FluentValidation
- [ ] One validator per Command/Query planned

### Principle VII: API Design Conventions
- [ ] Controllers follow project conventions
- [ ] XML documentation planned
- [ ] Pagination pattern applied where needed

### Principle VIII: C# Standards
- [ ] Code will comply with A1–A19 conventions
- [ ] DateTime.Now (not UtcNow) for timestamps

### Principle IX: SQL Server Standards
- [ ] SQL scripts follow project conventions (if applicable)
- [ ] No sp_ or usp_ prefixes on stored procedures

### Principle X: Security Standards
- [ ] Component type identified in spec.md
- [ ] Correct security profiles selected (secure-core.md + conditional profiles)
- [ ] Security requirements from selected profiles documented with REQ-IDs
- [ ] No duplication of security policies from central package
- [ ] Security review gates planned
- [ ] AI-generated code marked as unverified
- [ ] Traceability plan includes: REQ-ID, OWASP category, core rule (R#), test evidence

**Security Decisions**:

<!--
  Document security-specific technical decisions here.
  Reference the applicable profiles and requirements.
-->

- **Authentication/Authorization**: [Describe approach and which requirements it satisfies]
- **Data Protection**: [Describe crypto, secrets management, PII handling]
- **Input Validation**: [Describe validation strategy and injection prevention]
- **Security Testing**: [Describe which WSTG/MASVS tests will be implemented]

**Pass/Fail**: [PASS/FAIL - must be PASS before proceeding]

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
