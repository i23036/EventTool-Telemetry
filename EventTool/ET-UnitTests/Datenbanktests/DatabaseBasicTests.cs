using System.Data;
using System.Linq;
using Dapper;
using ET_Backend.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Xunit;
using FluentAssertions;

public class DatabaseBasicTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection;

    // Der DatabseInitializer wird verwendet um alle Tabellen zu erstellen
    private readonly DatabaseInitializer _initializer;

    public DatabaseBasicTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Dummy-Logger
        var logger = new LoggerFactory().CreateLogger<DatabaseInitializer>();
        _initializer = new DatabaseInitializer(_connection, logger);

        // Tabellen anlegen
        _initializer.Initialize();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _connection.DisposeAsync();

    [Fact]
    public void All_Expected_Tables_Exist()
    {
        var tables = _connection.Query<string>(
            "SELECT name FROM sqlite_master WHERE type='table';"
        ).ToList();

        // Prüfe auf einige zentrale Tabellen
        tables.Should().Contain("Users");
        tables.Should().Contain("Organizations");
        tables.Should().Contain("Accounts");
        tables.Should().Contain("Events");
        tables.Should().Contain("Events");
        tables.Should().Contain("OrganizationMembers");
        tables.Should().Contain("EventMembers");
        tables.Should().Contain("Processes");
        tables.Should().Contain("ProcessSteps");
        tables.Should().Contain("Triggers");
        tables.Should().Contain("EmailVerificationTokens");
    }

    [Fact]
    public void Can_Insert_And_Select_User()
    {
        _connection.Execute("INSERT INTO Users (Firstname, Lastname, Password) VALUES ('Test', 'User', 'pw')");
        var user = _connection.QuerySingle("SELECT * FROM Users WHERE Firstname = 'Test'");
        Assert.Equal("User", user.Lastname);
    }

    [Fact]
    public void Can_Insert_And_Select_Organization()
    {
        _connection.Execute("INSERT INTO Organizations (Name, Domain, Description) VALUES ('TestOrg', 'test.org', 'desc')");
        var org = _connection.QuerySingle("SELECT * FROM Organizations WHERE Domain = 'test.org'");
        Assert.Equal("TestOrg", org.Name);
    }

    [Fact]
    public void Can_Insert_And_Select_Account()
    {
        _connection.Execute("INSERT INTO Users (Firstname, Lastname, Password) VALUES ('A', 'B', 'pw')");
        var userId = _connection.ExecuteScalar<long>("SELECT Id FROM Users WHERE Firstname = 'A'");
        _connection.Execute("INSERT INTO Accounts (Email, UserId) VALUES ('a@b.de', @UserId)", new { UserId = userId });
        var acc = _connection.QuerySingle("SELECT * FROM Accounts WHERE Email = 'a@b.de'");
        Assert.Equal(userId, (long)acc.UserId);
    }

    [Fact]
    public void Can_Insert_Read_And_Print_User()
    {
        // User anlegen
        _connection.Execute("INSERT INTO Users (Firstname, Lastname, Password) VALUES ('Lisa', 'Musterfrau', 'secret')");

        // User auslesen
        var user = _connection.QuerySingle("SELECT * FROM Users WHERE Firstname = 'Lisa'");

        // Ausgabe auf der Konsole
        Console.WriteLine($"User: Id={user.Id}, Firstname={user.Firstname}, Lastname={user.Lastname}, Password={user.Password}");

        // Optional: Assertion
        Assert.Equal("Musterfrau", user.Lastname);
    }

}
