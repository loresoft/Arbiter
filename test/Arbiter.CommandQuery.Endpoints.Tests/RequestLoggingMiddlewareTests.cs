using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Arbiter.CommandQuery.Endpoints;

namespace Arbiter.CommandQuery.Endpoints.Tests;

public class RequestLoggingMiddlewareTests
{
    #region Basic Request Logging Tests

    [Test]
    public async Task InvokeAsync_BasicRequest_LogsRequestDetails()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions();
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().HaveCountGreaterThanOrEqualTo(1);
        logger.LoggedMessages.Should().ContainMatch("*GET*/api/test*200*");
    }

    [Test]
    public async Task InvokeAsync_PostRequest_LogsMethodAndPath()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions();
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("POST", "/api/users");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch("*POST*/api/users*");
    }

    [Test]
    [Arguments("GET", "/api/test")]
    [Arguments("POST", "/api/users")]
    [Arguments("PUT", "/api/users/123")]
    [Arguments("DELETE", "/api/users/456")]
    [Arguments("PATCH", "/api/products")]
    public async Task InvokeAsync_DifferentHttpMethods_LogsCorrectMethod(string method, string path)
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions();
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext(method, path);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{method}*{path}*");
    }

    #endregion

    #region Response Status Code Tests

    [Test]
    [Arguments(200)]
    [Arguments(201)]
    [Arguments(204)]
    [Arguments(400)]
    [Arguments(401)]
    [Arguments(403)]
    [Arguments(404)]
    [Arguments(500)]
    [Arguments(503)]
    public async Task InvokeAsync_DifferentStatusCodes_LogsCorrectStatusCode(int statusCode)
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions();
        var middleware = new RequestLoggingMiddleware(
            next: ctx =>
            {
                ctx.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            },
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{statusCode}*");
    }

    #endregion

    #region Request Body Logging Tests

    [Test]
    public async Task InvokeAsync_WithJsonBody_LogsRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var jsonBody = "{\"name\":\"John\",\"age\":30}";
        var context = CreateHttpContext("POST", "/api/users", jsonBody, "application/json");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{jsonBody}*");
    }

    [Test]
    public async Task InvokeAsync_WithXmlBody_LogsRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var xmlBody = "<user><name>John</name></user>";
        var context = CreateHttpContext("POST", "/api/users", xmlBody, "application/xml");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{xmlBody}*");
    }

    [Test]
    public async Task InvokeAsync_WithTextXmlBody_LogsRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var xmlBody = "<data>test</data>";
        var context = CreateHttpContext("POST", "/api/data", xmlBody, "text/xml");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{xmlBody}*");
    }

    [Test]
    public async Task InvokeAsync_WithContentTypeCharset_LogsRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var jsonBody = "{\"test\":\"data\"}";
        var context = CreateHttpContext("POST", "/api/test", jsonBody, "application/json; charset=utf-8");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{jsonBody}*");
    }

    [Test]
    public async Task InvokeAsync_IncludeBodyDisabled_DoesNotLogRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: false);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var jsonBody = "{\"secret\":\"password123\"}";
        var context = CreateHttpContext("POST", "/api/login", jsonBody, "application/json");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().NotContainMatch("*password123*");
    }

    [Test]
    public async Task InvokeAsync_WithNonJsonContentType_DoesNotLogRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var body = "plain text body";
        var context = CreateHttpContext("POST", "/api/upload", body, "text/plain");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().NotContainMatch($"*{body}*");
    }

    [Test]
    public async Task InvokeAsync_EmptyRequestBody_DoesNotLogRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("POST", "/api/test", "", "application/json");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().HaveCountGreaterThanOrEqualTo(1);
        // Should use basic logging format without body
        logger.LoggedMessages.Should().ContainMatch("*POST*/api/test*200*");
    }

    [Test]
    public async Task InvokeAsync_NullContentLength_DoesNotLogRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/test";
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = null;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task InvokeAsync_BodyExceedsMaxSize_DoesNotLogRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true, maxBodySize: 100);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var largeBody = new string('x', 200);
        var context = CreateHttpContext("POST", "/api/upload", largeBody, "application/json");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().NotContainMatch($"*{largeBody}*");
    }

    [Test]
    public async Task InvokeAsync_NoContentType_DoesNotLogRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/test";
        var bytes = Encoding.UTF8.GetBytes("test body");
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        // No ContentType set

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    #endregion

    #region Request Body Stream Position Tests

    [Test]
    public async Task InvokeAsync_AfterReadingBody_ResetsStreamPosition()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var bodyRead = false;

        var middleware = new RequestLoggingMiddleware(
            next: async ctx =>
            {
                // Try to read the body in the next middleware
                using var reader = new StreamReader(ctx.Request.Body);
                var body = await reader.ReadToEndAsync();
                bodyRead = !string.IsNullOrEmpty(body);
            },
            logger: logger,
            options: options
        );

        var jsonBody = "{\"test\":\"data\"}";
        var context = CreateHttpContext("POST", "/api/test", jsonBody, "application/json");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        bodyRead.Should().BeTrue("the request body should be readable by the next middleware");
    }

    #endregion

    #region Timing Tests

    [Test]
    public async Task InvokeAsync_MeasuresElapsedTime()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions();
        var middleware = new RequestLoggingMiddleware(
            next: async _ =>
            {
                await Task.Delay(50); // Simulate some processing time
            },
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().HaveCountGreaterThanOrEqualTo(1);
        // Should contain elapsed time in milliseconds
        logger.LoggedMessages.Should().ContainMatch("* ms*");
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public async Task InvokeAsync_WhenNextThrowsException_StillLogsRequest()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions();
        var middleware = new RequestLoggingMiddleware(
            next: _ => throw new InvalidOperationException("Test exception"),
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await middleware.InvokeAsync(context)
        );

        // Should still log the request even though an exception occurred
        logger.LoggedMessages.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task InvokeAsync_WhenBodyReadFails_ContinuesWithoutBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/test";
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = 100;
        // Use a stream that throws when read
        context.Request.Body = new ThrowingStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Should log without body
        logger.LoggedMessages.Should().HaveCountGreaterThanOrEqualTo(1);
        logger.LoggedMessages.Should().ContainMatch("*POST*/api/test*");
    }

    #endregion

    #region LogLevel Tests

    [Test]
    [Arguments(LogLevel.Trace)]
    [Arguments(LogLevel.Debug)]
    [Arguments(LogLevel.Information)]
    [Arguments(LogLevel.Warning)]
    [Arguments(LogLevel.Error)]
    public async Task InvokeAsync_WithDifferentLogLevels_LogsAtSpecifiedLevel(LogLevel logLevel)
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(logLevel: logLevel);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LogLevel.Should().Be(logLevel);
    }

    #endregion

    #region Custom MIME Types Tests

    [Test]
    public async Task InvokeAsync_WithCustomMimeType_LogsRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var customMimeTypes = new HashSet<string> { "application/custom" };
        var options = CreateOptions(includeRequestBody: true, mimeTypes: customMimeTypes);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var customBody = "custom data format";
        var context = CreateHttpContext("POST", "/api/custom", customBody, "application/custom");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{customBody}*");
    }

    [Test]
    public async Task InvokeAsync_CaseInsensitiveMimeType_LogsRequestBody()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions(includeRequestBody: true);
        var middleware = new RequestLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var jsonBody = "{\"test\":\"data\"}";
        var context = CreateHttpContext("POST", "/api/test", jsonBody, "APPLICATION/JSON");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LoggedMessages.Should().ContainMatch($"*{jsonBody}*");
    }

    #endregion

    #region Helper Methods

    private static IOptions<RequestLoggingOptions> CreateOptions(
        bool includeRequestBody = false,
        int maxBodySize = 10240,
        LogLevel logLevel = LogLevel.Information,
        ISet<string>? mimeTypes = null)
    {
        return Options.Create(new RequestLoggingOptions
        {
            IncludeRequestBody = includeRequestBody,
            RequestBodyMaxSize = maxBodySize,
            LogLevel = logLevel,
            RequestBodyMimeTypes = mimeTypes ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "application/json",
                "application/xml",
                "text/xml"
            }
        });
    }

    private static HttpContext CreateHttpContext(
        string method,
        string path,
        string? body = null,
        string? contentType = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;

        if (string.IsNullOrEmpty(body))
            return context;

        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;

        if (!string.IsNullOrEmpty(contentType))
            context.Request.ContentType = contentType;

        return context;
    }

    #endregion

    #region Test Helpers

    private class FakeLogger<T> : ILogger<T>
    {
        public List<string> LoggedMessages { get; } = new();
        public LogLevel LogLevel { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            LogLevel = logLevel;
            var message = formatter(state, exception);
            LoggedMessages.Add(message);
        }
    }

    private class ThrowingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new IOException("Stream read failed");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    #endregion
}
