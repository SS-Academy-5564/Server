# Pulse

Server-side monitoring solution built with .NET 10.

## Architecture

```
Pulse.slnx
├── Pulse.API               — HTTP endpoints (Controllers, Middleware, Filters)
├── Pulse.BL                — Business logic, DTOs, validation
├── Pulse.DAL               — Database access via Dapper (Queries, Commands, Scripts), entities and enums
├── Pulse.Worker            — Background polling worker
└── Pulse.Tests.Unit        — Unit tests (xUnit)
```

### Dependency Direction

```
Pulse.API     → Pulse.BL
Pulse.Worker  → Pulse.BL
Pulse.BL      → Pulse.DAL
```

### Database Setup Instructions (Locally)

Define the database connection string in `appsettings.json` (inside both **Pulse.API** and **Pulse.Worker**):

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=Pulse;User ID=your_user_id;Password=your_password;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True"
  }
}
```

**Pulse.API** runs DbUp migrations on startup. The database is created if it does not exist.

**Pulse.Worker** does not run migrations; it expects the schema to already be applied. Start the API at least once before the Worker, or ensure the database is already migrated.


## Getting Started

```bash
cd Server
dotnet restore
dotnet build
dotnet test
```

### Run API

```bash
dotnet run --project Pulse.API
```

OpenAPI docs and Scalar UI are available in development mode at `/openapi/v1.json` and `/scalar/v1`.

### Run Worker

```bash
dotnet run --project Pulse.Worker
```


