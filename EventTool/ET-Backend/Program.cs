using Microsoft.Data.Sqlite;
using System.Data;
using Dapper;
using ET_Backend.DTOs;
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

var builder = WebApplication.CreateBuilder(args);

// 1) Service-Registrierung (vor Build)

// a) Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// b) DTOs
// Müssen nicht registriert werden

// c) Models
// Müssen nicht angemeldet werden

// d) Repository
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProcessRepository, ProcessRepository>(); 
builder.Services.AddScoped<IProcessStepRepository, ProcessStepRepository>();


// Dapper + SQLite: IDbConnection
// Transient: jede Anfrage eine neue Connection
var cs = builder.Configuration.GetConnectionString("Default")
         ?? "Data Source=bitworks.db";
builder.Services.AddTransient<IDbConnection>(_ =>
    new SqliteConnection(cs));

// e) Services
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


// d) JWT-Authentifizierung

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBaererOptionsSetup>();

// e) AdminOnly-Policy zum Organisationen anlegen und löschen
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

// a) Swagger-Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// b) Schema-Initialisierung mit Dapper
using (var conn = app.Services.GetRequiredService<IDbConnection>())
{
    conn.Execute(@"

      CREATE TABLE IF NOT EXISTS Organizations (
        Name        TEXT    NOT NULL,
        Description TEXT,
        Domain      TEXT    PRIMARY KEY
      );

      CREATE TABLE IF NOT EXISTS Accounts (
        Email        TEXT    PRIMARY KEY,
        Organization INTEGER NOT NULL,
        Role         INTEGER NOT NULL,
        PasswordHash TEXT    NOT NULL
      );
    ");
}


app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();


app.Run();