using Ecosystem.Application.DTOs;
using Ecosystem.Application.Models;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Common;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.UnitTests.WebAPI;

public class TopicsControllerTests
{
    private readonly TaxonomyService _taxonomyService;
    private readonly TopicsController _controller;

    public TopicsControllerTests()
    {
        _taxonomyService = new TaxonomyService();
        _controller = new TopicsController(_taxonomyService);
    }

    [Fact]
    public async Task GetAll_ReturnsOkObjectResult_WithTopicsList()
    {
        // Arrange
        var query = new TaxonomyFilterQuery();

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<IEnumerable<TopicResponseDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.NotEmpty(apiResponse.Data);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOkObjectResult_WithTopic()
    {
        // Arrange
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _controller.GetById(topicId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<TopicDetailResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(topicId, apiResponse.Data?.Id);
    }

    [Fact]
    public async Task GetBySlug_WhenExists_ReturnsOkObjectResult_WithTopic()
    {
        // Arrange
        var slug = "lap-trinh-dotnet-aspnet-core";

        // Act
        var result = await _controller.GetBySlug(slug);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<TopicResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(slug, apiResponse.Data?.Slug);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreatedAtActionResult_With201Status()
    {
        // Arrange
        var request = new TopicCreateDto
        {
            Title = "Kiến Trúc Microservices",
            Description = "Phân tán và Docker hóa hệ thống",
            OrderIndex = 4
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(TopicsController.GetById), createdResult.ActionName);

        var apiResponse = Assert.IsType<ApiResponse<TopicResponseDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("kien-truc-microservices", apiResponse.Data?.Slug);
    }

    [Fact]
    public async Task Update_WhenIdMismatch_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        var bodyId = Guid.NewGuid(); // Mismatched
        var request = new TopicUpdateDto
        {
            Id = bodyId,
            Title = "Cập nhật sai ID"
        };

        // Act
        var result = await _controller.Update(routeId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsNoContentResult()
    {
        // Arrange
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new TopicUpdateDto
        {
            Id = topicId,
            Title = "Lập trình C# & ASP.NET Core Đổi Tên",
            OrderIndex = 99
        };

        // Act
        var result = await _controller.Update(topicId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenValid_ReturnsNoContentResult()
    {
        // Arrange
        var topicId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        var result = await _controller.Delete(topicId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Nested_GetAspectsByTopicId_ReturnsOkWithAspects()
    {
        // Arrange
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _controller.GetAspectsByTopicId(topicId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<IEnumerable<AspectResponseDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotEmpty(apiResponse.Data!);
    }

    [Fact]
    public async Task Nested_CreateAspectForTopic_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var topicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new AspectCreateDto
        {
            Title = "Clean Architecture & DDD",
            Description = "Domain Driven Design mẫu mực",
            OrderIndex = 3
        };

        // Act
        var result = await _controller.CreateAspectForTopic(topicId, request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("Aspects", createdResult.ControllerName);
        Assert.Equal("GetById", createdResult.ActionName);

        var apiResponse = Assert.IsType<ApiResponse<AspectResponseDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("clean-architecture-ddd", apiResponse.Data?.Slug);
    }
}
