using FluentValidation;
using Pulse.API.Extensions;
using Pulse.BL;
using Pulse.DAL.Database;
using Pulse.DAL.DependencyInjection;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess()
    .AddBusinessLogic(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(typeof(BLAssemblyMarker).Assembly, includeInternalTypes: true);

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddLoginRateLimiter(builder.Configuration);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
}

WebApplication app = builder.Build();

ILogger migrationLogger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("DatabaseMigration");

bool seedDevData = builder.Configuration.GetValue<bool>("Database:SeedDevData");

await DatabaseMigration.RunWithRetryAsync(connectionString, migrationLogger, seedDevData);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseResponseLogging();
app.UseRouting();
app.UseRateLimiter();

app.UseExceptionHandling();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program
{
}
