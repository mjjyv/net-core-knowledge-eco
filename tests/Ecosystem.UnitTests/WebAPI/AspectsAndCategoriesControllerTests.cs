using Ecosystem.Application.DTOs;
using Ecosystem.Application.Models;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Common;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Ecosystem.UnitTests.WebAPI;

public class AspectsAndCategoriesControllerTests
{
    private readonly TaxonomyService _taxonomyService;
    private readonly AspectsController _aspectsController;
    private readonly CategoriesController _categoriesController;

    public AspectsAndCategoriesControllerTests()
    {
        _taxonomyService = new TaxonomyService();
        _aspectsController = new AspectsController(_taxonomyService);
        _categoriesController = new CategoriesController(_taxonomyService);
    }

    [Fact]
    public async Task AspectsController_GetById_WhenExists_ReturnsOk()
    {
        // Arrange
        var aspectId = Guid.Parse("11111111-aaaa-1111-1111-111111111111");

        // Act
        var result = await _aspectsController.GetById(aspectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<AspectDetailResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(aspectId, apiResponse.Data?.Id);
    }

    [Fact]
    public async Task AspectsController_Update_WhenIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var request = new AspectUpdateDto { Id = bodyId, Title = "Aspect sai ID" };

        // Act
        var result = await _aspectsController.Update(routeId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task AspectsController_Update_WhenValid_ReturnsNoContent()
    {
        // Arrange
        var aspectId = Guid.Parse("11111111-aaaa-1111-1111-111111111111");
        var request = new AspectUpdateDto
        {
            Id = aspectId,
            Title = "Nền tảng C# Mới Nhất",
            OrderIndex = 1
        };

        // Act
        var result = await _aspectsController.Update(aspectId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AspectsController_Delete_ReturnsNoContent()
    {
        // Arrange
        var aspectId = Guid.Parse("22222222-aaaa-2222-2222-222222222222");

        // Act
        var result = await _aspectsController.Delete(aspectId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AspectsController_Nested_GetCategories_ReturnsOk()
    {
        // Arrange
        var aspectId = Guid.Parse("11111111-aaaa-1111-1111-111111111111");

        // Act
        var result = await _aspectsController.GetCategoriesByAspectId(aspectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<IEnumerable<CategoryResponseDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotEmpty(apiResponse.Data!);
    }

    [Fact]
    public async Task AspectsController_Nested_CreateCategory_ReturnsCreatedAtAction()
    {
        // Arrange
        var aspectId = Guid.Parse("11111111-aaaa-1111-1111-111111111111");
        var request = new CategoryCreateDto
        {
            Title = "Span<T> and Memory<T> Performance",
            Description = "Tối ưu bộ nhớ đệm",
            OrderIndex = 3
        };

        // Act
        var result = await _aspectsController.CreateCategoryForAspect(aspectId, request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("Categories", createdResult.ControllerName);
        Assert.Equal("GetById", createdResult.ActionName);

        var apiResponse = Assert.IsType<ApiResponse<CategoryResponseDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("spant-and-memoryt-performance", apiResponse.Data?.Slug);
    }

    [Fact]
    public async Task CategoriesController_GetById_WhenExists_ReturnsOk()
    {
        // Arrange
        var catId = Guid.Parse("11111111-aaaa-c111-1111-111111111111");

        // Act
        var result = await _categoriesController.GetById(catId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<CategoryResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(catId, apiResponse.Data?.Id);
    }

    [Fact]
    public async Task CategoriesController_Update_WhenIdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var routeId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var request = new CategoryUpdateDto { Id = bodyId, Title = "Category sai ID" };

        // Act
        var result = await _categoriesController.Update(routeId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CategoriesController_Update_WhenValid_ReturnsNoContent()
    {
        // Arrange
        var catId = Guid.Parse("11111111-aaaa-c111-1111-111111111111");
        var request = new CategoryUpdateDto
        {
            Id = catId,
            Title = "OOP Căn Bản và Nâng Cao",
            OrderIndex = 1
        };

        // Act
        var result = await _categoriesController.Update(catId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CategoriesController_Delete_ReturnsNoContent()
    {
        // Arrange
        var catId = Guid.Parse("22222222-aaaa-c111-2222-222222222222");

        // Act
        var result = await _categoriesController.Delete(catId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
