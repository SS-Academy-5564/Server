using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Pulse.API.Extensions;
using Pulse.BL;
using Pulse.BL.DependencyInjection;
using Pulse.DAL.Database;
using Pulse.DAL.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess();
builder.Services.AddBusinessLogic(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(typeof(BLAssemblyMarker).Assembly, includeInternalTypes: true);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddNativeOpenApi();
builder.Services.AddLoginRateLimiter(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
}

var app = builder.Build();

var migrationLogger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("DatabaseMigration");

var seedDevData = app.Configuration.GetValue<bool>("Database:SeedDevData");

await DatabaseMigration.RunWithRetryAsync(
    connectionString,
    migrationLogger,
    seedDevData);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Pulse API Documentation");
    });
}

app.UseResponseLogging();
app.UseExceptionHandling();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program
{
}
