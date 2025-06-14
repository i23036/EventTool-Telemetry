using Dapper;
using ET.Shared.DTOs.Enums;
using System.Data;
using System.Resources;

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
                EventType           TEXT NOT NULL,
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
                Status              INTEGER NOT NULL DEFAULT 0, -- Enum: EventStatus
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
                IsParticipant    INTEGER DEFAULT 0,
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
        // Anlegen der ersten Demo-Organisation und des Admin-Users!
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

        // Zusätzliche Test-Organisationen für die Dev-Umgebung
        var additionalOrgs = new[]
        {
            new { Name = "DemoOrg1", Domain = "demo1.org", Description = "Zweite Demo-Organisation", LogoFile = "1.png" },
            new { Name = "DemoOrg2", Domain = "demo2.org", Description = "Dritte Demo-Organisation", LogoFile = "2.png" },
            new { Name = "DemoOrg3", Domain = "demo3.org", Description = "Vierte Demo-Organisation", LogoFile = "3.png" }
        };

        foreach (var org in additionalOrgs)
        {
            var exists = _db.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Organizations WHERE Domain = @Domain;",
                new { org.Domain }) > 0;

            if (!exists)
            {
                // Logo in Base64 laden
                var logoBase64 = DbUtils.GetBase64FromImage($"Resources/Seed/{org.LogoFile}");

                // Organisation einfügen
                _db.Execute(@"
            INSERT INTO Organizations (Name, Domain, Description, OrgaPicAsBase64)
            VALUES (@Name, @Domain, @Description, @Logo);",
                    new { org.Name, org.Domain, org.Description, Logo = logoBase64 });

                // Account für "Max Mustermann" anlegen – UserId aus Mustermann holen
                var userId = _db.ExecuteScalar<int>("SELECT Id FROM Users WHERE Lastname = 'Mustermann' LIMIT 1;");
                _db.Execute(@"
            INSERT INTO Accounts (Email, IsVerified, UserId)
            VALUES (
                @Email,
                1,
                @UserId
            );",
                    new { Email = $"admin@{org.Domain}", UserId = userId });

                // OrgaMember mit Rolle Owner
                _db.Execute(@"
            INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role)
            VALUES (
                (SELECT Id FROM Accounts WHERE Email = @Email),
                (SELECT Id FROM Organizations WHERE Domain = @Domain),
                0 -- Role.Owner
            );",
                    new { Email = $"admin@{org.Domain}", org.Domain });
            }
        }

        /* ---------- 1. Zusätzliche Accounts für demo.org ---------- */
        // Rollen-Konvention: 0 = Owner | 1 = Organisator | 2 = Member
        var orgId = _db.ExecuteScalar<int>(
        "SELECT Id FROM Organizations WHERE Domain = 'demo.org';");

        void EnsureUser(string first, string last, string email, int role)
        {
            // User
            var usrId = _db.ExecuteScalar<int?>(
                "SELECT Id FROM Users WHERE Firstname = @first AND Lastname = @last;",
                new { first, last });
            if (usrId is null)
            {
                _db.Execute(
                    "INSERT INTO Users (Firstname, Lastname, Password) VALUES (@first, @last, 'demo');",
                    new { first, last });
                usrId = _db.ExecuteScalar<int>(
                    "SELECT Id FROM Users WHERE Firstname = @first AND Lastname = @last;",
                    new { first, last });
            }

            // Account
            var accId = _db.ExecuteScalar<int?>(
                "SELECT Id FROM Accounts WHERE Email = @email;", new { email });
            if (accId is null)
            {
                _db.Execute(
                    "INSERT INTO Accounts (Email, IsVerified, UserId) VALUES (@email, 1, @usrId);",
                    new { email, usrId });
                accId = _db.ExecuteScalar<int>(
                    "SELECT Id FROM Accounts WHERE Email = @email;", new { email });
            }

            // Orga-Mitgliedschaft
            var exists = _db.ExecuteScalar<int>(@"
            SELECT COUNT(1) FROM OrganizationMembers
            WHERE AccountId = @accId AND OrganizationId = @orgId;",
                new { accId, orgId }) > 0;

            if (!exists)
            {
                _db.Execute(@"
                INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role)
                VALUES (@accId, @orgId, @role);",
                    new { accId, orgId, role });
            }
        }

        // 2 Organisatoren
        EnsureUser("Olaf",  "Organizer",  "olaf@demo.org",  1);
        EnsureUser("Olivia","Organizer",  "olivia@demo.org", 1);

        // 3 normale Mitglieder
        EnsureUser("Mia",  "Member", "mia@demo.org",     2);
        EnsureUser("Milan","Member", "milan@demo.org",   2);
        EnsureUser("Mona", "Member", "mona@demo.org",    2);


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

            var orgaId = _db.ExecuteScalar<int>("SELECT Id FROM Organizations WHERE Domain = 'demo.org'");
            var processId = _db.ExecuteScalar<int?>("SELECT Id FROM Processes WHERE Name = 'Onboarding'");

            var parameters = new DynamicParameters();
            parameters.Add("Name", "Kickoff Meeting");
            parameters.Add("EventType", "Workshop");
            parameters.Add("Description", "Erstes Demo-Event");
            parameters.Add("OrgId", orgaId);
            parameters.Add("ProcessId", processId.HasValue ? processId.Value : (object?)null);
            parameters.Add("StartDate", today, DbType.Date);
            parameters.Add("EndDate", today, DbType.Date);
            parameters.Add("StartTime", today.Add(startTime.ToTimeSpan()), DbType.DateTime);
            parameters.Add("EndTime", today.Add(endTime.ToTimeSpan()), DbType.DateTime);
            parameters.Add("Location", "Konferenzraum A");
            parameters.Add("MinParticipants", 5);
            parameters.Add("MaxParticipants", 20);
            parameters.Add("RegStart", today, DbType.Date);
            parameters.Add("RegEnd", today.AddDays(6), DbType.Date);
            parameters.Add("Status", 1); // Status = Offen
            parameters.Add("IsBlueprint", false);

            _db.Execute(@"
            INSERT INTO Events (
                Name, EventType, Description, OrganizationId, ProcessId,
                StartDate, EndDate, StartTime, EndTime, Location,
                MinParticipants, MaxParticipants,
                RegistrationStart, RegistrationEnd, Status, IsBlueprint
            )
            VALUES (
                @Name, @EventType, @Description, @OrgId, @ProcessId,
                @StartDate, @EndDate, @StartTime, @EndTime, @Location,
                @MinParticipants, @MaxParticipants,
                @RegStart, @RegEnd, @Status, @IsBlueprint
            );", parameters);

            // Organisator eintragen
            var eventId = _db.ExecuteScalar<int>("SELECT Id FROM Events WHERE Name = 'Kickoff Meeting'");
            _db.Execute(@"
            INSERT INTO EventMembers (
                AccountId, EventId, IsOrganizer, IsContactPerson, IsParticipant
            )
            VALUES (
                @AccId, @EvtId, 1, 0, 0
            );", 
                new { AccId = _db.ExecuteScalar<int>("SELECT Id FROM Accounts WHERE Email = 'admin@demo.org'"), EvtId = eventId });
        }

        /* ---------- 2. Demo-Events für alle Status ---------- */
        bool eventsSeeded = _db.ExecuteScalar<int>(
        "SELECT COUNT(1) FROM Events WHERE Name LIKE 'Seed_%';") >= 5;

        if (!eventsSeeded)
        {
            var today = DateTime.Today;
            var start = new TimeOnly(10, 0);
            var end   = new TimeOnly(12, 0);

            void InsertEvent(string name, EventStatus status, int max, int min = 0)
            {
                var p = new DynamicParameters();
                p.Add("Name",           $"Seed_{name}");
                p.Add("EventType",      "Workshop");
                p.Add("Description",    $"Seed-Event im Status {status}");
                p.Add("OrgId",          orgId);
                p.Add("ProcessId",      null);
                p.Add("StartDate",      today,                   DbType.Date);
                p.Add("EndDate",        today,                   DbType.Date);
                p.Add("StartTime",      today.Add(start.ToTimeSpan()), DbType.DateTime);
                p.Add("EndTime",        today.Add(end.ToTimeSpan()),   DbType.DateTime);
                p.Add("Location",       "Raum 42");
                p.Add("MinParticipants",min);
                p.Add("MaxParticipants",max);
                p.Add("RegStart",       today,                   DbType.Date);
                p.Add("RegEnd",         today.AddDays(7),        DbType.Date);
                p.Add("Status",         (int)status);
                p.Add("IsBlueprint",    false);

                _db.Execute(@"
                    INSERT INTO Events (
                        Name, EventType, Description, OrganizationId, ProcessId,
                        StartDate, EndDate, StartTime, EndTime, Location,
                        MinParticipants, MaxParticipants,
                        RegistrationStart, RegistrationEnd, Status, IsBlueprint)
                    VALUES (
                        @Name, @EventType, @Description, @OrgId, @ProcessId,
                        @StartDate, @EndDate, @StartTime, @EndTime, @Location,
                        @MinParticipants, @MaxParticipants,
                        @RegStart, @RegEnd, @Status, @IsBlueprint);", p);

                // Owner als Organizer eintragen
               var evtId = _db.ExecuteScalar<int>(
                    "SELECT Id FROM Events WHERE Name = @Name;", new { Name = p.Get<string>("Name") });
                _db.Execute(@"
                    INSERT INTO EventMembers (AccountId, EventId, IsOrganizer)
                    VALUES (
                        (SELECT Id FROM Accounts WHERE Email = 'admin@demo.org'),
                        @EventId, 1);",
                    new { EventId = evtId });
            }

            InsertEvent("Entwurf",     EventStatus.Entwurf,   10);
            InsertEvent("Offen",       EventStatus.Offen,     10);
            InsertEvent("Geschlossen", EventStatus.Geschlossen,10);
            InsertEvent("Abgesagt",    EventStatus.Abgesagt,  10);
            InsertEvent("Archiviert",  EventStatus.Archiviert,10);
        }
    }
}