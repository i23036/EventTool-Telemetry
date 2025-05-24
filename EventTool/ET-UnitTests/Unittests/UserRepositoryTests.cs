using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using ET_Backend.Models;
using ET_Backend.Repository.Person;
using Xunit;

namespace ET_UnitTests.Unittests
{
    public class UserRepositoryTests
    {
        private IDbConnection CreateInMemoryDb()
        {
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();
            conn.Execute("CREATE TABLE Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Firstname TEXT, Lastname TEXT, Password TEXT)");
            return conn;
        }

        [Fact]
        public async Task UserExists_ReturnsFalse_WhenNotExists()
        {
            using var db = CreateInMemoryDb();
            var repo = new UserRepository(db);

            var result = await repo.UserExists(1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
        }

        [Fact]
        public async Task UserExists_ReturnsTrue_WhenExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            var repo = new UserRepository(db);

            var result = await repo.UserExists(1);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task CreateUser_CreatesAndReturnsUser()
        {
            using var db = CreateInMemoryDb();
            var repo = new UserRepository(db);

            var result = await repo.CreateUser("Max", "Mustermann", "pw");

            Assert.True(result.IsSuccess);
            Assert.Equal("Max", result.Value.Firstname);
            Assert.Equal("Mustermann", result.Value.Lastname);
        }

        [Fact]
        public async Task DeleteUser_RemovesUser()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            var repo = new UserRepository(db);

            var result = await repo.DeleteUser(1);

            Assert.True(result.IsSuccess);

            var count = db.ExecuteScalar<long>("SELECT COUNT(1) FROM Users WHERE Id = 1");
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task EditUser_UpdatesUserData()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            var repo = new UserRepository(db);

            var user = new User { Id = 1, Firstname = "Moritz", Lastname = "Musterfrau", Password = "pw" };
            var result = await repo.EditUser(user);

            Assert.True(result.IsSuccess);

            var updated = db.QuerySingle<dynamic>("SELECT Firstname, Lastname FROM Users WHERE Id = 1");
            Assert.Equal("Moritz", (string)updated.Firstname);
            Assert.Equal("Musterfrau", (string)updated.Lastname);
        }

        [Fact]
        public async Task GetUser_ReturnsUser_WhenExists()
        {
            using var db = CreateInMemoryDb();
            db.Execute("INSERT INTO Users (Id, Firstname, Lastname, Password) VALUES (1, 'Max', 'Mustermann', 'pw')");
            var repo = new UserRepository(db);

            var result = await repo.GetUser(1);

            Assert.True(result.IsSuccess);
            Assert.Equal("Max", result.Value.Firstname);
        }
    }
}
