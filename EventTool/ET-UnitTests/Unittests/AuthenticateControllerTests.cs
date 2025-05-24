using Microsoft.AspNetCore.Mvc;
using FluentResults;
using ET.Shared.DTOs;
using ET_Backend.Controllers;
using ET_Backend.Services.Helper.Authentication;
using Moq;
using Xunit;

namespace ET_UnitTests.Unittests
{
    public class AuthenticateControllerTests
    {
        [Fact]
        public async Task Login_ReturnsOk_WithToken_WhenLoginSucceeds()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthenticateService>();
            mockAuthService.Setup(s => s.LoginUser("test@demo.org", "password"))
                .ReturnsAsync(Result.Ok("jwt-token-1234"));

            var controller = new AuthenticateController(mockAuthService.Object);
            var loginDto = new LoginDto("test@demo.org", "password");

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var token = Assert.IsType<string>(okResult.Value);
            Assert.Equal("jwt-token-1234", token);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenLoginFails()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthenticateService>();
            mockAuthService.Setup(s => s.LoginUser("invalid@example.com", "password"))
                .ReturnsAsync(Result.Fail("Invalid credentials"));

            var controller = new AuthenticateController(mockAuthService.Object);
            var loginDto = new LoginDto("invalid@example.com", "password");
            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid credentials", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Register_ReturnsOk_WithMessage_WhenRegistrationSucceeds()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthenticateService>();
            mockAuthService.Setup(s => s.RegisterUser("Max", "Muster", "existing@example.com", "password"))
                .ReturnsAsync(Result.Ok("User added successfully")); // <-- Rückgabewert hinzufügen!

            var controller = new AuthenticateController(mockAuthService.Object);
            var registerDto = new RegisterDto("Max", "Muster", "existing@example.com", "password");

            // Act
            var result = await controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }


        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthenticateService>();
            mockAuthService.Setup(s => s.RegisterUser("Max", "Muster", "existing@example.com", "password"))
                .ReturnsAsync(Result.Fail("Account already exists"));

            var controller = new AuthenticateController(mockAuthService.Object);
            var registerDto = new RegisterDto("Max", "Muster", "existing@example.com", "password");


            // Act
            var result = await controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Account already exists", badRequestResult.Value.ToString());
        }
    }
}
