using Ecosystem.Application.DTOs;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Common;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.UnitTests.WebAPI;

public class DiagnosticsControllerPhase4Tests
{
    private readonly DiagnosticsController _controller;

    public DiagnosticsControllerPhase4Tests()
    {
        _controller = new DiagnosticsController(
            new LifetimeService(),
            new LifetimeService(),
            new LifetimeService(),
            new LifetimeService(),
            new LifetimeService(),
            new LifetimeService());
    }

    [Fact]
    public void ValidateEvenNumber_WhenValid_ReturnsOkObjectResult()
    {
        // Arrange
        var dto = new EvenNumberDemoDto
        {
            Name = "Even Number Test",
            Value = 42
        };

        // Act
        var result = _controller.ValidateEvenNumber(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var apiResponse = Assert.IsType<ApiResponse<EvenNumberDemoDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(42, apiResponse.Data?.Value);
    }
}
