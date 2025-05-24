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

namespace ET_UnitTests.Unittests
{
    public class EventControllerTests
    {
        /*
        [Fact]
        public async Task EventList_ReturnsOk_WithEventListDtos()
        {
            // Arrange
            var mockService = new Mock<IEventService>();
            var user = new Account { Id = 1, Organization = new Organization { Id = 2 } };
            var events = new List<Event>
            {
                new Event
                {
                    Id = 10,
                    Name = "TestEvent",
                    Description = "Desc",
                    MaxParticipants = 100,
                    Organizers = new List<Account> { user },
                    Participants = new List<Account> { user },
                    Organization = user.Organization
                }
            };
            mockService.Setup(s => s.GetEventsFromOrganization(user.Organization.Id))
                .ReturnsAsync(Result.Ok(events));

            var controller = new EventController(mockService.Object);

            // Act
            var result = await controller.EventList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<List<EventListDto>>(okResult.Value);
            Assert.Single(dtos);
            Assert.Equal("TestEvent", dtos[0].Name);
        }

    
        [Fact]
        public async Task EventList_ReturnsBadRequest_OnFailure()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.GetEventsFromOrganization(It.IsAny<int>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new EventController(mockService.Object);

            var result = await controller.EventList();

            Assert.IsType<BadRequestResult>(result);
        }
        */
        [Fact]
        public async Task SubscribeToEvent_ReturnsOk_OnSuccess()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.SubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            var controller = new EventController(mockService.Object);

            var result = await controller.SubscribeToEvent(5);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SubscribeToEvent_ReturnsBadRequest_OnFailure()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.SubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new EventController(mockService.Object);

            var result = await controller.SubscribeToEvent(5);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UnsubscribeToEvent_ReturnsOk_OnSuccess()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.UnsubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            var controller = new EventController(mockService.Object);

            var result = await controller.UnsubscribeToEvent(5);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UnsubscribeToEvent_ReturnsBadRequest_OnFailure()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.UnsubscribeToEvent(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new EventController(mockService.Object);

            var result = await controller.UnsubscribeToEvent(5);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task CreateEvent_ReturnsOk_OnSuccess()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.CreateEvent(It.IsAny<Event>()))
                .ReturnsAsync(Result.Ok(new Event()));

            var controller = new EventController(mockService.Object);

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

        /*
        [Fact]
        public async Task CreateEvent_ReturnsBadRequest_OnFailure()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.CreateEvent(It.IsAny<Event>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new EventController(mockService.Object);

            var dto = new EventDto(
                "Test",             // Name
                "Desc",             // Description
                DateOnly.Parse("2025-06-01"), // StartDate
                DateOnly.Parse("2025-06-01"), // EndDate
                5,                  // MinParticipants
                20,                 // MaxParticipants
                false               // IsBlueprint
            );

            var result = await controller.CreateEvent(dto);

            Assert.IsType<BadRequestResult>(result);
        }
        */

        [Fact]
        public async Task DeleteEvent_ReturnsOk_OnSuccess()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.DeleteEvent(It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            var controller = new EventController(mockService.Object);

            var result = await controller.DeleteEvent(7);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsBadRequest_OnFailure()
        {
            var mockService = new Mock<IEventService>();
            mockService.Setup(s => s.DeleteEvent(It.IsAny<int>()))
                .ReturnsAsync(Result.Fail("Fehler"));

            var controller = new EventController(mockService.Object);

            var result = await controller.DeleteEvent(7);

            Assert.IsType<BadRequestResult>(result);
        }
    }
}
