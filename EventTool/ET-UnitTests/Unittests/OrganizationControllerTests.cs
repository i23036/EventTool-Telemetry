using Xunit;
using Moq;
using ET_Backend.Controllers;
using ET_Backend.Services.Organization;
using ET.Shared.DTOs;
using ET_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using FluentResults;
using System.Threading.Tasks;

namespace ET_UnitTests.Unittests
{
    public class OrganizationControllerTests
    {
        [Fact]
        public async Task CreateOrganization_ReturnsOk_OnSuccess()
        {
            // Arrange
            var mockService = new Mock<IOrganizationService>();
            var orgDto = new OrganizationDto("TestOrg", "test.org", "Beschreibung", null);
            mockService.Setup(s => s.CreateOrganization(orgDto.Name, orgDto.Domain, orgDto.Description))
                .ReturnsAsync(Result.Ok(new Organization()));

            var controller = new OrganizationController(mockService.Object);

            // Act
            var result = await controller.CreateOrganization(orgDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrganization_ReturnsBadRequest_OnFailure()
        {
            // Arrange
            var mockService = new Mock<IOrganizationService>();
            var orgDto = new OrganizationDto("TestOrg", "test.org", "Beschreibung", null);
            mockService.Setup(s => s.CreateOrganization(orgDto.Name, orgDto.Domain, orgDto.Description))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new OrganizationController(mockService.Object);

            // Act
            var result = await controller.CreateOrganization(orgDto);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetAllOrganizations_ReturnsOk_WithOrganizationDtos()
        {
            var mockService = new Mock<IOrganizationService>();
            var orgList = new List<Organization>
    {
        new Organization { Name = "Org1", Domain = "org1.de", Description = "Desc1", OrgaPicAsBase64 = null },
        new Organization { Name = "Org2", Domain = "org2.de", Description = "Desc2", OrgaPicAsBase64 = null }
    };
            mockService.Setup(s => s.GetAllOrganizations())
                .ReturnsAsync(Result.Ok(orgList));

            var controller = new OrganizationController(mockService.Object);

            var result = await controller.GetAllOrganizations();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<List<OrganizationDto>>(okResult.Value);
            Assert.Equal(2, dtos.Count);
        }


        [Fact]
        public async Task DeleteOrganization_ReturnsOk_OnSuccess()
        {
            var mockService = new Mock<IOrganizationService>();
            mockService.Setup(s => s.DeleteOrganization("org1.de"))
                .ReturnsAsync(Result.Ok());

            var controller = new OrganizationController(mockService.Object);

            var result = await controller.DeleteOrganization("org1.de");

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteOrganization_ReturnsBadRequest_OnFailure()
        {
            var mockService = new Mock<IOrganizationService>();
            mockService.Setup(s => s.DeleteOrganization("org1.de"))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new OrganizationController(mockService.Object);

            var result = await controller.DeleteOrganization("org1.de");

            Assert.IsType<BadRequestResult>(result);
        }

    }
}
