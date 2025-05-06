using Xunit;
using Moq;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ET_Backend.Controllers;
using ET.Shared.DTOs;
using ET_Backend.Services.Helper.Authentication;


namespace ET_UnitTests.Unittests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Login_ReturnsOk_WhenSuccees()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthenticateService>();
            mockAuthService
                .Setup(service => service.LoginUser("test@example.com", "password123"))
                .ReturnsAsync(Result.Ok("MockedToken"));

            var controller = new AuthenticateController(mockAuthService.Object);

            var loginDto = new LoginDto("test@example.com", "password123");

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("MockedToken", okResult.Value);
        }
    }
}
