using Xunit;
using Moq;
using ET_Backend.Controllers;
using ET_Backend.Services.Event;
using ET_Backend.Models;
using ET.Shared.DTOs;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ET_Backend.Services.Person;
using System.Security.Claims;

namespace ET_UnitTests.Unittests
{
    public class EventControllerTests
    {
        private static Account GetTestUser() =>
            new Account { Id = 1, Organization = new Organization { Id = 2 } };

        [Fact]
        public async Task SubscribeToEvent_ReturnsOk_OnSuccess()
        {
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.SubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(GetTestUser());

            var controller = new EventController(mockEventService.Object, mockUserService.Object);

            var result = await controller.SubscribeToEvent(5);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SubscribeToEvent_ReturnsBadRequest_OnFailure()
        {
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.SubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(GetTestUser());

            var controller = new EventController(mockEventService.Object, mockUserService.Object);

            var result = await controller.SubscribeToEvent(5);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UnsubscribeToEvent_ReturnsOk_OnSuccess()
        {
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.UnsubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(GetTestUser());

            var controller = new EventController(mockEventService.Object, mockUserService.Object);

            var result = await controller.UnsubscribeToEvent(5);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UnsubscribeToEvent_ReturnsBadRequest_OnFailure()
        {
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.UnsubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(GetTestUser());

            var controller = new EventController(mockEventService.Object, mockUserService.Object);

            var result = await controller.UnsubscribeToEvent(5);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task CreateEvent_ReturnsOk_OnSuccess()
        {
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.CreateEvent(It.IsAny<Event>()))
                .ReturnsAsync(Result.Ok(new Event()));

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(GetTestUser());

            var controller = new EventController(mockEventService.Object, mockUserService.Object);

            var dto = new EventDto(
                "Test",
                "Desc",
                DateOnly.Parse("2025-06-01"),
                DateOnly.Parse("2025-06-01"),
                5,
                20,
                false
            );

            var result = await controller.CreateEvent(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsOk_OnSuccess()
        {
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.DeleteEvent(It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(GetTestUser());

            var controller = new EventController(mockEventService.Object, mockUserService.Object);

            var result = await controller.DeleteEvent(7);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsBadRequest_OnFailure()
        {
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(s => s.DeleteEvent(It.IsAny<int>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(GetTestUser());

            var controller = new EventController(mockEventService.Object, mockUserService.Object);

            var result = await controller.DeleteEvent(7);

            Assert.IsType<BadRequestResult>(result);
        }
    }
}
