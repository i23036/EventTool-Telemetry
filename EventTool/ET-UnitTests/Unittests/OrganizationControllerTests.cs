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
    }
}
