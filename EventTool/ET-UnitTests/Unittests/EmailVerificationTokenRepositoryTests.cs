using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using ET_Backend.Repository.Authentication;
using Xunit;
using FluentResults;

namespace ET_UnitTests.Unittests
{
    public class EmailVerificationTokenRepositoryTests
    {
        private IDbConnection CreateInMemoryDb()
        {
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();
            conn.Execute("CREATE TABLE EmailVerificationTokens (Id INTEGER PRIMARY KEY AUTOINCREMENT, AccountId INTEGER, Token TEXT, ExpiresAt TEXT)");
            return conn;
        }

        [Fact]
        public async Task CreateAsync_InsertsToken_AndReturnsSuccess()
        {
            // Arrange
            using var db = CreateInMemoryDb();
            var repo = new EmailVerificationTokenRepository(db);
            const int accountId = 1;
            const string token = "test-token-123";

            // Act
            var result = await repo.CreateAsync(accountId, token);

            // Assert
            Assert.True(result.IsSuccess);
            var count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM EmailVerificationTokens WHERE AccountId = @AccountId AND Token = @Token", 
                new { AccountId = accountId, Token = token });
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetAsync_ReturnsTokenData_WhenTokenExists()
        {
            // Arrange
            using var db = CreateInMemoryDb();
            var repo = new EmailVerificationTokenRepository(db);
            const int accountId = 1;
            const string token = "test-token-123";
            DateTime expiresAt = DateTime.UtcNow.AddDays(1);
            await db.ExecuteAsync("INSERT INTO EmailVerificationTokens (AccountId, Token, ExpiresAt) VALUES (@AccountId, @Token, @ExpiresAt)", 
                new { AccountId = accountId, Token = token, ExpiresAt = expiresAt });

            // Act
            var result = await repo.GetAsync(token);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(accountId, result.Value.AccountId);
            // Datumswerte in SQLite können leicht abweichen, daher prüfen wir nur grobes Datum
            Assert.True((result.Value.ExpiresAt - expiresAt).TotalSeconds < 5);
        }

        [Fact]
        public async Task GetAsync_ReturnsFail_WhenTokenNotFound()
        {
            // Arrange
            using var db = CreateInMemoryDb();
            var repo = new EmailVerificationTokenRepository(db);
            
            // Act
            var result = await repo.GetAsync("non-existent-token");

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains("nicht gefunden", result.Errors.First().Message);
        }

        [Fact]
        public async Task ConsumeAsync_DeletesToken_AndReturnsSuccess()
        {
            // Arrange
            using var db = CreateInMemoryDb();
            var repo = new EmailVerificationTokenRepository(db);
            const string token = "test-token-to-delete";
            await db.ExecuteAsync("INSERT INTO EmailVerificationTokens (AccountId, Token, ExpiresAt) VALUES (1, @Token, @ExpiresAt)", 
                new { Token = token, ExpiresAt = DateTime.UtcNow.AddDays(1) });
            
            // Act
            var tx = db.BeginTransaction();
            var result = await repo.ConsumeAsync(token, db, tx);
            tx.Commit();

            // Assert
            Assert.True(result.IsSuccess);
            var count = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM EmailVerificationTokens WHERE Token = @Token", new { Token = token });
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task ConsumeAsync_ReturnsFail_WhenTokenNotFound()
        {
            // Arrange
            using var db = CreateInMemoryDb();
            var repo = new EmailVerificationTokenRepository(db);
            
            // Act
            var tx = db.BeginTransaction();
            var result = await repo.ConsumeAsync("non-existent-token", db, tx);
            tx.Commit();

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains("nicht gefunden", result.Errors.First().Message);
        }
    }
}
