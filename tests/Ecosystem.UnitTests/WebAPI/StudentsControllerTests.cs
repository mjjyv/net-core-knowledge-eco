using Ecosystem.Application.DTOs;
using Ecosystem.Domain.Common;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.UnitTests.WebAPI;

public class StudentsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly StudentsController _controller;

    private static readonly Guid SeededStudent1Id = Guid.Parse("33333333-3333-3333-3333-333333333331");

    public StudentsControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        DatabaseSeeder.SeedInitialDataAsync(_context).GetAwaiter().GetResult();

        _controller = new StudentsController(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAll_ReturnsAllStudents_AndSupportsNameFilter()
    {
        // Act 1: All seeded students (2)
        var resultAll = await _controller.GetAll();
        var okAll = Assert.IsType<OkObjectResult>(resultAll.Result);
        var respAll = Assert.IsType<ApiResponse<IEnumerable<StudentResponseDto>>>(okAll.Value);
        Assert.Equal(2, respAll.Data!.Count());

        // Act 2: Filter by name "An"
        var resultFiltered = await _controller.GetAll("An");
        var okFiltered = Assert.IsType<OkObjectResult>(resultFiltered.Result);
        var respFiltered = Assert.IsType<ApiResponse<IEnumerable<StudentResponseDto>>>(okFiltered.Value);
        Assert.Single(respFiltered.Data!);
        Assert.Contains("An", respFiltered.Data!.First().Name);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOk_WhenNotFound_Returns404()
    {
        // Act 1
        var resultOk = await _controller.GetById(SeededStudent1Id);
        var okResult = Assert.IsType<OkObjectResult>(resultOk.Result);
        var response = Assert.IsType<ApiResponse<StudentResponseDto>>(okResult.Value);
        Assert.Equal("Nguyễn Văn An", response.Data?.Name);

        // Act 2
        var resultNotFound = await _controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(resultNotFound.Result);
    }

    [Fact]
    public async Task Create_Update_Delete_FullLifecycle_Succeeds()
    {
        // 1. Create
        var createDto = new StudentCreateDto
        {
            Name = "Phạm Hoàng Long",
            Major = "Trí tuệ nhân tạo",
            GPA = 3.95
        };

        var createResult = await _controller.Create(createDto);
        var createdObj = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        Assert.Equal(201, createdObj.StatusCode);
        var createdResp = Assert.IsType<ApiResponse<StudentResponseDto>>(createdObj.Value);
        var newId = createdResp.Data!.Id;

        // 2. Update
        var updateDto = new StudentUpdateDto
        {
            Name = "Phạm Hoàng Long (Updated)",
            Major = "AI Engineering",
            GPA = 4.0
        };

        var updateResult = await _controller.Update(newId, updateDto);
        Assert.IsType<NoContentResult>(updateResult);

        var getAfterUpdate = await _controller.GetById(newId);
        var okGet = Assert.IsType<OkObjectResult>(getAfterUpdate.Result);
        var getResp = Assert.IsType<ApiResponse<StudentResponseDto>>(okGet.Value);
        Assert.Equal("Phạm Hoàng Long (Updated)", getResp.Data!.Name);
        Assert.Equal(4.0, getResp.Data!.GPA);

        // 3. Delete
        var deleteResult = await _controller.Delete(newId);
        Assert.IsType<NoContentResult>(deleteResult);

        var getAfterDelete = await _controller.GetById(newId);
        Assert.IsType<NotFoundObjectResult>(getAfterDelete.Result);
    }
}
