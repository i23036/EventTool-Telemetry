using Dapper;
using ET.Shared.DTOs.Enums;
using System.Data;
using ET_Backend.Models.Enums;

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
        Status              INTEGER NOT NULL DEFAULT 0,
        IsBlueprint         INTEGER DEFAULT 0,
        FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
    );
");

        _db.Execute(@"
    CREATE TABLE IF NOT EXISTS Processes (
        Id      INTEGER PRIMARY KEY AUTOINCREMENT,
        EventId INTEGER NOT NULL UNIQUE,
        FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE CASCADE
    );
");
        
        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS ProcessSteps (
                Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                Name              TEXT NOT NULL,
                Trigger           INTEGER NOT NULL,
                Action            INTEGER NOT NULL,
                Offset            INTEGER DEFAULT 0,
                TriggeredByStepId INTEGER,
                ExecutedAt        TEXT NULL,         -- ISO-8601-Zeitstempel
                Subject           TEXT NULL,
                Body              TEXT NULL,
                ProcessId         INTEGER NOT NULL,
                FOREIGN KEY (ProcessId) REFERENCES Processes(Id) ON DELETE CASCADE
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
        // FK-Prüfung ausschalten, damit SQLite nicht meckert
        _db.Execute("PRAGMA foreign_keys = OFF;");

        // ─ Von den abhängigen zu den Basis-Tabellen ─
        _db.Execute("DROP TABLE IF EXISTS EventMembers;");            
        _db.Execute("DROP TABLE IF EXISTS ProcessSteps;");            
        _db.Execute("DROP TABLE IF EXISTS Processes;");               
        _db.Execute("DROP TABLE IF EXISTS Events;");                  
        _db.Execute("DROP TABLE IF EXISTS EmailVerificationTokens;"); 
        _db.Execute("DROP TABLE IF EXISTS OrganizationMembers;");     
        _db.Execute("DROP TABLE IF EXISTS Accounts;");                
        _db.Execute("DROP TABLE IF EXISTS Users;");
        _db.Execute("DROP TABLE IF EXISTS Organizations;");

        // FK-Prüfung wieder aktivieren
        _db.Execute("PRAGMA foreign_keys = ON;");
    }

    public void SeedDemoData()
    {
        SeedOrganizations();
        SeedUsersAndAccounts();
        SeedOrganizationMembers();
        SeedDemoEvents();
    }

// 1) Organisationen + Logos anlegen
private void SeedOrganizations()
{
    var orgs = new[]
    {
        new { Name = "DemoOrg",     Domain = "demo.org",   Desc = "Demo-Organisation", Logo = "BitWorksSimpel-Gro.png" },
        new { Name = "CodeCompany", Domain = "code.org",   Desc = "Softwareunternehmen", Logo = "1.png" },
        new { Name = "Rohrspatzen", Domain = "musik.org",  Desc = "Lokaler Musikverein", Logo = "2.png" },
        new { Name = "Bergschule",  Domain = "schule.org", Desc = "Grundschule", Logo = "3.png" }
    };

    foreach (var o in orgs)
    {
        bool exists = _db.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Organizations WHERE Domain = @Domain;",
            new { o.Domain }) > 0;

        if (!exists)
        {
            var base64 = DbUtils.GetBase64FromImage($"Resources/Seed/{o.Logo}");
            _db.Execute("""
                INSERT INTO Organizations (Name, Domain, Description, OrgaPicAsBase64)
                VALUES (@Name, @Domain, @Desc, @Logo);
            """, new { o.Name, o.Domain, o.Desc, Logo = base64 });
        }
    }
}

// 2) Benutzer + Accounts anlegen (Max + Petra + weitere Demo-User)
private void SeedUsersAndAccounts()
{
    void EnsureUser(string first, string last, string pwd = "demo")
    {
        if (_db.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Users WHERE Firstname=@first AND Lastname=@last;",
                new { first, last }) == 0)
        {
            _db.Execute("INSERT INTO Users (Firstname, Lastname, Password) VALUES (@first,@last,@pwd);",
                        new { first, last, pwd });
        }
    }

    // --- Max Mustermann (nur DemoOrg-Owner) ---------------------------
    EnsureUser("Max", "Owner");
    int maxUid = _db.ExecuteScalar<int>("SELECT Id FROM Users WHERE Firstname='Max' AND Lastname='Owner';");
    _db.Execute("""
        INSERT OR IGNORE INTO Accounts (Email,IsVerified,UserId)
        VALUES ('owner@demo.org',1,@uid);
    """, new { uid = maxUid });
    
    // Zusätzliche Demo-Accounts für DemoOrg (orga1/2 + mem1..3)
    var demoOrgUsers = new[]
    {
        new { First="Olaf",  Last="Organizer",  Mail="orga1@demo.org" },
        new { First="Olivia",Last="Organizer",  Mail="orga2@demo.org" },
        new { First="Mia",   Last="Member",    Mail="mem1@demo.org"  },
        new { First="Milan", Last="Member",    Mail="mem2@demo.org"  },
        new { First="Mona",  Last="Member",    Mail="mem3@demo.org"  }
    };

    foreach (var u in demoOrgUsers)
    {
        EnsureUser(u.First, u.Last);
        EnsureAccount(u.Mail, u.First, u.Last);
    }

    // --- Petra Wechsler (Vorführuser mit 3 Rollen) --------------------
    EnsureUser("Petra", "Wechsler");
    int petraUid = _db.ExecuteScalar<int>("SELECT Id FROM Users WHERE Firstname='Petra' AND Lastname='Wechsler';");

    var petraAccounts = new[]
    {
        "petra@code.org",   // Owner CodeCompany
        "petra@musik.org",  // Organizer Rohrspatzen
        "petra@schule.org"  // Member Bergschule
    };
    foreach (var mail in petraAccounts)
    {
        _db.Execute("""
            INSERT OR IGNORE INTO Accounts (Email,IsVerified,UserId)
            VALUES (@mail,1,@uid);
        """, new { mail, uid = petraUid });
    }

    // --- Spezifische weitere Nutzer ----------------------------------
    EnsureUser("Rolf",   "Refrain");      // Owner Rohrspatzen
    EnsureUser("Bernd",  "Rektor");       // Owner Bergschule
    EnsureUser("Sabine", "Sekretärin");   // Organizer Bergschule
    EnsureUser("Chris",  "Code");         // Organizer CodeCompany
    EnsureUser("Clara",  "Coderin");      // Organizer CodeCompany

    // 9 Bergschul-Mitglieder
    for (int i = 1; i <= 9; i++) EnsureUser($"Schueler{i}", "Berg");
    // 5 Rohrspatzen-Mitglieder
    for (int i = 1; i <= 5; i++) EnsureUser($"Mitglied{i}", "Rohr");
    // 4 CodeCompany-Mitglieder
    for (int i = 1; i <= 4; i++) EnsureUser($"Mitarbeiter{i}", "Code");

    // Accounts zu den neuen Usern (Mail = vorname@domain)
    void EnsureAccount(string email, string first, string last)
    {
        int uId = _db.ExecuteScalar<int>("SELECT Id FROM Users WHERE Firstname=@first AND Lastname=@last;",
                                         new { first, last });
        _db.Execute("INSERT OR IGNORE INTO Accounts (Email,IsVerified,UserId) VALUES (@email,1,@uId);",
                    new { email, uId });
    }
        
    EnsureAccount("dirigent@musik.org",    "Rolf",   "Refrain");
    EnsureAccount("rektor@schule.org",     "Bernd",  "Rektor");
    EnsureAccount("sekretariat@schule.org","Sabine", "Sekretärin");
    EnsureAccount("chris@code.org",        "Chris",  "Code");
    EnsureAccount("clara@code.org",        "Clara",  "Coderin");

    for (int i = 1; i <= 9; i++)
        EnsureAccount($"schueler{i}@schule.org", $"Schueler{i}", "Berg");
    for (int i = 1; i <= 5; i++)
        EnsureAccount($"mitglied{i}@musik.org",  $"Mitglied{i}", "Rohr");
    for (int i = 1; i <= 4; i++)
        EnsureAccount($"mitarbeiter{i}@code.org", $"Mitarbeiter{i}", "Code");
}

// -----------------------------------------------------------------------------
// 3) Mitgliedschaften (OrganizationMembers) mit Rollen zuordnen
// -----------------------------------------------------------------------------
private void SeedOrganizationMembers()
{
    int Owner  = 0, Organizer = 1, Member = 2;

    void Add(string mail, string domain, int role)
    {
        int accId = _db.ExecuteScalar<int>("SELECT Id FROM Accounts WHERE Email=@mail", new { mail });
        int orgId = _db.ExecuteScalar<int>("SELECT Id FROM Organizations WHERE Domain=@domain", new { domain });
        _db.Execute("""
            INSERT OR IGNORE INTO OrganizationMembers (AccountId,OrganizationId,Role)
            VALUES (@accId,@orgId,@role);
        """, new { accId, orgId, role });
    }

    // DemoOrg – Max + 2 Orgas + 3 Member
    Add("owner@demo.org", "demo.org", Owner);
    Add("orga1@demo.org", "demo.org", Organizer);
    Add("orga2@demo.org", "demo.org", Organizer);
    Add("mem1@demo.org",  "demo.org", Member);
    Add("mem2@demo.org",  "demo.org", Member);
    Add("mem3@demo.org",  "demo.org", Member);

    // CodeCompany
    Add("petra@code.org",     "code.org", Owner);
    Add("chris@code.org",     "code.org", Organizer);
    Add("clara@code.org",     "code.org", Organizer);
    for (int i = 1; i <= 4; i++)
        Add($"mitarbeiter{i}@code.org", "code.org", Member);

    // Rohrspatzen
    Add("dirigent@musik.org", "musik.org", Owner);
    Add("petra@musik.org",    "musik.org", Organizer);
    for (int i = 1; i <= 5; i++)
        Add($"mitglied{i}@musik.org", "musik.org", Member);

    // Bergschule
    Add("rektor@schule.org",     "schule.org", Owner);
    Add("sekretariat@schule.org","schule.org", Organizer);
    Add("petra@schule.org",      "schule.org", Member);
    for (int i = 1; i <= 9; i++)
        Add($"schueler{i}@schule.org", "schule.org", Member);
}

// -----------------------------------------------------------------------------
// 4) Demo-Events gemäß Vorgaben anlegen
// -----------------------------------------------------------------------------
private void SeedDemoEvents()
{
    var today  = DateTime.Today;
    var startT = new TimeOnly(10, 0);
    var endT   = new TimeOnly(12, 0);

    // Mail->AccountId Dictionary
    var acc = _db.Query<(int Id, string Email)>("SELECT Id,Email FROM Accounts")
                 .ToDictionary(x => x.Email, x => x.Id);

    foreach (var org in _db.Query<(int Id, string Domain)>("SELECT Id,Domain FROM Organizations"))
    {
        // Event-Definition pro Orga
        var list = new List<(string Title, EventStatus Status, string? OrganizerMail)>();

        switch (org.Domain)
        {
            case "demo.org":
                list.Add(("Planung",       EventStatus.Entwurf,     "orga1@demo.org")); // NICHT Owner
                list.Add(("Workshop",      EventStatus.Offen,       "owner@demo.org"));
                list.Add(("OffenEvent",    EventStatus.Offen,       "orga1@demo.org"));
                list.Add(("Jahrestreffen", EventStatus.Geschlossen, "orga2@demo.org"));
                list.Add(("AbsageEvent",   EventStatus.Abgesagt,    "owner@demo.org"));
                list.Add(("ArchivTest",    EventStatus.Archiviert,  "owner@demo.org"));
                break;

            case "code.org":
                list.Add(("Teammeeting",   EventStatus.Offen,   "petra@code.org"));
                list.Add(("Geheimprojekt", EventStatus.Entwurf, "chris@code.org")); // Entwurf ohne Petra
                list.Add(("Jahresfeier",   EventStatus.Offen,   "chris@code.org"));
                break;

            case "musik.org":
                list.Add(("Jubiläumskonzert", EventStatus.Offen, "petra@musik.org"));
                break;

            case "schule.org":
                list.Add(("Schulfest",  EventStatus.Offen, "rektor@schule.org"));
                list.Add(("Infoabend",  EventStatus.Offen, "sekretariat@schule.org"));
                break;
        }

        foreach (var ev in list)
        {
            string evName = $"{org.Domain}-{ev.Title}";
            var orgId = _db.ExecuteScalar<int>("SELECT Id FROM Organizations WHERE Domain = 'demo.org'");
            
            var p = new DynamicParameters();
            p.Add("Name",             evName);
            p.Add("EventType",        "Standard");
            p.Add("Description",      "Seed-Event");
            p.Add("OrgId",            org.Id);
            p.Add("StartDate",        today);
            p.Add("EndDate",          today);
            p.Add("StartTime",        today.AddDays(10).Add(startT.ToTimeSpan()));
            p.Add("EndTime",          today.AddDays(10).Add(endT.ToTimeSpan()));
            p.Add("Location",         "Online");
            p.Add("MinParticipants",  3);
            p.Add("MaxParticipants",  30);
            p.Add("RegStart",         today);
            p.Add("RegEnd",           today.AddDays(7));
            p.Add("Status",           (int)ev.Status);
            p.Add("IsBlueprint",      false);

            _db.Execute("""
                INSERT INTO Events (
                    Name,EventType,Description,OrganizationId,
                    StartDate,EndDate,StartTime,EndTime,Location,
                    MinParticipants,MaxParticipants,
                    RegistrationStart,RegistrationEnd,Status,IsBlueprint)
                VALUES (
                    @Name,@EventType,@Description,@OrgId,
                    @StartDate,@EndDate,@StartTime,@EndTime,@Location,
                    @MinParticipants,@MaxParticipants,
                    @RegStart,@RegEnd,@Status,@IsBlueprint);
            """, p);

            int evtId = _db.ExecuteScalar<int>("SELECT Id FROM Events WHERE Name=@Name;", new { Name = evName });
            
            // 1) Prozess für dieses Event erzeugen
            int procId = _db.ExecuteScalar<int>(
                "INSERT INTO Processes (EventId) VALUES (@Evt); SELECT last_insert_rowid();",
                new { Evt = evtId });

            // Event mit ProcessId verknüpfen
            _db.Execute("UPDATE Events SET ProcessId=@Pid WHERE Id=@Evt;", new { Pid = procId, Evt = evtId });

            // 2) Nur für demo.org: Auto-Close-Step anlegen
            if (org.Domain == "demo.org")
            {
                _db.Execute(@"
                    INSERT INTO ProcessSteps
                            (Name, Trigger, Action, Offset, TriggeredByStepId, ProcessId)
                    VALUES ('Anmeldung schließen, wenn nur noch 3 Plätze frei sind.',
                            @Trigger, @Action, -3, 0, @Pid);",
                    new {
                        Trigger = (int)ProcessStepTrigger.MaxParticipantsReached,
                        Action  = (int)ProcessStepAction.CloseEvent,
                        Pid     = procId
                    });
            }
            
            int? orgAccId = null;
                
            // Organizer eintragen, sofern Mail vorhanden und Account gefunden
            if (ev.OrganizerMail is not null && acc.TryGetValue(ev.OrganizerMail, out int tmpId))
            {
                orgAccId = tmpId;
                _db.Execute("""
                                INSERT INTO EventMembers (AccountId,EventId,IsOrganizer,IsParticipant)
                                VALUES (@a,@e,1,1);
                            """, new { a = orgAccId, e = evtId });
            }

            // ------------------------ Teilnehmer (max. 2) -----------------------------
            var participants = acc.Values
                .Where(id =>
                    (!orgAccId.HasValue || id != orgAccId.Value) &&               // nicht der Organizer
                    _db.ExecuteScalar<int>(
                        "SELECT COUNT(1) FROM OrganizationMembers WHERE AccountId=@id AND OrganizationId=@oid;",
                        new { id, oid = org.Id }) > 0)
                .Take(2);

            foreach (var pid in participants)
            {
                _db.Execute("""
                                INSERT OR IGNORE INTO EventMembers (AccountId,EventId,IsParticipant)
                                VALUES (@pid,@evtId,1);
                            """, new { pid, evtId });
            }
        }
    }
}
}