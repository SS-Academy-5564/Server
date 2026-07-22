# AGENTS.md

Single source of truth for AI coding agents working in this repository.
Other agent config files (`CLAUDE.md`, `.github/copilot-instructions.md`) are
thin pointers to this file — edit **this** file, never the pointers.

## Project Overview

Pulse is a server-side monitoring platform built with **.NET 10**.

### Solution Structure

```text
Pulse.slnx
├── Pulse.API         → HTTP layer (controllers, middleware, filters, DI wiring)
├── Pulse.BL          → business logic (feature handlers, services, validation, authorization)
├── Pulse.DAL         → persistence (Dapper queries/commands, entities, SQL scripts)
├── Pulse.Worker      → background jobs / monitoring execution
└── Pulse.Tests.Unit  → xUnit tests
```

### Getting Started

```bash
cd Server
dotnet restore
dotnet build
dotnet test
```

## Architecture

Follow **Clean Architecture** principles.

### Dependency Direction

```text
Pulse.API     → Pulse.BL
Pulse.Worker  → Pulse.BL
Pulse.BL      → Pulse.DAL
```

Cross-layer dependencies are forbidden.

### Code Organization — Feature Folders

Both `Pulse.API` and `Pulse.BL` are organized by **feature**, not by technical
type. A feature lives together across layers, e.g.:

```text
Pulse.BL/Features/Auth/Login/LoginHandler.cs
Pulse.API/Features/Auth/Login/LoginController.cs
```

Business operations are implemented as handlers dispatched through the
interfaces in `Pulse.BL/Common/Handlers`:

- `IAsyncHandler<TCommand, TResult>` — commands
- `IAsyncQueryHandler<TResult>` — queries

There is no MediatR; handlers are registered and injected through DI.

### Layer Responsibilities

#### Pulse.API

* Controllers, middleware, filters, request/response mapping, DI configuration.

Rules:

* Handle HTTP concerns only.
* Use DTOs for requests and responses.
* Return `IActionResult` / `ActionResult<T>` with appropriate status codes.
* Do not contain business logic.
* Do not access the DAL directly.
* All controllers must extend `PulseControllerBase`.
* Do not apply `[AutoValidate]` directly to controllers — it is inherited from `PulseControllerBase`.

#### Pulse.BL

* Business logic and use cases, feature handlers, services, DTOs, validation,
  authorization, and application errors.

Rules:

* Contains all business logic.
* Performs entity-to-DTO mapping.
* Must not access the database directly.
* Must not reference `Pulse.API` or any HTTP/controller types.

#### Pulse.DAL

* Dapper queries and commands, database entities and enums, SQL scripts.

Rules:

* Persistence logic only.
* Use parameterized SQL; keep SQL inside DAL.
* Each query/command performs a single operation.
* Each file in `Queries/` and `Commands/` represents a single domain area.
* Create connections only through `IDbConnectionFactory`.

#### Pulse.Worker

* Background jobs, scheduled tasks, monitoring execution.

Rules:

* Uses BL services only.
* Supports and propagates `CancellationToken`.
* Logs execution and failures.
* Must **not** execute migrations; assumes the schema is already up to date.

## Error Handling — Result Pattern (FluentResults)

Business operations do **not** throw exceptions for expected failures. They
return a `Result` / `Result<T>` (FluentResults) and, on failure, attach a
specific `AppError` subclass from `Pulse.BL/Common/Errors`:

| Error type         | Meaning              | HTTP status |
| ------------------ | -------------------- | ----------- |
| `ValidationError`  | invalid input        | 400         |
| `UnauthorizedError`| not authenticated    | 401         |
| `ForbiddenError`   | not permitted        | 403         |
| `NotFoundError`    | resource missing     | 404         |
| `ConflictError`    | state conflict       | 409         |
| `InternalError`    | unexpected failure   | 500         |

```csharp
// BL — return, do not throw
if (user is null)
{
    return Result.Fail(new UnauthorizedError("Invalid email or password."));
}

return Result.Ok();
```

Controllers convert results to HTTP via `PulseControllerBase.ToActionResult(...)`,
which delegates to `ResultMapper`. Responses are wrapped in the `ApiResponse` /
`ApiResponse<T>` envelope.

```csharp
public class LoginController : PulseControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login([Validate] LoginRequest request)
    {
        Result<LoginResponse> result = await _handler.HandleAsync(request);
        return ToActionResult(result);
    }
}
```

Rules:

* Do **not** throw exceptions for expected business errors.
* Do **not** use `try`/`catch` for business flow control.
* Unexpected/unhandled exceptions are caught by `ExceptionHandlingMiddleware` in the API.

## Request Validation

* All controllers inherit `[AutoValidate]` from `PulseControllerBase` — do not add it manually.
* Mark action parameters that require validation with `[Validate]`.
* Validators (FluentValidation) are registered automatically — do not register them manually in DI.
* `ValidateRequestActionFilter` resolves validators at runtime; only parameters marked `[Validate]` are validated.

```csharp
public class ExampleRequestValidator : AbstractValidator<ExampleRequest>
{
    public ExampleRequestValidator()
    {
        RuleFor(r => r.ExampleProperty).MinimumLength(10).MaximumLength(100);
    }
}
```

## Dependency Injection

* Use constructor injection.
* Register all services through DI.
* Do not use the service locator pattern.
* Do not manually create service dependencies.

## Database Access

* Use Dapper and `Microsoft.Data.SqlClient`.
* Create connections only through `IDbConnectionFactory`.
* Keep SQL inside DAL.
* Read connection strings from configuration only.

## Database Migrations

* Migrations are SQL scripts in `Pulse.DAL/Scripts`.
* Never modify an existing migration script that has already been committed.
* Migration scripts must be additive and executed in sequence.
* Preserve the existing script naming and numbering conventions.
* Creation and execution are handled by `DatabaseInitializer` and `DatabaseMigration`.
* `Pulse.API` executes migrations automatically during startup before accepting HTTP requests.
* `Pulse.Worker` must not execute migrations.
* Do not execute migration SQL directly from services, controllers, workers, or application logic.
* Development seed data belongs in `Scripts/Dev/Seed` and runs only when enabled through configuration.

## Configuration

* Use `appsettings.json`.
* Never commit secrets or credentials.

## Async

* Use `async`/`await` for all I/O operations.
* Async methods must end with `Async`.
* Propagate `CancellationToken`.
* Avoid `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()`.

## Testing

* Use **xUnit**.
* Follow Arrange–Act–Assert.
* Mock external dependencies using Moq or NSubstitute.

Naming:

```text
MethodName_StateUnderTest_ExpectedBehavior
```

```csharp
CreateMonitor_EmptyName_ShouldThrowValidationException()
```

## Forbidden

* Business logic in controllers.
* Direct DAL access from API or Worker.
* Direct `SqlConnection` creation outside the connection factory.
* SQL outside DAL.
* Returning database entities from API.
* Throwing exceptions for expected business errors (use the Result pattern).
* `try`/`catch` for business flow control.
* Blocking async calls.
* Cross-layer dependencies.
* Applying `[AutoValidate]` directly to a controller class.
* Manually registering FluentValidation validators in DI.

## Git Workflow

- **Branches:** one per issue, named `{type}/{issue-id}-{short-description}`
  with the description kebab-cased — e.g. `feature/72-agents-md`,
  `bug/99-login-error-persists`. Types in use: `feature`, `fix`, `bug`,
  `chore`, `docs`, `devops`, `ci`.
- **Commits:** follow [Conventional Commits](https://www.conventionalcommits.org)
  — `type(scope): summary` (e.g. `feat: add token revocation`,
  `fix: correct reset-code cooldown`). Not enforced by a hook — keep to it by hand.
  Keep messages as short as possible — ideally a single subject line. Add a body
  only when absolutely needed (e.g. an architectural decision, a breaking change,
  or non-obvious rationale).
- **Pull requests are squash-merged**, so the PR title becomes the single commit
  on the default branch. PR titles follow `Issue {issue-id}: {description}`
  (e.g. `Issue 72: Add AGENTS.md`). This is why `main`'s history shows
  `Issue N: ...` rather than conventional-commit subjects — do **not** infer the
  per-commit format from that history.
- Rebase the branch on its target before opening/updating a PR (see the PR
  template's Definition of Done).

## Agent Behavior

* Follow existing project patterns.
* Prefer minimal, incremental changes.
* Reuse existing abstractions.
* Do not introduce new frameworks or architecture styles unless requested.
* Generate production-ready code by default.
* **Do not write comments for self-evident code** — good code is
  self-documenting through clear names and structure. Only add a comment when
  it explains a non-obvious architectural or design decision (the *why*) that
  a reader cannot infer from the code itself. Do not restate *what* the code
  does, do not add section banners, and do not leave TODO/task-tracking
  comments.
* **XML doc comments (`///`) are required on every public member** —
  public types, methods, properties, events, and constructors — because they
  document API surface for tooling and IDE tooltips. They are not "what"
  comments and are not covered by the rule above. Each block must be
  **complete**: a `<summary>` describing the member's purpose, a `<param>`
  for every parameter, a `<returns>` when the member returns a value, and a
  `<exception>` for each exception it can throw. Partial doc comments
  (summary only, or documenting some parameters but not others) are not
  acceptable. Non-public members (`private`, `internal`, `protected`) do not
  require doc comments, but if you add one it must be complete under the same
  rules.
* After making changes, **offer to verify** them end-to-end (build, tests, or
  the affected flow) before the user commits.
* Once verification passes with no issues, **give the user a ready-to-use pull
  request title and description** — the title in `Issue {issue-id}: {description}`
  form, the description following `.github/pull_request_template.md`.
