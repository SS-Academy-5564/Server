using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using Pulse.API.Extensions;
using Pulse.BL;
using Pulse.BL.DependencyInjection;
using Pulse.DAL.Database;
using Pulse.DAL.DependencyInjection;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddDataAccess()
    .AddBusinessLogic(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(typeof(BLAssemblyMarker).Assembly, includeInternalTypes: true);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddNativeOpenApi();
builder.Services.AddPulseRateLimiting(builder.Configuration);
builder.Services.AddJwtAuthentication();
builder.Services.AddCurrentUserService();

string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

string defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

string? migrationConnectionString = builder.Configuration.GetConnectionString("MigrationConnection");

if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(migrationConnectionString))
{
    migrationConnectionString = defaultConnectionString;
}
else if (string.IsNullOrWhiteSpace(migrationConnectionString))
{
    throw new InvalidOperationException("Connection string 'MigrationConnection' is required in non-Development environments, but is missing or empty.");
}

WebApplication app = builder.Build();

ILogger migrationLogger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("DatabaseMigration");

bool seedDevData = builder.Configuration.GetValue<bool>("Database:SeedDevData");

await DatabaseMigration.RunWithRetryAsync(migrationConnectionString, migrationLogger, seedDevData);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Pulse API Documentation"));
}

app.UseResponseLogging();
app.UseExceptionHandling();
app.UseRouting();

// Liveness probe endpoint for platform health checks
app.MapHealthChecks("/health");

app.UseCors("AngularPolicy");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program
{
}
