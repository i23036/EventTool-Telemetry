using System.Data;
using System.Resources;
using Dapper;

namespace ET_Backend.Repository;

public class DatabaseInitializer(IDbConnection db, ILogger<DatabaseInitializer> logger)
{
    private readonly IDbConnection _db = db;
    private readonly ILogger _logger = logger;

    public void Initialize()
    {
        try
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            _logger.LogInformation("DB-Verbindung erfolgreich geöffnet.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Öffnen der DB-Verbindung in DatabaseInitializer.");
            throw; // App soll stoppen, wenn keine Verbindung
        }

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS Organizations(
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT NOT NULL,
                Domain      TEXT NOT NULL UNIQUE,
                Description TEXT,
                OrgaPicAsBase64 TEXT
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                Firstname TEXT NOT NULL,
                Lastname  TEXT NOT NULL,
                Password  TEXT NOT NULL
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS Accounts (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                Email          TEXT NOT NULL UNIQUE,
                IsVerified     INTEGER DEFAULT 0,
                UserId         INTEGER NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS Processes (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                Name           TEXT NOT NULL,
                OrganizationId INTEGER NOT NULL,
                FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS Events (
                Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                Name                TEXT NOT NULL,
                Description         TEXT,
                OrganizationId      INTEGER NOT NULL,
                ProcessId           INTEGER,
                StartDate           DATE,
                EndDate             DATE,
                StartTime           TIME,
                EndTime             TIME,
                Location            TEXT,
                MinParticipants     INTEGER,
                MaxParticipants     INTEGER,
                RegistrationStart   DATE,
                RegistrationEnd     DATE,
                IsBlueprint         INTEGER DEFAULT 0,
                FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
                FOREIGN KEY (ProcessId) REFERENCES Processes(Id)
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS EventMembers (
                AccountId        INTEGER NOT NULL,
                EventId          INTEGER NOT NULL,
                IsOrganizer      INTEGER DEFAULT 0,
                IsContactPerson  INTEGER DEFAULT 0,
                PRIMARY KEY (AccountId, EventId),
                FOREIGN KEY (AccountId) REFERENCES Accounts(Id),
                FOREIGN KEY (EventId) REFERENCES Events(Id)
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS OrganizationMembers (
                AccountId        INTEGER NOT NULL,
                OrganizationId   INTEGER NOT NULL,
                Role             INTEGER NOT NULL,
                PRIMARY KEY (AccountId, OrganizationId),
                FOREIGN KEY (AccountId) REFERENCES Accounts(Id),
                FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS Triggers (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                Attribut TEXT NOT NULL
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS ProcessSteps (
                Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                Name           TEXT NOT NULL,
                OrganizationId INTEGER NOT NULL,
                TriggerId      INTEGER,
                FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
                FOREIGN KEY (TriggerId) REFERENCES Triggers(Id)
            );
        ");

        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS EmailVerificationTokens (
                 Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                 AccountId  INTEGER NOT NULL,
                 Token      TEXT NOT NULL UNIQUE,
                 ExpiresAt  TEXT NOT NULL,
                 FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
            );
        ");

        _logger.LogInformation("Datenbank-Tabellen erfolgreich erstellt.");
    }

    public void DropAllTables()
    {
        _db.Execute("DROP TABLE IF EXISTS EmailVerificationTokens;");
        _db.Execute("DROP TABLE IF EXISTS EventMembers;");
        _db.Execute("DROP TABLE IF EXISTS Events;");
        _db.Execute("DROP TABLE IF EXISTS OrganizationMembers;");
        _db.Execute("DROP TABLE IF EXISTS Accounts;");
        _db.Execute("DROP TABLE IF EXISTS ProcessSteps;");
        _db.Execute("DROP TABLE IF EXISTS Processes;");
        _db.Execute("DROP TABLE IF EXISTS Triggers;");
        _db.Execute("DROP TABLE IF EXISTS Users;");
        _db.Execute("DROP TABLE IF EXISTS Organizations;");
    }


    public void SeedDemoData()
    {
        var orgExists = _db.ExecuteScalar<int>("SELECT COUNT(1) FROM Organizations WHERE Domain = 'demo.org';") > 0;
        if (!orgExists)
        {
            var logoBase64 = DbUtils.GetBase64FromImage("Resources/Seed/BitWorksSimpel-Gro.png");

            // Organisation erstellen
            _db.Execute(@"
            INSERT INTO Organizations (Name, Domain, Description, OrgaPicAsBase64)
            VALUES (@Name, @Domain, @Description, @OrgaPicAsBase64);",
                new
                {
                    Name = "DemoOrg",
                    Domain = "demo.org",
                    Description = "Dies ist eine Demo-Organisation",
                    OrgaPicAsBase64 = logoBase64
                });

            // User erstellen
            _db.Execute(@"
            INSERT INTO Users (Firstname, Lastname, Password)
            VALUES ('Max', 'Mustermann', 'demo');");

            // Account erstellen
            _db.Execute(@"
            INSERT INTO Accounts (Email, IsVerified, UserId)
            VALUES (
                'admin@demo.org',
                1,
                (SELECT Id FROM Users WHERE Lastname = 'Mustermann')
            );");

            // OrgaMember mit Rolle Owner
            _db.Execute(@"
            INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role)
            VALUES (
            (SELECT Id FROM Accounts WHERE Email = 'admin@demo.org'),
            (SELECT Id FROM Organizations WHERE Domain = 'demo.org'),
            0 -- Role.Owner
            );");
        }


        // Trigger
        var triggerCount = _db.ExecuteScalar<int>("SELECT COUNT(1) FROM Triggers;");
        if (triggerCount == 0)
        {
            _db.Execute(@"
            INSERT INTO Triggers (Attribut)
            VALUES ('E-Mail bestätigt');
            ");
        }

        // Processes
        var processCount = _db.ExecuteScalar<int>("SELECT COUNT(1) FROM Processes;");
        if (processCount == 0)
        {
            _db.Execute(@"
            INSERT INTO Processes (Name, OrganizationId)
            VALUES ('Onboarding', (SELECT Id FROM Organizations WHERE Domain = 'demo.org'));
            ");
        }

        // ProcessSteps
        var stepCount = _db.ExecuteScalar<int>("SELECT COUNT(1) FROM ProcessSteps;");
        if (stepCount == 0)
        {
            _db.Execute(@"
            INSERT INTO ProcessSteps (Name, OrganizationId, TriggerId)
            VALUES (
                'Willkommen',
                (SELECT Id FROM Organizations WHERE Domain = 'demo.org'),
                (SELECT Id FROM Triggers WHERE Attribut = 'E-Mail bestätigt')
                );
            ");
        }

        // Events
        var eventExists = _db.ExecuteScalar<int>("SELECT COUNT(1) FROM Events;") > 0;
        if (!eventExists)
        {
            var today = DateTime.Today;
            var startTime = new TimeOnly(10, 0);
            var endTime = new TimeOnly(12, 0);

            var orgId = _db.ExecuteScalar<int>("SELECT Id FROM Organizations WHERE Domain = 'demo.org'");
            var processId = _db.ExecuteScalar<int>("SELECT Id FROM Processes WHERE Name = 'Onboarding'");

            var parameters = new DynamicParameters();
            parameters.Add("Name", "Kickoff Meeting");
            parameters.Add("Description", "Erstes Demo-Event");
            parameters.Add("OrgId", orgId);
            parameters.Add("ProcessId", processId);
            parameters.Add("StartDate", today, DbType.Date);
            parameters.Add("EndDate", today, DbType.Date);
            parameters.Add("StartTime", today.Add(startTime.ToTimeSpan()), DbType.DateTime);
            parameters.Add("EndTime", today.Add(endTime.ToTimeSpan()), DbType.DateTime);
            parameters.Add("Location", "Konferenzraum A");
            parameters.Add("MinParticipants", 5);
            parameters.Add("MaxParticipants", 20);
            parameters.Add("RegStart", today, DbType.Date);
            parameters.Add("RegEnd", today.AddDays(6), DbType.Date);
            parameters.Add("IsBlueprint", false);

            _db.Execute(@"
        INSERT INTO Events (
            Name, Description, OrganizationId, ProcessId,
            StartDate, EndDate, StartTime, EndTime, Location,
            MinParticipants, MaxParticipants,
            RegistrationStart, RegistrationEnd, IsBlueprint
        )
        VALUES (
            @Name, @Description, @OrgId, @ProcessId,
            @StartDate, @EndDate, @StartTime, @EndTime, @Location,
            @MinParticipants, @MaxParticipants,
            @RegStart, @RegEnd, @IsBlueprint
        );", parameters);
        }
    }
}