using Microsoft.Data.Sqlite;
using System.Data;
using Dapper;
using ET_Backend.Repository.Person;
using ET_Backend.Repository.Event;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Processes;
using ET_Backend.Services.Helper.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1) Service-Registrierung (vor Build)

// a) Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// b) Dapper + SQLite: IDbConnection
//    Transient: jede Anfrage eine neue Connection
var cs = builder.Configuration.GetConnectionString("Default")
         ?? "Data Source=bitworks.db";
builder.Services.AddTransient<IDbConnection>(_ =>
    new SqliteConnection(cs));

// c) Repository
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();


// d) JWT-Authentifizierung
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBaererOptionsSetup>();


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
      CREATE TABLE IF NOT EXISTS Accounts (
        Email        TEXT    PRIMARY KEY,
        Organization INTEGER NOT NULL,
        Role         INTEGER NOT NULL,
        PasswordHash TEXT    NOT NULL
      );");
}


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.Run();