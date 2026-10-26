using System.Text.Json;
using Ecosystem.Domain.Exceptions;
using Ecosystem.WebAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ecosystem.UnitTests.WebAPI;

public class GlobalExceptionMiddlewareTests
{
    private readonly NullLogger<GlobalExceptionMiddleware> _logger = NullLogger<GlobalExceptionMiddleware>.Instance;

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundExceptionThrown_ShouldReturn404ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/v1/taxonomy/topics/unknown";

        RequestDelegate next = (ctx) => throw new KeyNotFoundException("Tài nguyên không tồn tại.");
        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(404, context.Response.StatusCode);
        Assert.Equal("application/problem+json; charset=utf-8", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(404, doc.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Tài nguyên không tìm thấy", doc.RootElement.GetProperty("title").GetString());
        Assert.Equal("Tài nguyên không tồn tại.", doc.RootElement.GetProperty("detail").GetString());
        Assert.Equal("/api/v1/taxonomy/topics/unknown", doc.RootElement.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentExceptionThrown_ShouldReturn400ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new ArgumentException("Dữ liệu không hợp lệ.");
        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(400, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(400, doc.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Yêu cầu không hợp lệ", doc.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessExceptionThrown_ShouldReturn401ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new UnauthorizedAccessException("Chưa đăng nhập.");
        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledExceptionThrown_ShouldReturn500ProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) => throw new InvalidOperationException("Lỗi máy chủ nghiêm trọng.");
        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(500, doc.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Lỗi máy chủ nội bộ", doc.RootElement.GetProperty("title").GetString());
    }
}
