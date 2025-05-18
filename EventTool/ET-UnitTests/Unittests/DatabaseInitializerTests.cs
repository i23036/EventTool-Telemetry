using Xunit;
using Microsoft.Data.Sqlite;
using System.Data;
using Microsoft.Extensions.Logging;
using ET_Backend.Repository;
using Moq;

namespace ET_UnitTests.Unittests
{
    public class DatabaseInitializerTests
    {
        [Fact]
        public void Initialize_CreatesAllTables()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var logger = new LoggerFactory().CreateLogger<DatabaseInitializer>();
            var initializer = new DatabaseInitializer(connection, logger);

            initializer.Initialize();

            // Prüfe, ob z. B. die Tabelle "Users" existiert
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users';";
            var result = cmd.ExecuteScalar();
            Assert.Equal("Users", result);
        }

        [Fact]
        public void Initialize_ThrowsException_WhenConnectionFails()
        {
            // Mock für IDbConnection, das beim Öffnen eine Exception wirft
            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.State).Returns(ConnectionState.Closed);
            mockConnection.Setup(c => c.Open()).Throws(new InvalidOperationException("Verbindung fehlgeschlagen"));

            var logger = new LoggerFactory().CreateLogger<DatabaseInitializer>();
            var initializer = new DatabaseInitializer(mockConnection.Object, logger);

            Assert.Throws<InvalidOperationException>(() => initializer.Initialize());
        }


        [Fact]
        public void SeedDemoData_InsertsDemoData()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var logger = new LoggerFactory().CreateLogger<DatabaseInitializer>();
            var initializer = new DatabaseInitializer(connection, logger);

            initializer.Initialize();
            initializer.SeedDemoData();

            // Prüfe, ob Demo-User existiert
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Firstname = 'Max' AND Lastname = 'Mustermann';";
                var userCount = (long)cmd.ExecuteScalar();
                Assert.Equal(1, userCount);
            }

            // Prüfe, ob Demo-Organisation existiert
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM Organizations WHERE Name = 'DemoOrg';";
                var orgCount = (long)cmd.ExecuteScalar();
                Assert.Equal(1, orgCount);
            }
        }
    }
}
