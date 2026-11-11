using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Common;
using Ecosystem.Domain.Enums;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.Infrastructure.Repositories;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.UnitTests.WebAPI;

public class ArticlesControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly ArticleService _articleService;
    private readonly ArticlesController _controller;
    private readonly CategoriesController _categoriesController;

    private static readonly Guid CategoryCleanArchId = Guid.Parse("11111111-aaaa-c111-1111-111111111111");
    private static readonly Guid SeededArticleId = Guid.Parse("11111111-aaaa-c111-a001-111111111111");

    public ArticlesControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        DatabaseSeeder.SeedInitialDataAsync(_context).GetAwaiter().GetResult();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();

        _unitOfWork = new UnitOfWork(_context);
        _articleService = new ArticleService(_unitOfWork, mapper, NullLogger<ArticleService>.Instance);
        _controller = new ArticlesController(_articleService);
        _categoriesController = new CategoriesController(new TaxonomyService(_unitOfWork), _articleService);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAll_ReturnsOkObjectResult_WithArticles()
    {
        // Act
        var result = await _controller.GetAll(new ArticleFilterQuery());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ArticleResponseDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOk_WhenNotFound_Returns404()
    {
        // Act 1: Exists
        var resultOk = await _controller.GetById(SeededArticleId);
        var okResult = Assert.IsType<OkObjectResult>(resultOk.Result);
        var response = Assert.IsType<ApiResponse<ArticleDetailResponseDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(SeededArticleId, response.Data?.Id);

        // Act 2: Not Found
        var resultNotFound = await _controller.GetById(Guid.NewGuid());
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultNotFound.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetBySlug_WhenExists_ReturnsOk_WhenNotFound_Returns404()
    {
        // Act 1
        var resultOk = await _controller.GetBySlug("huong-dan-thiet-ke-solution-chuan-clean-architecture-trong-net-8");
        Assert.IsType<OkObjectResult>(resultOk.Result);

        // Act 2
        var resultNotFound = await _controller.GetBySlug("non-existent-slug");
        Assert.IsType<NotFoundObjectResult>(resultNotFound.Result);
    }

    [Fact]
    public async Task Create_ValidArticle_Returns201Created()
    {
        // Arrange
        var dto = new ArticleCreateDto
        {
            CategoryId = CategoryCleanArchId,
            Title = "Bài viết kiểm thử RESTful API",
            Summary = "Tóm tắt",
            MarkdownContent = "# Hello World",
            Status = ArticleStatus.Published
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(ArticlesController.GetById), createdResult.ActionName);

        var response = Assert.IsType<ApiResponse<ArticleDetailResponseDto>>(createdResult.Value);
        Assert.True(response.Success);
        Assert.Equal("bai-viet-kiem-thu-restful-api", response.Data?.Slug);
    }

    [Fact]
    public async Task Create_DuplicateSlug_Returns409Conflict()
    {
        // Arrange
        var dto = new ArticleCreateDto
        {
            CategoryId = CategoryCleanArchId,
            Title = "Trùng Slug",
            Slug = "huong-dan-thiet-ke-solution-chuan-clean-architecture-trong-net-8",
            MarkdownContent = "# Duplicate"
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public async Task Create_NonExistentCategory_Returns400BadRequest()
    {
        // Arrange
        var dto = new ArticleCreateDto
        {
            CategoryId = Guid.NewGuid(),
            Title = "Category không tồn tại",
            MarkdownContent = "# Invalid"
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Update_Valid_ReturnsOk_NonExistent_Returns404()
    {
        // Arrange
        var updateDto = new ArticleUpdateDto
        {
            CategoryId = CategoryCleanArchId,
            Title = "Tiêu đề đã sửa qua Controller",
            MarkdownContent = "# Updated Content"
        };

        // Act 1: Valid
        var resultOk = await _controller.Update(SeededArticleId, updateDto);
        var okResult = Assert.IsType<OkObjectResult>(resultOk.Result);
        var response = Assert.IsType<ApiResponse<ArticleDetailResponseDto>>(okResult.Value);
        Assert.Equal("tieu-de-da-sua-qua-controller", response.Data?.Slug);

        // Act 2: Not Found
        var resultNotFound = await _controller.Update(Guid.NewGuid(), updateDto);
        Assert.IsType<NotFoundObjectResult>(resultNotFound.Result);
    }

    [Fact]
    public async Task Delete_Valid_Returns204NoContent_NotFound_Returns404()
    {
        // Act 1: Valid
        var resultNoContent = await _controller.Delete(SeededArticleId);
        Assert.IsType<NoContentResult>(resultNoContent);

        // Act 2: Deleting again returns 404 (filtered out by soft delete)
        var resultNotFound = await _controller.Delete(SeededArticleId);
        Assert.IsType<NotFoundObjectResult>(resultNotFound);
    }

    [Fact]
    public async Task CategoriesController_GetArticlesByCategoryId_ReturnsArticles()
    {
        // Act 1: Valid Category
        var resultOk = await _categoriesController.GetArticlesByCategoryId(CategoryCleanArchId);
        var okResult = Assert.IsType<OkObjectResult>(resultOk.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ArticleResponseDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Single(response.Data!);

        // Act 2: Non-existent Category
        var resultNotFound = await _categoriesController.GetArticlesByCategoryId(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(resultNotFound.Result);
    }
}
