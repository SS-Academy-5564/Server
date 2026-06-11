using Pulse.BL;
using Pulse.DAL.Database;
using Pulse.DAL.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess()
    .AddBusinessLogic(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
