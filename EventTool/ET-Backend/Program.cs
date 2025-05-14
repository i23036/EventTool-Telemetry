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

var builder = WebApplication.CreateBuilder(args);

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

// ermittelt, ob wir im Development-Modus laufen
bool isDev = builder.Environment.IsDevelopment();

// Dapper + SQLite: IDbConnection
// Transient: jede Anfrage eine neue Connection
var cs = builder.Configuration.GetConnectionString("Default")
          ?? throw new InvalidOperationException("No connection string defined.");

builder.Services.AddTransient<IDbConnection>(sp =>
{
    if (isDev)
    {
        var sqlite = new SqliteConnection(cs);
        sqlite.Open();
        return sqlite;
    }
    else
    {
        var sql = new SqlConnection(cs);
        sql.Open();
        return sql;
    }
});

builder.Services.AddTransient<DatabaseInitializer>();


// Services
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IFileManagementService, FileManagementService>();
builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();
builder.Services.AddScoped<IEMailService, EMailService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<IProcessStepService, ProcessStepService>();


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

// Schema-Initialisierung mit Dapper
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

    if (env.IsDevelopment())
    {
        initializer.DropAllTables();
    }

    initializer.Initialize();
    initializer.SeedDemoData();
}

app.MapControllers();
app.Run();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Testlog: Backend läuft und hat Logging aktiviert.");