using Pulse.API.Extensions;
using Pulse.API.Middleware;
using Pulse.DAL.Database;
using Pulse.DAL.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddLoginRateLimiter(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

var app = builder.Build();

var migrationLogger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("DatabaseMigration");

var seedDevData = builder.Configuration.GetValue<bool>("Database:SeedDevData");

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

app.UseAuthorization();

app.MapControllers();

app.Run();
