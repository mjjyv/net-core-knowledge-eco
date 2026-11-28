using Ecosystem.Application.Common;
using Ecosystem.WebAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Ecosystem.UnitTests.WebAPI;

public class RequestTimingAndLoggingMiddlewareTests
{
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();
        public List<IReadOnlyDictionary<string, object>> Scopes { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            if (state is IReadOnlyDictionary<string, object> dict)
            {
                Scopes.Add(dict);
            }

            return new NoopScope();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }

        private sealed class NoopScope : IDisposable
        {
            public void Dispose() { }
        }
    }

    [Fact]
    public async Task RequestTimingMiddleware_WhenFastRequest_LogsInformation()
    {
        // Arrange
        var logger = new CapturingLogger<RequestTimingMiddleware>();
        var monitorMock = new Mock<IOptionsMonitor<ObservabilityOptions>>();
        monitorMock.Setup(m => m.CurrentValue).Returns(new ObservabilityOptions { SlowRequestThresholdMs = 500 });

        var middleware = new RequestTimingMiddleware(_ => Task.CompletedTask, logger, monitorMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/health";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("Request timing:"));
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task RequestTimingMiddleware_WhenSlowRequest_LogsWarning()
    {
        // Arrange
        var logger = new CapturingLogger<RequestTimingMiddleware>();
        var monitorMock = new Mock<IOptionsMonitor<ObservabilityOptions>>();
        monitorMock.Setup(m => m.CurrentValue).Returns(new ObservabilityOptions { SlowRequestThresholdMs = 30 });

        var middleware = new RequestTimingMiddleware(
            async _ => await Task.Delay(60),
            logger,
            monitorMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/diagnostics/slow-demo";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("Slow HTTP Request detected"));
    }

    [Fact]
    public async Task RequestTimingMiddleware_WhenExceptionThrown_StillLogsTimingInFinally()
    {
        // Arrange
        var logger = new CapturingLogger<RequestTimingMiddleware>();
        var middleware = new RequestTimingMiddleware(
            _ => throw new InvalidOperationException("Boom"),
            logger);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/diagnostics/error-demo";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
        Assert.Single(logger.Entries);
    }

    [Fact]
    public async Task RequestLoggingMiddleware_SanitizesIncomingRequestIdAndOpensBeginScope()
    {
        // Arrange
        var logger = new CapturingLogger<RequestLoggingMiddleware>();
        var middleware = new RequestLoggingMiddleware(_ => Task.CompletedTask, logger);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/v1/articles";
        context.Request.Headers["X-Request-Id"] = "trace-abc\r\nforged-line";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var cleanId = context.Response.Headers["X-Request-Id"].ToString();
        Assert.Equal("trace-abcforged-line", cleanId);
        Assert.Equal("trace-abcforged-line", context.TraceIdentifier);
        Assert.Single(logger.Scopes);
        Assert.Equal("trace-abcforged-line", logger.Scopes[0]["RequestId"]);
        Assert.Equal(2, logger.Entries.Count);
    }
}
