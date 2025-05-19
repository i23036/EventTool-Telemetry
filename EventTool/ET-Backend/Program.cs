using Microsoft.Data.Sqlite;
using System.Data;
using Dapper;
using ET.Shared.DTOs;
using ET_Backend.Repository.Person;
using ET_Backend.Repository.Event;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Processes;
using ET_Backend.Services.Administration;
using ET_Backend.Services.Event;
using ET_Backend.Services.Helper;
using ET_Backend.Services.Helper.Authentication;
using ET_Backend.Services.Organization;
using ET_Backend.Services.Person;
using ET_Backend.Services.Processes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using ET_Backend.Repository;
using Microsoft.Data.SqlClient; // für Azure SQL
using Microsoft.Extensions.Logging;
using ET_Backend.Repository.Authentication;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Service-Registrierung (vor Build)

// Controller + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS konfigurieren
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7210",
                "http://localhost:7210",
                "http://localhost:7085",
                "https://nice-field-0026f6403.6.azurestaticapps.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // wegen JWT im Header oder Cookies
    });
});

// Repository
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProcessRepository, ProcessRepository>(); 
builder.Services.AddScoped<IProcessStepRepository, ProcessStepRepository>();

// Dapper + SQLite: IDbConnection
// Transient: jede Anfrage eine neue Connection
builder.Services.AddTransient<IDbConnection>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? throw new InvalidOperationException("No connection string defined.");

    return env.IsDevelopment()
        ? new SqliteConnection(cs)
        : new SqlConnection(cs);
});

builder.Services.AddTransient<DatabaseInitializer>();


// Services
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IFileManagementService, FileManagementService>();
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<IProcessStepService, ProcessStepService>();

// Email-Service
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEMailService, EMailService>();
builder.Services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();

// JWT-Authentifizierung
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBaererOptionsSetup>();

// AdminOnly-Policy zum Organisationen anlegen und löschen
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var http = (Microsoft.AspNetCore.Http.HttpContext?)context.Resource;
            if (http == null) return false;
            var ip = http.Connection.RemoteIpAddress;
            return IPAddress.IsLoopback(ip);
        })
    );
});

var app = builder.Build();






Console.WriteLine($"Aktives Environment: {app.Environment.EnvironmentName}");

// Logger einrichten (nach Build!)
var logger = app.Services.GetRequiredService<ILogger<Program>>();

var connString = app.Configuration.GetConnectionString("Default");

try
{
    using IDbConnection testConn = app.Environment.IsDevelopment()
        ? new SqliteConnection(connString)
        : new SqlConnection(connString);

    testConn.Open();
    var dbType = testConn is SqliteConnection ? "SQLite" : "Azure SQL";
    var version = testConn is SqliteConnection ? "n/a" : ((SqlConnection)testConn).ServerVersion;
    logger.LogInformation("Verbindung zur {dbType} erfolgreich. Version: {version}", dbType, version);
}
catch (Exception ex)
{
    logger.LogError(ex, "Fehler beim Verbindungsaufbau zur Datenbank");
}


// 2) Pipeline- & Schema-Setup (nach Build, vor Run)

// Swagger-Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS früh einbinden
app.UseCors("AllowBlazorClient");

// Auth & AuthZ
app.UseAuthentication();
app.UseAuthorization();

// Schema-Init mit Logging
// Nur für Dev-Umgebung
// Prod-DB (Azure-DB) sollte schon existieren. (Init-Files in Resource-Ordner)
try
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

    if (app.Environment.IsDevelopment())
    {
        initializer.DropAllTables();
        initializer.Initialize();
        initializer.SeedDemoData();
    }
}
catch (Exception ex)
{
    var log = app.Services.GetRequiredService<ILogger<Program>>();
    log.LogError(ex, "Fehler beim Datenbankzugriff während Startup.");
}


app.MapControllers();
app.Run();