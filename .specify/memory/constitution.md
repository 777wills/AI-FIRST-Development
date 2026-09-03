<!--
  Sync Impact Report
  Version change: 1.3.1 → 1.4.0
  Modified principles: None
  Added principles:
    - X. Security Standards — Application Security Governance (NEW)
      Establishes security as a constitutional principle with:
      · Mandatory secure-core.md for all development
      · Conditional security profiles (api.spec.md, mobile.spec.md, web.spec.md)
      · Traceability requirements (REQ-ID, OWASP category, core rule, test evidence)
      · AI-generated code verification requirements
      · Non-duplication of global security policies
  Removed principles: None
  Synchronization performed:
    ✅ Added Principle X referencing .specify/presets/secure-engineering-kit/memory/security/
    ✅ Updated spec-template.md with Security Profile Selection section
    ✅ Updated plan-template.md Constitution Check to include security compliance
    ✅ Updated tasks-template.md with [sec:REQ-ID] tag convention
    ✅ README.md updated to reference Security Standards in documentation table
  Templates synchronized:
    ✅ .specify/templates/spec-template.md — Security Profile Selection
    ✅ .specify/templates/plan-template.md — Security in Constitution Check
    ✅ .specify/templates/tasks-template.md — [sec:REQ-ID] tagging
    ✅ README.md — Documentation table includes Security Standards
  Security package status:
    ✅ .specify/presets/secure-engineering-kit/memory/security/secure-core.md (PRESERVED)
    ✅ .specify/presets/secure-engineering-kit/memory/security/api.spec.md (PRESERVED)
    ✅ .specify/presets/secure-engineering-kit/memory/security/mobile.spec.md (PRESERVED)
    ✅ .specify/presets/secure-engineering-kit/memory/security/web.spec.md (PRESERVED)
  Follow-up TODOs: None
-->

# Olimpia Constitution

## Core Principles

### I. Clean Architecture — Strict Layer Isolation (NON-NEGOTIABLE)

The solution is organized into layers with enforced one-way dependencies:
`Domain` ← `Application` ← `Infrastructure` / `Api`.

- `Domain` MUST have zero external NuGet dependencies. Pure C# only.
- `Application` MUST depend only on `Domain`. It defines contracts (interfaces) that
  `Infrastructure` fulfills. NEVER reference `Infrastructure` or `Api` from `Application`.
- `Infrastructure` and `Api` MAY depend on `Application`; `Infrastructure` may reference
  `Application` assemblies for DI scanning only — never for logic.
- Cross-layer shortcuts (injecting a concrete repository into a controller, referencing
  Infrastructure types from Application handlers) are strictly forbidden.
- Any violation MUST be justified, documented in the plan's Complexity Tracking table,
  and approved before merging.
- Projects MAY add an `Infrastructure.Logging` layer to isolate logging concerns from
  the main `Infrastructure` project. If present, `Api` depends on it for startup wiring.
- **Domain model rules:**
  - All entities MUST inherit from `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt`).
  - Entities MUST be `sealed` and provide both a **parameterless constructor** (for Dapper)
    and a **parameterized constructor** (for application code).
  - Strings MUST be initialized with `= string.Empty` (never left as nullable without intent).
  - Boolean properties MUST use semantic prefixes: `Is*`, `Has*`, `Can*`, `Should*`.
  - Pagination types `FilterCriteria`, `SortCriteria`, and `FilterOperator` live in
    `Olimpia.Domain.Common` and MUST be referenced from there.
  - Repository interfaces inherit from `IGenericRepository<T>` and declare only
    domain-specific method signatures; `IGenericRepository<T>` exposes `GetPagedAsync`.
  - Stored Procedure result types live in `Domain/StoredProcedureEntities/`.
  - Stored Procedure interfaces live in `Domain/StoredProcedureRepositories/`.
  - View interfaces live in `Domain/ViewRepositories/`.
  - UnitOfWork interfaces live in `Domain/UnitOfWork/` (one per database if multi-database).

### II. CQRS via Cortex.Mediator — No MediatR (NON-NEGOTIABLE)

All business operations MUST be expressed as Commands (write) or Queries (read) dispatched
through `Cortex.Mediator`.

- **Dispatch**: `await _mediator.SendAsync(command)` for Commands;
  `await _mediator.SendQueryAsync(query)` for Queries. Never mix dispatch methods.
- MediatR is explicitly forbidden: no `IMediator`, `IRequest`, or pipeline behaviors
  from MediatR.
- **Scaffolding vs. Logic separation** (enforced by agent roles):
  - `Command` / `Query` records and `Dto` types are **declarative scaffolding** — created
    first by the Application Implementer agent.
  - `Handler` and `Validator` classes contain **business logic** — created by the TDD
    Implementer agent via Red → Green → Refactor.
  - NEVER merge scaffolding and logic creation into one step.
- **Command Handler rules:**
  - Implements `ICommandHandler<TCommand, TResult>`.
  - ALWAYS injects `IUnitOfWork` and wraps mutations in:
    `BeginTransactionAsync` → `CommitAsync` / `catch { RollbackAsync; throw; }`.
  - Throws semantic business exceptions (`InvalidOperationException`, `ArgumentException`,
    `KeyNotFoundException`) — never returns `null` to signal failure.
- **Query Handler rules:**
  - Implements `IQueryHandler<TQuery, TResult>`.
  - NEVER uses `IUnitOfWork` or opens transactions.
  - ALWAYS returns DTOs; NEVER returns domain entities directly.
  - SHOULD implement Cache-Aside with `IDistributedCache` (Check → Return or Fetch → Store).
- **Paginated Queries:**
  - Query record inherits from `PagedQuery` (abstract record in
    `Application/Common/Pagination/`); implements `IQuery<PagedResult<TDto>>`.
  - Handler calls `_repository.GetPagedAsync(...)`, maps to DTOs, returns
    `PagedResult<TDto>.Create(dtos, pageNumber, pageSize, totalCount)`.
  - If a business default sort is needed, the handler applies it when `SortFields` is
    null/empty. The controller NEVER sets default sort.

### III. Test-First TDD — Red → Green → Refactor (NON-NEGOTIABLE)

TDD is mandatory for all new code in `Application`, `Infrastructure`, and `Domain`.

- Tests MUST be written (and reviewed) **before** the implementation code.
- Cycle: **Red** (failing test) → **Green** (minimum code to pass) → **Refactor** (clean up).
  Running `dotnet test` is mandatory after each phase.
- Minimum line coverage for any new file: **≥ 95%**, verified by the Coverage Analyzer
  agent before the PR gate.
- **Test framework**: MSTest v4 + Moq + FluentAssertions. No xUnit or NUnit.
- **One logical assert per test**: verify a single concept per test method.
  - For full-shape DTO verification use `result.Should().BeEquivalentTo(expected)` — one
    call counts as one logical assert.
  - Scalar results use `result.Should().Be(value)`.
  - Side-effects use `mock.Verify(…, Times.Once)`.
- **Parameterized tests**: `[TestMethod]` + `[DataRow]`. `[DataTestMethod]` is deprecated
  (MSTEST0044) and MUST NOT be used.
- **No conditional logic in tests**: no `if`, `switch`, ternaries, or loops inside test
  methods. Use `[DataRow]` to separate scenarios.
- **Naming**: class = `{Subject}Tests`; method = `{Method}_Should_{Result}_When_{Scenario}`.
- **`global::` prefix**: entity types in handlers, repositories, and tests MUST be
  fully qualified: `global::Olimpia.Domain.Entities.{Entity}` to avoid namespace collisions.

### IV. No ORM — Dapper + SqlKata Only (NON-NEGOTIABLE)

Entity Framework (any version) is forbidden in this solution.

- All database access MUST use **Dapper** for execution and **SqlKata** for query
  construction through `IGenericRepository<T>`, `IStoredProcedureRepository`, or
  `IViewRepository` contracts.
- **Forbidden**: raw SQL strings embedded in C# code (`Db.Statement("SELECT …")`).
  Use SqlKata's fluent API: `Db.Query("Products").Where("Id", id)…`
- **`GenericRepository<T>` column exclusion contract:**
  - INSERT auto-excludes `Id` and `UpdatedAt` (generated by DB).
  - UPDATE auto-excludes `Id` and `CreatedAt` (immutable).
  - Table name convention: `typeof(T).Name + "s"` — override with
    `protected override string TableName`.
- **`IUnitOfWork`** exposes `IDbConnection DbConnection` and `IDbTransaction? DbTransaction`
  (abstract `System.Data` types — NEVER `SqlConnection`/`SqlTransaction` directly).
- **Multi-database**: if the feature requires more than one database, define a separate
  `IXxxUnitOfWork` interface per database in `Domain/UnitOfWork/`. Each SP/View
  repository accepts its own UnitOfWork. MCP `db-*` services are used for schema
  introspection and DDL execution; identify the correct MCP before running DDL.
- **Retry Decorator rule**: `GenericRepositoryRetryDecorator<T>` applies Polly v8
  `ResiliencePipeline` retry **only to reads** (`GetByIdAsync`, `GetAllAsync`,
  `GetPagedAsync`). Writes (`AddAsync`, `UpdateAsync`, `DeleteAsync`) are NEVER
  retried (not idempotent).
- **DI registration**: use **Scrutor** for auto-discovery and decorator registration.
  NEVER register repositories manually unless auto-discovery cannot cover it.
- **Transactions**: ALWAYS pass `transaction: UnitOfWork.DbTransaction` to every Dapper /
  SqlKata method call inside a command handler.
- **Stored Procedures**: use `IStoredProcedureRepository` (`ExecuteAsync` for no-result,
  `QueryAsync<T>` for lists, `DynamicParameters` for OUTPUT params).
- **Views**: use `IViewRepository` (`QueryAsync<T>`, `QueryPagedAsync<T>`).
- **SQL Scripts**: schema changes delivered as idempotent scripts under `scripts/sql/`.
  Full SQL standards are governed by Principle IX.

### V. Observability — Structured Logging via OlimpiaIT.Logging.Serilog (NON-NEGOTIABLE)

All services MUST produce structured, queryable logs through `LogCentral` using the
corporate packages `OlimpiaIT.Logging.Serilog` and `OlimpiaIT.Logging.Entities`.

- Use `ILogger<T>` with extension methods from `OlimpiaIT.Logging.Serilog.Extensions`:
  - `logger.LogAudit(action, parameter, before, after)` — auditoría de cambios de datos.
  - `logger.LogEvent(detail)` — eventos de negocio completados.
  - `logger.LogError(message, exception, severity)` — errores con excepción.
  - `logger.LogRequest(endpoint, method, statusCode, durationMs)` — métricas HTTP.
  - `logger.LogStructuredAudit(dto)`, `LogStructuredError(dto)`, `LogStructuredEvent(dto)`,
    `LogStructuredRequest(dto)` — para DTOs completos con todos los campos.
- NEVER use `Console.Write*` or raw `ILogger.Log(...)` without an EventId in business logic.
- Log type is routed automatically by the Serilog sink based on EventId:
  - EventId 1 ("Audit") → `Audits/CreateAudit`
  - EventId 2 ("Error") → `Errors/CreateError`
  - EventId 3 ("Event") → `Events/CreateEvent`
  - EventId 4 ("Request") → `Requests/CreateRequest`
- The HTTP sink uses Polly for retries (3 attempts, exponential backoff). Failures are
  discarded silently — fire-and-forget, non-blocking.
- Emojis and unstructured messages in infrastructure logs are forbidden.
- Security-sensitive data (passwords, tokens, PII) MUST NOT appear in log messages.
  Configure `SensitiveFields` in the LogCentral service section to auto-redact JSON fields.
- `ApplicationName` MUST be set in the `LogCentralService` configuration section.
  An empty value causes `InvalidOperationException` on startup by design.
- `LogCentralService:Token` MUST be managed via User Secrets (dev) or environment
  variables (CI/CD, prod). NEVER commit tokens in `appsettings*.json`.
- Bootstrap in `Program.cs`:
  `builder.Services.AddLogCentral(builder.Configuration);`
  `builder.Host.UseSerilogWithLogCentral(builder.Configuration);`
- `AuditMiddleware` automatically logs every HTTP request/response as audit.
- `RequestLoggingMiddleware` automatically logs HTTP metrics as request.

### VI. FluentValidation — One Validator per Command/Query (NON-NEGOTIABLE)

Every Command and Query MUST have exactly one `FluentValidation.AbstractValidator<T>`.

- **One validator per Command/Query**, co-located in the same feature folder.
- Validators are auto-invoked by `Cortex.Mediator` before the handler is executed.
- **Validation messages in Spanish** (they reach end users).
- Validators MUST NOT perform database lookups; delegate uniqueness checks to the handler.
- Validators only validate what the spec requires. No speculative rules.
- **Paginated Query validators MUST define:**
  - `PageNumber >= 1` and `PageSize` between 1 and 100.
  - A `Dictionary<string, IReadOnlyList<FilterOperator>>` whitelist of filterable fields
    (case-insensitive).
  - A `HashSet<string>` whitelist of sortable fields (case-insensitive).
  - Per-filter value type validation (`decimal.TryParse`, `DateTime.TryParse`, etc.).

### VII. API Design Conventions (NON-NEGOTIABLE)

Controllers, versioning, authorization, pagination, and documentation MUST follow these
rules; violations block PR approval.

#### Controllers
- Inherit from `ApiController` (project base class).
- Are `sealed` and placed in `src/Olimpia.Api/Controllers/V{N}/`.
- Carry class-level `[ApiVersion("N.0")]`; each method carries `[MapToApiVersion("N.0")]`
  immediately before the HTTP verb attribute.
- Inject only `IMediator` (Cortex.Mediator). NEVER inject repositories or services directly.
- Contain **zero business logic**: receive → dispatch → return.
- **FORBIDDEN**: `try/catch` in controller actions. `ExceptionHandlingMiddleware` translates
  exceptions to `ProblemDetails`. Document expected HTTP codes via `[ProducesResponseType]`
  and `<response>` XML tags; do NOT implement the mapping.

#### Authorization
- Controllers are protected with `[Authorize]` at class level by default.
- Endpoint-level scopes: `[Authorize(Policy = "feature.read")]` or `"feature.write"`.
- `[AllowAnonymous]` is reserved for health-check and explicitly anonymous public endpoints.

#### Pagination
- Paginated endpoints use `[PaginatedEndpoint(AllowedFilters="…", AllowedSortFields="…")]`.
- Dynamic filters (`campo[operador]=valor`) are parsed via `QueryStringFilterParser`.
- Response: `Ok(PagedEnvelope<TDto>.FromPagedResult(result))` — `{ data, meta }` envelope.
- Non-paginated endpoints MUST NOT wrap in envelope.

#### XML Documentation & Response Codes
- Every controller action MUST have: `<summary>`, `<remarks>`, `<param>` per parameter,
  `<response code="XXX">` per possible HTTP code.
- Every exposed Command / Query / DTO MUST have `<summary>` on the type and each
  public property. `<example>` required for non-obvious formats (IDs, codes, ISO dates).
- `[ProducesResponseType]` MUST exist for every declared `<response>` code.
- `dotnet build` MUST succeed without CS1591 warnings.

### VIII. C# and .NET 10 Code Standards (NON-NEGOTIABLE)

All C# code MUST comply with conventions A1–A19 in `.github/instructions/csharp-conventions.instructions.md`
and with the Microsoft C# Coding Conventions and Framework Design Guidelines.

#### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Class, struct, record, interface, delegate, enum | PascalCase | `ProductRepository` |
| Public member (property, method, event, field) | PascalCase | `IsActive`, `GetByIdAsync` |
| Interface | `I` prefix + PascalCase | `IProductRepository` |
| Private / internal instance field | `_camelCase` | `_unitOfWork` |
| Private / internal static field | `s_camelCase` | `s_cache` |
| Thread-static field | `t_camelCase` | `t_context` |
| Local variable, method parameter | camelCase | `pageNumber`, `sortFields` |
| Generic type parameter | `T` or `T`-prefixed descriptor | `T`, `TResult`, `TEntity` |
| Constant (field or local) | PascalCase | `MaxPageSize` |
| Boolean property / variable | Semantic prefix: `Is`, `Has`, `Can`, `Should` | `IsActive` |

- Identifiers MUST be meaningful and descriptive — prefer clarity over brevity.
- Abbreviations: only widely accepted abbreviations allowed (`Id`, `Url`, `Http`, `Sql`,
  `Api`, `Guid`, `Io`). All-caps abbreviations (`ID`, `URL`, `HTTP`) are forbidden.

#### `var` Discipline

- Use `var` only when the type is **unambiguously obvious** from the right-hand side
  (e.g., `new` operator, explicit cast, literal).
- NEVER use `var` when the type must be inferred from a method name alone.
- Use explicit types in `foreach` loops.

#### Async / Await

- All I/O-bound operations MUST be `async`/`await`. NEVER use `.Result` or `.Wait()`.
- Pass `CancellationToken` through all public async methods.
- Use `ConfigureAwait(false)` in library and infrastructure code (not in controller actions).

#### Exception Discipline

- Catch **specific** exception types; never catch bare `Exception` without a `when` filter.
- Use `using var` (C# 8 declarative form) for `IDisposable` cleanup.
- No `== true` / `== false` comparisons; use `&&`/`||` (short-circuit) for booleans.

#### Layout & Style

- 4-space indentation; no tab characters.
- File-scoped namespace declarations (`namespace Olimpia.X;`) for all new files.
- `using` directives placed **outside** the namespace declaration.
- Every generated method MUST include `// Método generado por GitHub Copilot`.
- Generated blocks wrapped in `// Inicio código generado por GitHub Copilot` /
  `// Fin código generado por GitHub Copilot`.
- `sealed` on all concrete classes unless designed for inheritance.
- All architecture changes MUST update `copilot-instructions.md`, affected `docs/` files,
  and this constitution.

#### DateTime and Timezone Handling

- **NEVER use `DateTime.UtcNow`** in application or infrastructure code.
  All timestamp generation MUST use **`DateTime.Now`** so that values reflect the server's
  local time. The server's OS timezone MUST be configured to the business timezone
  (Colombia / Perú — UTC-5, IANA: `America/Bogota`, Windows: `SA Pacific Standard Time`).
- `DateTime` values stored in the database represent **local business time** (`DateTimeKind.Local`
  or `DateTimeKind.Unspecified` from Dapper mapping). NEVER store UTC timestamps unless a
  specific cross-timezone integration requires it (document reason).
- `BaseEntity.CreatedAt` defaults to `DateTime.Now`; `GenericRepository.UpdateAsync` sets
  `UpdatedAt = DateTime.Now` before persisting.
- In test data, use `DateTime.Now` or `new DateTime(y, m, d, h, min, s, DateTimeKind.Unspecified)`
  — NEVER `DateTimeKind.Utc`.



### X. Security Standards — Application Security Governance (NON-NEGOTIABLE)

Security is a foundational, non-negotiable principle enforced at the constitutional level.
All features MUST comply with the centralized security package and traceability requirements.

#### Security Package Location

The single source of truth for security requirements resides in:
`.specify/presets/secure-engineering-kit/memory/security/`

**Package contents**:
- **`secure-core.md`** — MANDATORY for ALL development. Contains universal rules (R1–R7),
  OWASP mappings, anti-LLM robustness, and hard constraints verified in pentests.
- **`api.spec.md`** — CONDITIONAL profile for API/backend development (OWASP API Security Top 10 2023).
- **`mobile.spec.md`** — CONDITIONAL profile for mobile app development (OWASP Mobile Top 10 2024 + MASVS).
- **`web.spec.md`** — CONDITIONAL profile for web frontend development (OWASP Top 10 2021/2025 + WSTG).

#### Mandatory Application Rules

1. **Universal Core**:
   - `secure-core.md` applies to EVERY feature without exception.
   - Rules R1–R7 are non-negotiable hard constraints (zero secrets in code, AES-256-GCM crypto,
     ownership validation, auth-by-default, parameterized queries, token validation, minimal exposure).

2. **Conditional Profiles**:
   - Component type MUST be identified during specification phase.
   - Apply the matching profile(s):
     * API/backend → `api.spec.md`
     * Mobile app → `mobile.spec.md`
     * Web frontend → `web.spec.md`
   - Full-stack systems may require multiple profiles (e.g., mobile app + its API).
   - If component type cannot be determined, BLOCK spec generation and request clarification.

3. **Non-Duplication**:
   - Security policies live ONLY in the centralized package.
   - Project-specific code MUST NOT redefine, copy, or modify these policies.
   - Local deviations require explicit constitutional amendment with justification.

4. **Traceability**:
   - Every security implementation MUST be traceable via:
     * **REQ-ID**: unique requirement identifier from the security spec (e.g., `REQ-API-01`).
     * **OWASP Category**: reference to OWASP Top 10 / API Top 10 / Mobile Top 10 category.
     * **Core Rule**: reference to `secure-core.md` rule (R1–R7) when applicable.
     * **Test Evidence**: reference to WSTG test case or MASVS verification requirement.
   - Task descriptions use `[sec:REQ-ID]` tags for security-related tasks.

5. **AI-Generated Code Verification**:
   - All code generated by AI agents is considered **unverified** until it passes:
     * Static analysis (SAST)
     * Security code review by human
     * Security-specific tests (contract/integration)
   - AI agents MUST declare generated code as unverified in comments.
   - Merge gates MUST block unverified security implementations.

6. **Security Review Gates**:
   - **Specification phase**: Component type identified; correct profiles selected.
   - **Planning phase**: Security decisions documented; Constitution Check includes security compliance.
   - **Implementation phase**: Security tasks tagged with `[sec:REQ-ID]`; traceability maintained.
   - **Review phase**: SAST results reviewed; security tests passing; human approval obtained.
   - **Merge gate**: All security tasks completed and verified before PR approval.

#### Security Package Maintenance

- The security package is maintained by the Security Engineering team.
- Updates to the package automatically propagate to all projects.
- Local projects MUST NOT modify security package files.
- To request a policy change, submit a constitutional amendment proposal.

---

| Anti-Pattern | Why Forbidden |
|--------------|--------------|
| **Service Locator** — `IServiceProvider.GetService(…)` inside a class | Hides dependencies; violates IoC |
| **Captive Dependency** — singleton holding a scoped dependency | Scoped lifetime leaked |
| **Async DI factory** — `Task.Result` inside `AddSingleton` lambda | Causes deadlock on startup |
| **Manual disposal of DI services** — calling `.Dispose()` on injected `IDisposable` | Container owns lifetime |
| **Stateful static classes** | Hidden global state; untestable |
| **Direct instantiation** — `new ConcreteService()` inside a class | Couples code; prevents testing |

- Singleton services MUST be thread-safe.
- Scoped services MUST NOT be resolved from the root container.

### IX. SQL Server Design Standards (NON-NEGOTIABLE)

All T-SQL code (scripts, stored procedures, views, functions) MUST comply with these rules.

#### Script Structure

Every SQL script MUST begin with:
```sql
USE [DatabaseName];
GO
SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO
```
And MUST be **idempotent**: use `IF OBJECT_ID(…) IS NULL` or `IF NOT EXISTS (…)` guards
before every `CREATE` statement.

#### T-SQL Keywords & Formatting

- **T-SQL reserved keywords in UPPERCASE**: `SELECT`, `FROM`, `WHERE`, `INSERT INTO`, etc.
- **Object names in PascalCase**: tables, columns, parameters, indexes, constraints.
- All statements MUST be terminated with a semicolon (`;`).
- Always prefix Unicode string literals with `N`: `N'valor'`.
- Never use `SELECT *` — list explicit columns.
- Explicit column lists in every `INSERT INTO … VALUES` statement.

#### Naming Conventions

| Object | Convention | Example |
|--------|-----------|---------|
| Table | PascalCase plural | `Products`, `SalesOrders` |
| Column | PascalCase | `CreatedAt`, `IsActive` |
| Stored Procedure | PascalCase (no prefix) | `GetProductById`, `CreateOrder` |
| View | `vw_Noun` | `vw_ActiveProducts` |
| Function (scalar) | `ufn_VerbNoun` | `ufn_CalculateTax` |
| Index | `IX_Table_Column(s)` | `IX_Products_IsActive` |
| Primary Key | `PK_Table` | `PK_Products` |
| Foreign Key | `FK_Table_RelatedTable` | `FK_Orders_Products` |
| SP Parameter | `@PascalCase` | `@ProductId`, `@IsActive` |

- **NEVER use the `sp_` prefix**: reserved for SQL Server system procedures; causes engine
  to search `master` first, degrading performance and risking name collisions.
- **NEVER use the `usp_` prefix**: does not add value, inconsistent with existing SPs.
  Use plain PascalCase names (e.g., `GetProductById`, `CreateOrder`).

#### Stored Procedure Standards

Every stored procedure MUST include a header comment block:
```sql
-- =============================================
-- Author:      [Name]
-- Create Date: [YYYY-MM-DD]
-- Description: [Purpose of the procedure]
-- Parameters:
--   @Param1  [type]  [description]
-- Returns:    [description]
-- Modified:   [YYYY-MM-DD] [Name] [Reason]
-- =============================================
```

Additional rules:
- `SET NOCOUNT ON;` as the **first statement** in every SP body.
- Use **`ALTER PROCEDURE`** to modify existing SPs — never `DROP` + `CREATE`.
- NEVER grant `SELECT`/`INSERT`/`UPDATE`/`DELETE` on tables to application users.
  Grant `EXECUTE` on stored procedures only.
- Use explicit schema qualification: `dbo.Products`, not `Products`.
- Dynamic SQL MUST use `sp_executesql` with typed parameters — never string concatenation.

#### Standard Data Types

| Concern | Type | Notes |
|---------|------|-------|
| Integer primary key | `INT IDENTITY(1,1)` | auto-increment |
| Text (variable) | `NVARCHAR(N)` | Unicode; specify N explicitly |
| Text (large) | `NVARCHAR(MAX)` | only when > 4000 chars needed |
| Decimal / money | `DECIMAL(18,2)` | never `FLOAT` for financial values |
| Date + time | `DATETIME2(7)` | preferred over `DATETIME` |
| Date only | `DATE` | |
| Flag / boolean | `BIT` | `1` = true, `0` = false |

---

## Amendment Process

- All amendments MUST bump the semantic version and record the date.
- Every PR MUST include a Constitution Check confirming compliance with all ten principles.
- Compliance review MUST be performed at every sprint boundary.
- Runtime guidance for AI agents lives in `.github/copilot-instructions.md` and
  `.github/instructions/` files. The Constitution defines *what*; those files define *how*.
- Security package updates in `.specify/presets/secure-engineering-kit/memory/security/`
  are managed by the Security Engineering team and propagate automatically.

---

**Version**: 1.4.0 | **Ratified**: 2026-06-01 | **Last Amended**: 2026-08-20
