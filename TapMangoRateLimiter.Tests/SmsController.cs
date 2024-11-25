using Microsoft.AspNetCore.Mvc;
using Moq;
using TapMangoGatekeeper.Controllers;
using TapMangoGatekeeper.Models;
using TapMangoGatekeeper.Services;
using Xunit;

namespace TapMangoGatekeeper.Tests.Controllers
{
    public class SmsControllerTests
    {
        private readonly Mock<IRateLimitService> _mockRateLimitService;
        private readonly SmsController _controller;

        public SmsControllerTests()
        {
            _mockRateLimitService = new Mock<IRateLimitService>();
            _controller = new SmsController(_mockRateLimitService.Object);
        }

        [Fact]
        public void CanSend_ShouldReturnBadRequest_ForInvalidInput()
        {
            // Arrange
            SmsRequest invalidRequest = null;

            // Act
            var result = _controller.CanSend(invalidRequest) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            var response = result.Value as ApiResponse;
            Assert.NotNull(response);
            Assert.Equal("Invalid request data.", response.Message);
        }

        [Fact]
        public void CanSend_ShouldReturnTooManyRequests_WhenRateLimitExceeded()
        {
            // Arrange
            var request = new SmsRequest { PhoneNumber = "1234567890", Message = "Test message" };
            _mockRateLimitService.Setup(x => x.CanSend(request)).Returns(false);

            // Act
            var result = _controller.CanSend(request) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(429, result.StatusCode);
            var response = result.Value as ApiResponse;
            Assert.NotNull(response);
            Assert.Equal("Rate limit exceeded. Try again later.", response.Message);
        }

        [Fact]
        public void CanSend_ShouldReturnOk_WhenWithinRateLimit()
        {
            // Arrange
            var request = new SmsRequest { PhoneNumber = "1234567890", Message = "Test message" };
            _mockRateLimitService.Setup(x => x.CanSend(request)).Returns(true);

            // Act
            var result = _controller.CanSend(request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var response = result.Value as ApiResponse;
            Assert.NotNull(response);
            Assert.Equal("Message can be sent.", response.Message);
        }
    }
}
