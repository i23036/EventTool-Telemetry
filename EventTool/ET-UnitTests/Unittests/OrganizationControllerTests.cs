using Xunit;
using Moq;
using ET_Backend.Controllers;
using ET_Backend.Services.Organization;
using ET.Shared.DTOs;
using ET_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using FluentResults;
using System.Collections.Generic;
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
            var orgDto = new OrganizationDto(
                "TestOrg", "test.org", "Beschreibung", null,
                "Max", "Mustermann", "max@example.com", "supersecret"
            );

            mockService.Setup(s =>
                s.CreateOrganization(
                    orgDto.Name,
                    orgDto.Domain,
                    orgDto.Description,
                    orgDto.OwnerFirstName,
                    orgDto.OwnerLastName,
                    orgDto.OwnerEmail,
                    orgDto.InitialPassword
                )
            ).ReturnsAsync(Result.Ok());

            var controller = new OrganizationController(mockService.Object);

            // Act
            var result = await controller.CreateOrganization(orgDto);

            // Assert
            Assert.IsType<OkObjectResult>(result); // <-- geändert!
        }

        [Fact]
        public async Task CreateOrganization_ReturnsBadRequest_OnFailure()
        {
            // Arrange
            var mockService = new Mock<IOrganizationService>();
            var orgDto = new OrganizationDto(
                "TestOrg", "test.org", "Beschreibung", null,
                "Max", "Mustermann", "max@example.com", "supersecret"
            );

            mockService.Setup(s =>
                s.CreateOrganization(
                    orgDto.Name,
                    orgDto.Domain,
                    orgDto.Description,
                    orgDto.OwnerFirstName,
                    orgDto.OwnerLastName,
                    orgDto.OwnerEmail,
                    orgDto.InitialPassword
                )
            ).ReturnsAsync(Result.Fail("Fehler"));

            var controller = new OrganizationController(mockService.Object);

            // Act
            var result = await controller.CreateOrganization(orgDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result); // <-- geändert!
        }

        [Fact]
        public async Task DeleteOrganization_ReturnsOk_OnSuccess()
        {
            // Arrange
            var mockService = new Mock<IOrganizationService>();
            mockService.Setup(s => s.DeleteOrganization("org1.de"))
                .ReturnsAsync(Result.Ok());

            var controller = new OrganizationController(mockService.Object);

            // Act
            var result = await controller.DeleteOrganization("org1.de");

            // Assert
            Assert.IsType<OkResult>(result);

        }

        [Fact]
        public async Task DeleteOrganization_ReturnsBadRequest_OnFailure()
        {
            // Arrange
            var mockService = new Mock<IOrganizationService>();
            mockService.Setup(s => s.DeleteOrganization("org1.de"))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new OrganizationController(mockService.Object);

            // Act
            var result = await controller.DeleteOrganization("org1.de");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result); // <-- geändert!
        }

    }
}
