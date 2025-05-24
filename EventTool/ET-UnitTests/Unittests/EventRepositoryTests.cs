using System;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;
using ET_Backend.Models;
using ET_Backend.Repository.Event;
using Xunit;
using Xunit.Abstractions;

namespace ET_UnitTests.Unittests
{
    public class EventRepositoryTests
    {
        private readonly ITestOutputHelper _output;

        public EventRepositoryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private IDbConnection CreateInMemoryDb()
        {
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();

            // TypeHandler manuell registrieren, um DateOnly und TimeOnly zu unterstützen
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

            // Vollständige Events-Tabelle mit allen benötigten Spalten
            conn.Execute(@"CREATE TABLE Events (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                Name TEXT, 
                Description TEXT,
                OrganizationId INTEGER,
                ProcessId INTEGER NULL,
                StartDate TEXT,
                EndDate TEXT,
                StartTime TEXT,
                EndTime TEXT,
                Location TEXT,
                MinParticipants INTEGER,
                MaxParticipants INTEGER,
                RegistrationStart TEXT,
                RegistrationEnd TEXT,
                IsBlueprint INTEGER
            )");

            conn.Execute("CREATE TABLE Organizations (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Description TEXT, Domain TEXT)");
            return conn;
        }




        [Fact]
        public async Task GetEvent_ReturnsFail_WhenNotExists()
        {
            using var db = CreateInMemoryDb();
            var repo = new EventRepository(db);

            var result = await repo.GetEvent(999);

            Assert.False(result.IsSuccess);
            Assert.Contains("NotFound", result.Errors[0].Message);
        }

        

        [Fact]
        public async Task DeleteEvent_RemovesEvent()
        {
            using var db = CreateInMemoryDb();
            db.Execute(@"INSERT INTO Events (
                Id, Name, OrganizationId, Description, StartDate, EndDate, StartTime, EndTime, Location,
                MinParticipants, MaxParticipants, RegistrationStart, RegistrationEnd, IsBlueprint
            ) VALUES (
                1, 'Event1', 1, 'Beschreibung', '2023-01-01', '2023-01-01', '12:00:00', '13:00:00', 'Ort',
                1, 10, '2022-12-01', '2022-12-31', 0
            )");
            var repo = new EventRepository(db);

            var result = await repo.DeleteEvent(1);

            if (!result.IsSuccess)
                _output.WriteLine($"Fehler: {string.Join(", ", result.Errors.Select(e => e.Message))}");

            Assert.True(result.IsSuccess);

            var count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Events WHERE Id = 1");
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsFail_WhenEventNotExists()
        {
            using var db = CreateInMemoryDb();
            var repo = new EventRepository(db);

            var result = await repo.DeleteEvent(999);

            Assert.False(result.IsSuccess);
            Assert.Contains("NotFound", result.Errors[0].Message);
        }

        [Fact]
        public async Task EventExists_ReturnsTrue_WhenExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute(@"INSERT INTO Events (
                Id, Name, OrganizationId, Description, StartDate, EndDate, StartTime, EndTime, Location,
                MinParticipants, MaxParticipants, RegistrationStart, RegistrationEnd, IsBlueprint
            ) VALUES (
                1, 'Event1', 1, 'Beschreibung', '2023-01-01', '2023-01-01', '12:00:00', '13:00:00', 'Ort',
                1, 10, '2022-12-01', '2022-12-31', 0
            )");
            var repo = new EventRepository(db);

            var result = await repo.EventExists(1);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task EventExists_ReturnsFalse_WhenNotExists()
        {
            using var db = CreateInMemoryDb();
            var repo = new EventRepository(db);

            var result = await repo.EventExists(999);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
        }

        // AB HIER NOCH NACHARBEITEN
        /*

        [Fact]
        public async Task GetEvent_ReturnsEvent_WhenExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description) VALUES (1, 'demo.org', 'Beschreibung')");
            db.Execute(@"INSERT INTO Events (
                Id, Name, OrganizationId, Description, StartDate, EndDate, StartTime, EndTime, Location,
                MinParticipants, MaxParticipants, RegistrationStart, RegistrationEnd, IsBlueprint
            ) VALUES (
                1, 'Event1', 1, 'Beschreibung', '2023-01-01', '2023-01-01', '12:00:00', '13:00:00', 'Ort',
                1, 10, '2022-12-01', '2022-12-31', 0
            )");
            var repo = new EventRepository(db);

            var result = await repo.GetEvent(1);

            if (!result.IsSuccess)
                _output.WriteLine($"Fehler: {string.Join(", ", result.Errors.Select(e => e.Message))}");

            Assert.True(result.IsSuccess);
            Assert.Equal("Event1", result.Value.Name);
            Assert.Equal("Beschreibung", result.Value.Description);
        }

        [Fact]
        public async Task GetEventsByOrganizationId_ReturnsEvents()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description) VALUES (1, 'demo.org', 'Beschreibung')");
            db.Execute(@"INSERT INTO Events (
                Name, OrganizationId, Description, StartDate, EndDate, StartTime, EndTime, Location, 
                MinParticipants, MaxParticipants, RegistrationStart, RegistrationEnd, IsBlueprint
            ) VALUES 
            ('Event1', 1, 'Beschreibung', '2023-01-01', '2023-01-01', '12:00:00', '13:00:00', 'Ort', 1, 10, '2022-12-01', '2022-12-31', 0),
            ('Event2', 1, 'Beschreibung', '2023-01-01', '2023-01-01', '12:00:00', '13:00:00', 'Ort', 1, 10, '2022-12-01', '2022-12-31', 0)");

            var repo = new EventRepository(db);

            var result = await repo.GetEventsByOrganizationId(1);

            if (!result.IsSuccess)
                _output.WriteLine($"Fehler: {string.Join(", ", result.Errors.Select(e => e.Message))}");

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("Event1", result.Value[0].Name);
            Assert.Equal("Event2", result.Value[1].Name);
        }

        [Fact]
        public async Task CreateEvent_CreatesAndReturnsEvent()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description) VALUES (1, 'demo.org', 'Beschreibung')");
            var repo = new EventRepository(db);

            var org = new Organization { Id = 1, Name = "Org", Description = "Beschreibung" };
            var result = await repo.CreateEvent("Neues Event", org);

            if (!result.IsSuccess)
                _output.WriteLine($"Fehler: {string.Join(", ", result.Errors.Select(e => e.Message))}");

            Assert.True(result.IsSuccess);
            Assert.Equal("Neues Event", result.Value.Name);

            // Überprüfen, dass das Event tatsächlich in der Datenbank angelegt wurde
            var count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Events WHERE Name = 'Neues Event'");
            Assert.Equal(1, count);
        }
        */

    }
}
