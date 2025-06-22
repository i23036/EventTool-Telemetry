using System.Threading.Tasks;
using ET_Backend.Models;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Person;
using ET_Backend.Services.Organization;
using ET.Shared.DTOs;
using FluentResults;
using Moq;
using Xunit;
using ET_Backend.Models.Enums;

namespace ET_UnitTests.Unittests
{
    public class OrganizationServiceTests
    {
        [Fact]
        public async Task GetOrganization_ReturnsDomainResult_WhenCalledWithDomain()
        {
            // Arrange
            var mockOrgRepo = new Mock<IOrganizationRepository>();
            var mockAccRepo = new Mock<IAccountRepository>();

            var org = new Organization { 
                Id = 1, 
                Name = "Test Org", 
                Domain = "test.org",
                Description = "Test Description" 
            };

            mockOrgRepo.Setup(r => r.GetOrganization("test.org"))
                .ReturnsAsync(Result.Ok(org));

            var service = new OrganizationService(mockOrgRepo.Object, mockAccRepo.Object);

            // Act
            var result = await service.GetOrganization("test.org");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Test Org", result.Value.Name);
            Assert.Equal("test.org", result.Value.Domain);
            mockOrgRepo.Verify(r => r.GetOrganization("test.org"), Times.Once);
        }

        [Fact]
        public async Task GetAllOrganizations_ReturnsAllOrganizations()
        {
            // Arrange
            var mockOrgRepo = new Mock<IOrganizationRepository>();
            var mockAccRepo = new Mock<IAccountRepository>();

            var orgs = new List<Organization>
            {
                new Organization { Id = 1, Name = "Org 1", Domain = "org1.com" },
                new Organization { Id = 2, Name = "Org 2", Domain = "org2.com" }
            };

            mockOrgRepo.Setup(r => r.GetAllOrganizations())
                .ReturnsAsync(Result.Ok(orgs));

            var service = new OrganizationService(mockOrgRepo.Object, mockAccRepo.Object);

            // Act
            var result = await service.GetAllOrganizations();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("Org 1", result.Value[0].Name);
            Assert.Equal("Org 2", result.Value[1].Name);
        }

        [Fact]
        public async Task CreateOrganization_CreatesAndReturnsOrganization()
        {
            // Arrange
            var mockOrgRepo = new Mock<IOrganizationRepository>();
            var mockAccRepo = new Mock<IAccountRepository>();

            var org = new Organization { 
                Id = 1, 
                Name = "New Org", 
                Domain = "new.org",
                Description = "New Description" 
            };

            mockOrgRepo.Setup(r => r.CreateOrganization(
                    "New Org", "new.org", "New Description", 
                    "Max", "Mustermann", "max@new.org", "password123"))
                .ReturnsAsync(Result.Ok(org));

            var service = new OrganizationService(mockOrgRepo.Object, mockAccRepo.Object);

            // Act
            var result = await service.CreateOrganization(
                "New Org", "new.org", "New Description",
                "Max", "Mustermann", "max@new.org", "password123");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("New Org", result.Value.Name);
            Assert.Equal("new.org", result.Value.Domain);
        }

        [Fact]
        public async Task UpdateMemberRole_UpdatesRole_WhenMemberAndOrgExist()
        {
            // Arrange
            var mockOrgRepo = new Mock<IOrganizationRepository>();
            var mockAccRepo = new Mock<IAccountRepository>();

            var org = new Organization { Id = 1, Name = "Test Org", Domain = "test.org" };
            var user = new User { Id = 1, Firstname = "Max", Lastname = "Muster" };
            var account = new Account { 
                Id = 1, 
                EMail = "max@test.org", 
                User = user,
                Organization = org,
                Role = Role.Member
            };

            mockOrgRepo.Setup(r => r.GetOrganization("test.org"))
                .ReturnsAsync(Result.Ok(org));

            mockAccRepo.Setup(r => r.GetAccount("max@test.org"))
                .ReturnsAsync(Result.Ok(account));

            mockAccRepo.Setup(r => r.EditAccount(It.IsAny<Account>()))
                .ReturnsAsync(Result.Ok());

            var service = new OrganizationService(mockOrgRepo.Object, mockAccRepo.Object);

            // Act
            var result = await service.UpdateMemberRole("test.org", "max@test.org", (int)Role.Organizer);

            // Assert
            Assert.True(result.IsSuccess);
            mockAccRepo.Verify(r => r.EditAccount(It.Is<Account>(a => a.Role == Role.Organizer)), Times.Once);
        }

        [Fact]
        public async Task RemoveMember_RemovesMember_WhenAccountExists()
        {
            // Arrange
            var mockOrgRepo = new Mock<IOrganizationRepository>();
            var mockAccRepo = new Mock<IAccountRepository>();

            var org = new Organization { Id = 1, Name = "Test Org", Domain = "test.org" };
            var user = new User { Id = 1, Firstname = "Max", Lastname = "Muster" };
            var account = new Account { 
                Id = 1, 
                EMail = "max@test.org", 
                User = user,
                Organization = org,
                Role = Role.Member
            };

            mockOrgRepo.Setup(r => r.GetOrganization("test.org"))
                .ReturnsAsync(Result.Ok(org));

            mockAccRepo.Setup(r => r.GetAccount("max@test.org"))
                .ReturnsAsync(Result.Ok(account));

            mockAccRepo.Setup(r => r.RemoveFromOrganization(1,1))
                .ReturnsAsync(Result.Ok());

            var service = new OrganizationService(mockOrgRepo.Object, mockAccRepo.Object);

            // Act
            var result = await service.RemoveMember("test.org", "max@test.org");

            // Assert
            Assert.True(result.IsSuccess);
            mockAccRepo.Verify(r => r.RemoveFromOrganization(1,1), Times.Once);
        }
    }
}
