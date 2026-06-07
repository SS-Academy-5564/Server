# GitHub Copilot Instructions

## Project Overview

Pulse is a server-side monitoring platform built with .NET 10.

### Solution Structure

```text
Pulse.slnx
├── Pulse.API         
├── Pulse.BL           
├── Pulse.DAL          
├── Pulse.Worker       
└── Pulse.Tests.Unit   
```

### Getting Started

```bash
cd Server
dotnet restore
dotnet build
dotnet test
```

## Architecture

Follow Clean Architecture principles.

### Dependency Direction

```text
Pulse.API     → Pulse.BL
Pulse.Worker  → Pulse.BL
Pulse.BL      → Pulse.DAL
```

Cross-layer dependencies are forbidden.

### Layer Responsibilities

#### Pulse.API

* Controllers
* Middleware
* Filters
* Request/response mapping
* Dependency injection configuration

Rules:

* Handle HTTP concerns only
* Use DTOs for requests and responses
* Return appropriate HTTP status codes and `ActionResult<T>`
* Do not contain business logic
* Do not access DAL directly

#### Pulse.BL

* Business logic and use cases
* Services
* DTOs
* Validation
* Authorization
* Application exceptions

Rules:

* Contains all business logic
* Performs entity-to-DTO mapping
* Must not access the database directly
* Must not reference API

#### Pulse.DAL

* Dapper queries and commands
* Database entities and enums
* SQL scripts

Rules:

* Contains persistence logic only
* Uses parameterized SQL
* Each query/command performs a single operation
* Each file in Queries/ and Commands/ represents a single domain area

#### Pulse.Worker

* Background jobs
* Scheduled tasks
* Monitoring execution

Rules:

* Uses BL services only
* Supports CancellationToken
* Logs execution and failures

## Dependency Injection

* Use constructor injection
* Register all services through DI
* Do not use service locator pattern
* Do not manually create service dependencies

## Database Access

* Use Dapper and Microsoft.Data.SqlClient
* Create connections only through `IDbConnectionFactory`
* Keep SQL inside DAL
* Read connection strings from configuration only

## Database Migrations

* Database migrations are implemented as SQL scripts in Pulse.DAL/Scripts
* Never modify an existing migration script that has already been committed.
* Migration scripts must be additive and executed in sequence.
* Preserve the existing numbering style in the repository;
* Preserve the existing script naming and numbering conventions.
* Database creation and migration execution are handled by `DatabaseInitializer` and `DatabaseMigration`.
* Pulse.API executes migrations automatically during application startup before accepting HTTP requests.
* Pulse.Worker must not execute migrations and assumes the database schema is already up to date.
* Do not execute migration SQL directly from services, controllers, workers, or application logic.
* Development seed data belongs in Scripts/Dev/Seed and is executed only when enabled through configuration.

## Configuration
* Use `appsettings.json`
* Never commit secrets or credentials

## Async

* Use async/await for all I/O operations
* Async methods must end with `Async`
* Propagate `CancellationToken`
* Avoid `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()`

## Validation & Error Handling

* Validation belongs in BL
* Use dedicated validators
* Throw typed exceptions:

  * `ValidationException`
  * `NotFoundException`
  * `ForbiddenException`
* Handle exceptions globally in API middleware
* Do not use controller-level try/catch for business errors

## Testing

* Use xUnit
* Follow Arrange–Act–Assert
* Mock external dependencies using Moq or NSubstitute

Naming:

```text
MethodName_StateUnderTest_ExpectedBehavior
```

Example:

```csharp
CreateMonitor_EmptyName_ShouldThrowValidationException()
```

## Forbidden

* Business logic in controllers
* Direct DAL access from API or Worker
* Direct `SqlConnection` creation outside `SqlConnectionFactory`
* SQL outside DAL
* Returning database entities from API
* Blocking async calls
* Cross-layer dependencies

## Copilot Behavior

* Follow existing project patterns
* Prefer minimal, incremental changes
* Reuse existing abstractions
* Do not introduce new frameworks or architecture styles unless requested
* Generate production-ready code by default
