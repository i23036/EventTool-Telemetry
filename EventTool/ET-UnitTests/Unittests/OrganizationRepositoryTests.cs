using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using ET_Backend.Models;
using ET_Backend.Repository.Organization;
using Xunit;
using ET_Backend.Repository.Person;

namespace ET_UnitTests.Unittests
{
    public class OrganizationRepositoryTests
    {
        private IDbConnection CreateInMemoryDb()
        {
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();

            // Tabellenstruktur minimal für die Tests anlegen
            conn.Execute("CREATE TABLE Organizations (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Description TEXT, Domain TEXT, OrgaPicAsBase64 TEXT)");
            conn.Execute("CREATE TABLE Accounts (Id INTEGER PRIMARY KEY AUTOINCREMENT, Email TEXT, UserId INTEGER, IsVerified INTEGER)");
            conn.Execute("CREATE TABLE Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Firstname TEXT, Lastname TEXT, Password TEXT)");
            conn.Execute("CREATE TABLE OrganizationMembers (AccountId INTEGER, OrganizationId INTEGER, Role INTEGER)");


            return conn;
        }

        [Fact]
        public async Task OrganizationExists_ById_ReturnsFalse_WhenNotExists()
        {
            using var db = CreateInMemoryDb();
            var repo = new OrganizationRepository(db);

            var result = await repo.OrganizationExists(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
        }

        [Fact]
        public async Task OrganizationExists_ById_ReturnsTrue_WhenExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            var repo = new OrganizationRepository(db);

            var result = await repo.OrganizationExists(1);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }
        /*
        // HIER NOCH NACHARBEITEN !!!! SCHLÄGT FEHL !!!!
        [Fact]
        public async Task CreateOrganization_CreatesAndReturnsOrganization()
        {
            using var db = CreateInMemoryDb();
            var repo = new OrganizationRepository(db);

            var result = await repo.CreateOrganization(
                "TestOrg",
                "Beschreibung",
                "test.org",
                "Max",
                "Mustermann",
                "max@test.org",
                "geheim"
            );

            // Prüfe, ob der Datensatz wirklich in der DB ist
            var count = db.ExecuteScalar<long>("SELECT COUNT(1) FROM Organizations WHERE Domain = 'test.org'");
            Assert.True(count > 0, "Organization wurde nicht in die DB geschrieben!");

            Assert.True(result.IsSuccess, string.Join(", ", result.Errors.Select(e => e.Message)));
            Assert.Equal("TestOrg", result.Value.Name);
            Assert.Equal("Beschreibung", result.Value.Description);
            Assert.Equal("test.org", result.Value.Domain);
        }
        */

        [Fact]
        public async Task DeleteOrganization_RemovesOrganization()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            var repo = new OrganizationRepository(db);

            var result = await repo.DeleteOrganization(1);

            Assert.True(result.IsSuccess);

            var count = await db.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM Organizations WHERE Id = 1");
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task EditOrganization_UpdatesOrganizationData()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            var repo = new OrganizationRepository(db);

            var org = new Organization { Id = 1, Name = "Neu", Description = "NeuDesc", Domain = "neu.org" };
            var result = await repo.EditOrganization(org);

            Assert.True(result.IsSuccess);

            var updated = await db.QuerySingleAsync<dynamic>("SELECT Name, Description, Domain FROM Organizations WHERE Id = 1");
            Assert.Equal("Neu", (string)updated.Name);
            Assert.Equal("NeuDesc", (string)updated.Description);
            Assert.Equal("neu.org", (string)updated.Domain);
        }

        [Fact]
        public async Task GetOrganization_ReturnsOrganization_WhenExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            var repo = new OrganizationRepository(db);

            var result = await repo.GetOrganization(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Org", result.Value.Name);
        }

        [Fact]
        public async Task GetUser_ReturnsFail_WhenUserDoesNotExist()
        {
            using var db = CreateInMemoryDb();
            var repo = new UserRepository(db);

            var result = await repo.GetUser(999);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task CreateUser_ReturnsFail_OnSqlException()
        {
            using var db = CreateInMemoryDb();
            // Tabelle absichtlich löschen, um einen Fehler zu provozieren
            db.Execute("DROP TABLE Users");
            var repo = new UserRepository(db);

            var result = await repo.CreateUser("Max", "Mustermann", "pw");

            Assert.True(result.IsFailed);
            Assert.Contains("DBError", result.Errors.First().Message);
        }
        [Fact]
        public async Task GetAllOrganizations_ReturnsAllOrganizations()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Name, Description, Domain) VALUES ('Org1', 'Desc1', 'org1.org'), ('Org2', 'Desc2', 'org2.org')");
            var repo = new OrganizationRepository(db);

            var result = await repo.GetAllOrganizations();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetOrganization_ByDomain_ReturnsOrganization()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Name, Description, Domain) VALUES ('DemoOrg', 'DemoDesc', 'demo.org')");
            var repo = new OrganizationRepository(db);

            var result = await repo.GetOrganization("demo.org");

            Assert.True(result.IsSuccess);
            Assert.Equal("DemoOrg", result.Value.Name);
        }

        [Fact]
        public async Task GetMembersByDomain_ReturnsMembers_ForDemoOrg()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'DemoOrg', 'DemoDesc', 'demo.org')");
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            db.Execute("INSERT INTO Accounts (Id, Email, UserId, IsVerified) VALUES (1, 'max@demo.org', 1, 1)");
            db.Execute("INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role) VALUES (1, 1, 1)");
            var repo = new OrganizationRepository(db);

            var result = await repo.GetMembersByDomain("demo.org");

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal("max@demo.org", result.Value[0].Email);
        }

        [Fact]
        public async Task UpdateOrganization_UpdatesOrganizationData()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Alt', 'AltDesc', 'alt.org')");
            var repo = new OrganizationRepository(db);

            var dto = new ET.Shared.DTOs.OrganizationDto("Neu","neu.org", "NeuDesc", "bild1", "Gerhard", "Müller", "gm@demo.org", "verschluesselt");


            var result = await repo.UpdateOrganization("alt.org", dto);

            Assert.True(result.IsSuccess);

            var updated = await db.QuerySingleAsync<dynamic>("SELECT Name, Description, Domain FROM Organizations WHERE Id = 1");
            Assert.Equal("Neu", (string)updated.Name);
            Assert.Equal("NeuDesc", (string)updated.Description);
            Assert.Equal("neu.org", (string)updated.Domain);
        }



    }
}
