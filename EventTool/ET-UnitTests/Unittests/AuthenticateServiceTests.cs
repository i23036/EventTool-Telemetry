using System.Threading.Tasks;
using Xunit;
using Moq;
using ET_Backend.Models;
using ET_Backend.Repository.Organization;
using ET_Backend.Repository.Person;
using ET_Backend.Repository;
using ET_Backend.Services.Helper.Authentication;
using Microsoft.Extensions.Options;
using FluentResults;

public class AuthenticateServiceTests
{
    [Fact]
    public async Task RegisterUser_SuccessfullyRegistersAndStoresUser()
    {
        // Arrange
        var mockAccountRepo = new Mock<IAccountRepository>();
        var mockUserRepo = new Mock<IUserRepository>();
        var mockOrgRepo = new Mock<IOrganizationRepository>();

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audiece = "TestAudience",
            ExperationTime = 1,
            SecretKey = "SuperSecretKey1234567890"
        });

        string firstname = "Max";
        string lastname = "Mustermann";
        string email = "max@firma.de";
        string password = "geheim";
        string domain = "firma.de";

        // Account existiert noch nicht
        mockAccountRepo.Setup(r => r.AccountExists(email))
            .ReturnsAsync(Result.Ok(false));

        // Organisation existiert
        mockOrgRepo.Setup(r => r.OrganizationExists(domain))
            .ReturnsAsync(Result.Ok(true));

        // User wird erstellt
        var user = new User { Firstname = firstname, Lastname = lastname, Password = password, Id = 1 };
        mockUserRepo.Setup(r => r.CreateUser(firstname, lastname, password))
            .ReturnsAsync(Result.Ok(user));

        // Organisation wird geladen
        var org = new Organization { Id = 1, Name = "Firma", Domain = domain };
        mockOrgRepo.Setup(r => r.GetOrganization(domain))
            .ReturnsAsync(Result.Ok(org));

        // Account wird erstellt
        mockAccountRepo.Setup(r => r.CreateAccount(email, org, Role.Member, user))
            .ReturnsAsync(Result.Ok(new Account
            {
                EMail = email,
                User = user,
                Organization = org,
                Role = Role.Member
            }));

        var service = new AuthenticateService(
            mockAccountRepo.Object,
            mockUserRepo.Object,
            mockOrgRepo.Object,
            jwtOptions
        );

        // Act
        var result = await service.RegisterUser(firstname, lastname, email, password);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("User added successfully", result.Value);

        // Überprüfen, ob die Methoden mit den richtigen Parametern aufgerufen wurden
        mockAccountRepo.Verify(r => r.AccountExists(email), Times.Once);
        mockOrgRepo.Verify(r => r.OrganizationExists(domain), Times.Once);
        mockUserRepo.Verify(r => r.CreateUser(firstname, lastname, password), Times.Once);
        mockOrgRepo.Verify(r => r.GetOrganization(domain), Times.Once);
        mockAccountRepo.Verify(r => r.CreateAccount(email, org, Role.Member, user), Times.Once);
    }
}
