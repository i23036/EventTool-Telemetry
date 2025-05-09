using System.Data;
using System.Data.Common;
using System.Xml.Linq;
using Dapper;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Net.Mime.MediaTypeNames;

namespace ET_Backend.Repository;

public class DatabaseInitializer(IDbConnection db)
{
    private readonly IDbConnection _db = db;

    public void Initialize()
    {
        _db.Execute(@"
            CREATE TABLE IF NOT EXISTS Organizations(
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT NOT NULL,
                Domain      TEXT NOT NULL UNIQUE,
                Description TEXT
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
                Role           INTEGER NOT NULL,
                UserId         INTEGER NOT NULL,
                OrganizationId INTEGER NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id),
                FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
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
                StartDate           TEXT,
                EndDate             TEXT,
                StartTime           TEXT,
                EndTime             TEXT,
                Location            TEXT,
                MinParticipants     INTEGER,
                MaxParticipants     INTEGER,
                RegistrationStart   TEXT,
                RegistrationEnd     TEXT,
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
    }

    public void DropAllTables()
    {
        _db.Execute("DROP TABLE IF EXISTS EventMembers;");
        _db.Execute("DROP TABLE IF EXISTS Events;");
        _db.Execute("DROP TABLE IF EXISTS ProcessSteps;");
        _db.Execute("DROP TABLE IF EXISTS Processes;");
        _db.Execute("DROP TABLE IF EXISTS Triggers;");
        _db.Execute("DROP TABLE IF EXISTS Accounts;");
        _db.Execute("DROP TABLE IF EXISTS Users;");
        _db.Execute("DROP TABLE IF EXISTS Organizations;");
    }

    public void SeedDemoData()
    {
        // Insert Organization
        _db.Execute(@"
        INSERT INTO Organizations (Name, Domain, Description)
        VALUES ('DemoOrg', 'demo.org', 'Dies ist eine Demo-Organisation');
    ");

        // Insert User
        _db.Execute(@"
        INSERT INTO Users (Firstname, Lastname, Password)
        VALUES ('Max', 'Mustermann', 'demo');
    ");

        // Insert Trigger
        _db.Execute(@"
        INSERT INTO Triggers (Attribut)
        VALUES ('E-Mail bestätigt');
    ");

        // Insert Process
        _db.Execute(@"
        INSERT INTO Processes (Name, OrganizationId)
        VALUES ('Onboarding', (SELECT Id FROM Organizations WHERE Domain = 'demo.org'));
    ");

        // Insert ProcessStep
        _db.Execute(@"
        INSERT INTO ProcessSteps (Name, OrganizationId, TriggerId)
        VALUES (
            'Willkommen',
            (SELECT Id FROM Organizations WHERE Domain = 'demo.org'),
            (SELECT Id FROM Triggers WHERE Attribut = 'E-Mail bestätigt')
        );
    ");

        // Insert Account
        _db.Execute(@"
        INSERT INTO Accounts (Email, Role, UserId, OrganizationId)
        VALUES (
            'admin@demo.org',
            3, -- Admin
            (SELECT Id FROM Users WHERE Lastname = 'Mustermann'),
            (SELECT Id FROM Organizations WHERE Domain = 'demo.org')
        );
    ");

        // Insert Event
        _db.Execute(@"
        INSERT INTO Events (
            Name, Description, OrganizationId, ProcessId,
            StartDate, EndDate, StartTime, EndTime, Location,
            MinParticipants, MaxParticipants,
            RegistrationStart, RegistrationEnd, IsBlueprint
        )
        VALUES (
            'Kickoff Meeting', 'Erstes Demo-Event',
            (SELECT Id FROM Organizations WHERE Domain = 'demo.org'),
            (SELECT Id FROM Processes WHERE Name = 'Onboarding'),
            '2025-06-01', '2025-06-01', '10:00', '12:00', 'Konferenzraum A',
            5, 20,
            '2025-05-15', '2025-05-31',
            0
        );
    ");

        // Insert EventMember
        _db.Execute(@"
        INSERT INTO EventMembers (AccountId, EventId, IsOrganizer, IsContactPerson)
        VALUES (
            (SELECT Id FROM Accounts WHERE Email = 'admin@demo.org'),
            (SELECT Id FROM Events WHERE Name = 'Kickoff Meeting'),
            1, 1
        );
    ");
    }

}