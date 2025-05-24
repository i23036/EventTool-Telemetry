using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using ET_Backend.Models;
using ET_Backend.Repository.Person;
using Xunit;

namespace ET_UnitTests.Unittests
{
    public class AccountRepositoryTests
    {
        private IDbConnection CreateInMemoryDb()
        {
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();

            // Tabellenstruktur minimal für die Tests anlegen
            conn.Execute("CREATE TABLE Accounts (Id INTEGER PRIMARY KEY AUTOINCREMENT, Email TEXT, UserId INTEGER, IsVerified INTEGER)");
            conn.Execute("CREATE TABLE Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Firstname TEXT, Lastname TEXT, Password TEXT)");
            conn.Execute("CREATE TABLE OrganizationMembers (AccountId INTEGER, OrganizationId INTEGER, Role INTEGER)");
            conn.Execute("CREATE TABLE Organizations (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Description TEXT, Domain TEXT)");

            return conn;
        }
        /// <summary>
        /// Testet, ob die Methode AccountExists() korrekt funktioniert.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AccountExists_ByEmail_ReturnsFalse_WhenAccountDoesNotExist()
        {
            using var db = CreateInMemoryDb();
            var repo = new AccountRepository(db);

            var result = await repo.AccountExists("foo@bar.de");

            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
        }

        /// <summary>
        /// Testet, ob die Methode AccountExists() korrekt funktioniert.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AccountExists_ByEmail_ReturnsTrue_WhenAccountExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Accounts (Email, UserId, IsVerified) VALUES (@Email, 1, 0)", new { Email = "foo@bar.de" });
            var repo = new AccountRepository(db);

            var result = await repo.AccountExists("foo@bar.de");

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        /// <summary>
        /// Testet, ob die Methode AccountExists() korrekt funktioniert.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AccountExists_ById_ReturnsTrue_WhenAccountExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Accounts (Id, Email, UserId, IsVerified) VALUES (@Id, @Email, 1, 0)", new { Id = 5, Email = "foo@bar.de" });
            var repo = new AccountRepository(db);

            var result = await repo.AccountExists(5);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        /// <summary>
        ///     Testet, ob die Methode AccountExists() korrekt funktioniert.
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task DeleteAccount_ByEmail_ReturnsOk_WhenDeleted()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Accounts (Email, UserId, IsVerified) VALUES (@Email, 1, 0)", new { Email = "foo@bar.de" });
            var repo = new AccountRepository(db);

            var result = await repo.DeleteAccount("foo@bar.de");

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteAccount_ById_ReturnsFail_WhenNotFound()
        {
            using var db = CreateInMemoryDb();
            var repo = new AccountRepository(db);

            var result = await repo.DeleteAccount(999);

            Assert.False(result.IsSuccess);
            Assert.Equal("NotFound", result.Errors[0].Message);
        }

        [Fact]
        public async Task CreateAccount_CreatesAccountAndReturnsAccount()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            var repo = new AccountRepository(db);

            var user = new User { Firstname = "Max", Lastname = "Mustermann", Password = "pw" };
            var org = new Organization { Id = 1, Name = "Org", Description = "Desc", Domain = "org.com" };
            var result = await repo.CreateAccount("max@org.com", org, Role.Member, user);

            Assert.True(result.IsSuccess);
            Assert.Equal("max@org.com", result.Value.EMail);
            Assert.Equal("Max", result.Value.User.Firstname);
            Assert.Equal(Role.Member, result.Value.Role);
        }

        [Fact]
        public async Task EditAccount_UpdatesAccountData()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            db.Execute("INSERT INTO Accounts (Id, Email, UserId, IsVerified) VALUES (1, 'max@org.com', 1, 0)");
            db.Execute("INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role) VALUES (1, 1, 2)");
            var repo = new AccountRepository(db);

            var account = new Account
            {
                Id = 1,
                EMail = "neu@org.com",
                IsVerified = true,
                User = new User { Id = 1, Firstname = "Moritz", Lastname = "Muster", Password = "pw2" },
                Organization = new Organization { Id = 1 },
                Role = Role.Organizer
            };

            var result = await repo.EditAccount(account);

            Assert.True(result.IsSuccess);

            var updated = await db.QuerySingleAsync<dynamic>("SELECT Email, IsVerified FROM Accounts WHERE Id = 1");
            Assert.Equal("neu@org.com", (string)updated.Email);
            Assert.Equal(1, (long)updated.IsVerified);
        }

        [Fact]
        public async Task RemoveFromOrganization_UpdatesOrganizationIdToNull()
        {
            using var db = CreateInMemoryDb();
            // Achtung: Die Methode setzt OrganizationId auf NULL, aber das Feld gibt es in deiner Accounts-Tabelle nicht!
            // Passe ggf. die Tabelle an, falls du RemoveFromOrganization testen willst:
            db.Execute("DROP TABLE Accounts");
            db.Execute("CREATE TABLE Accounts (Id INTEGER PRIMARY KEY AUTOINCREMENT, Email TEXT, UserId INTEGER, IsVerified INTEGER, OrganizationId INTEGER)");
            db.Execute("INSERT INTO Accounts (Id, Email, UserId, IsVerified, OrganizationId) VALUES (1, 'foo@bar.de', 1, 0, 2)");
            var repo = new AccountRepository(db);

            var result = await repo.RemoveFromOrganization(1,1);

            Assert.True(result.IsSuccess);

            var orgId = await db.QuerySingleAsync<int?>("SELECT OrganizationId FROM Accounts WHERE Id = 1");
            Assert.Null(orgId);
        }

        [Fact]
        public async Task GetAccount_ByEmail_ReturnsAccount()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            db.Execute("INSERT INTO Accounts (Id, Email, UserId, IsVerified) VALUES (1, 'max@org.com', 1, 1)");
            db.Execute("INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role) VALUES (1, 1, 1)");
            var repo = new AccountRepository(db);

            var result = await repo.GetAccount("max@org.com");

            Assert.True(result.IsSuccess);
            Assert.Equal("max@org.com", result.Value.EMail);
            Assert.Equal("Max", result.Value.User.Firstname);
            Assert.Equal("Org", result.Value.Organization.Name);
        }

        [Fact]
        public async Task GetAccount_ById_ReturnsAccount()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Organizations (Id, Name, Description, Domain) VALUES (1, 'Org', 'Desc', 'org.com')");
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            db.Execute("INSERT INTO Accounts (Id, Email, UserId, IsVerified) VALUES (1, 'max@org.com', 1, 1)");
            db.Execute("INSERT INTO OrganizationMembers (AccountId, OrganizationId, Role) VALUES (1, 1, 1)");
            var repo = new AccountRepository(db);

            var result = await repo.GetAccount(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("max@org.com", result.Value.EMail);
            Assert.Equal("Max", result.Value.User.Firstname);
            Assert.Equal("Org", result.Value.Organization.Name);
        }

        [Fact]
        public async Task GetAccount_ByEmail_ReturnsFail_WhenNotExists()
        {
            using var db = CreateInMemoryDb();
            var repo = new AccountRepository(db);

            var result = await repo.GetAccount("notfound@org.com");

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task GetAccount_ById_ReturnsFail_WhenNotExists()
        {
            using var db = CreateInMemoryDb();
            var repo = new AccountRepository(db);

            var result = await repo.GetAccount(999);

            Assert.True(result.IsFailed);
        }

    }
}