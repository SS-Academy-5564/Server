![CodeRabbit Pull Request Reviews](https://img.shields.io/coderabbit/prs/github/SS-Academy-5564/Server?utm_source=oss&utm_medium=github&utm_campaign=SS-Academy-5564%2FServer&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)

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

**Pulse.API** runs DbUp migrations in `Program.cs` on startup (before HTTP requests are accepted). The database is created if it does not exist.

**Pulse.Worker** does not run migrations; it expects the schema to already be applied. Start the API at least once before the Worker, or ensure the database is already migrated.

### Email Configuration (Locally)

Email settings are defined in `appsettings.json` inside **Pulse.API** (see the `Email` section). The sender address and name are configurable via `FromAddress` and `FromName`.

**Local development** — use the `dummy` provider. Emails are not sent; they are logged as JSON instead. No API key is required:

```
{
  "Email": {
    "Provider": "dummy",
    "FromAddress": "noreply@pulse.com",
    "FromName": "Pulse"
  }
}
```

**Real sending via Resend** — use the `resend` provider and supply an API key.

Before switching to the `resend` provider, set up a Resend account:

1. Create an account at [resend.com](https://resend.com).
2. In the Resend dashboard, go to **API Keys** and create a new key.
3. Set a **Name** for the key (for example, `Pulse Local`).
4. Set **Permission** to **Sending access**.
5. Copy the generated API key — it is shown only once.

Then configure the application:

```
{
  "Email": {
    "Provider": "resend",
    "ApiKey": "",
    "FromAddress": "noreply@yourdomain.com",
    "FromName": "Pulse"
  }
}
```

`FromAddress` must use a domain verified in your Resend account.

**API key** — do not commit the key to the repository. Store it in User Secrets or environment variables:

```bash
dotnet user-secrets set "Email:Provider" "resend" --project Pulse.API
dotnet user-secrets set "Email:ApiKey" "re_xxxx" --project Pulse.API
```

Environment variable equivalent: `Email__ApiKey`, `Email__Provider`.

### JWT Secret Configuration

The JWT signing key must not be committed to the repository. Set it via environment variables or User Secrets instead:

```bash
dotnet user-secrets set "Jwt:SecretKey" "your-long-secret-key" --project Pulse.API
```

Environment variable equivalent: `Jwt__SecretKey`.

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

### Run with Docker

`docker-compose.yml` starts **Pulse.API** together with a SQL Server 2022 instance. The API runs migrations on startup, so no manual database setup is needed.

Copy `.env.example` to `.env` and fill in the values, then start all services:

```bash
cp .env.example .env
docker compose up --build
```

The API will be available at `http://localhost:8080`. OpenAPI and Scalar UI are served at `/openapi/v1.json` and `/scalar/v1`. The Worker starts automatically after the API.

All configuration is driven by `.env`. See `.env.example` for available variables.


## Code Formatting

Verify locally:

```bash
dotnet format Pulse.slnx --verify-no-changes
```

Fix violations:

```bash
dotnet format Pulse.slnx
```
