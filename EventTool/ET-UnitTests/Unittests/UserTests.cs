using ET_Backend.Models;
using Xunit;
using System.Collections.Generic;

namespace ET_UnitTests.Unittests
{
    public class UserTests
    {
        [Fact]
        public void CanCreateUser_WithValidData()
        {
            var user = new User
            {
                Id = 1,
                Lastname = "Mustermann",
                Firstname = "Max",
                Password = "geheim",
                Accounts = new List<Account>()
            };

            Assert.Equal(1, user.Id);
            Assert.Equal("Mustermann", user.Lastname);
            Assert.Equal("Max", user.Firstname);
            Assert.Equal("geheim", user.Password);
            Assert.NotNull(user.Accounts);
            Assert.Empty(user.Accounts);
        }

        [Fact]
        public void Accounts_DefaultsToEmptyList()
        {
            var user = new User();
            Assert.NotNull(user.Accounts);
            Assert.Empty(user.Accounts);
        }

        [Fact]
        public void CanSetAndGetProperties()
        {
            var user = new User();
            user.Id = 42;
            user.Lastname = "Test";
            user.Firstname = "Tester";
            user.Password = "pw";

            Assert.Equal(42, user.Id);
            Assert.Equal("Test", user.Lastname);
            Assert.Equal("Tester", user.Firstname);
            Assert.Equal("pw", user.Password);
        }

        [Fact]
        public void CanAddAccounts()
        {
            var user = new User();
            var account = new Account();
            user.Accounts.Add(account);

            Assert.Single(user.Accounts);
            Assert.Same(account, user.Accounts[0]);
        }
    }
}
