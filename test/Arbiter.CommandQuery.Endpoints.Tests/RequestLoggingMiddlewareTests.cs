using System.Security.Claims;
using System.Text;
using System.Diagnostics;
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

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticated_UsesAuthenticatedUserContext()
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
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice"),
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastUserName.Should().Be("alice");
        logger.LastUserId.Should().Be("user-123");
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsSetByNextMiddleware_UsesFinalAuthenticatedUserContext()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var options = CreateOptions();

        var middleware = new RequestLoggingMiddleware(
            next: ctx =>
            {
                var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "authorized-user"),
                    new Claim(ClaimTypes.NameIdentifier, "authorized-123")
                ],
                authenticationType: "Policy");
                ctx.User = new ClaimsPrincipal(identity);

                return Task.CompletedTask;
            },
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastUserName.Should().Be("authorized-user");
        logger.LastUserId.Should().Be("authorized-123");
    }

    [Test]
    public async Task InvokeAsync_WhenClaimLoggingAddsAdditionalClaims_IncludesAdditionalClaimsInFinalScope()
    {
        // Arrange
        var logger = new FakeLogger<RequestLoggingMiddleware>();
        var claimLogger = new FakeLogger<ClaimLoggingMiddleware>();
        var requestOptions = new RequestLoggingOptions()
            .IncludeClaim("partner_id", "PartnerId", "enduser.partner_id");

        var requestLogging = new RequestLoggingMiddleware(
            next: async ctx =>
            {
                var claimLogging = new ClaimLoggingMiddleware(
                    next: _ => Task.CompletedTask,
                    logger: claimLogger,
                    options: Options.Create(requestOptions));

                await claimLogging.InvokeAsync(ctx);
            },
            logger: logger,
            options: Options.Create(requestOptions)
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice"),
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim("partner_id", "partner-456")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await requestLogging.InvokeAsync(context);

        // Assert
        logger.LastScopeValues.Should().ContainKey("PartnerId").WhoseValue.Should().Be("partner-456");
    }

    #endregion

    #region Claim Logging Tests

    [Test]
    public async Task ClaimLogging_InvokeAsync_WhenUserIsAuthenticated_CreatesUserScopeForDownstreamLogs()
    {
        // Arrange
        var logger = new FakeLogger<ClaimLoggingMiddleware>();
        var middleware = new ClaimLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: Options.Create(new RequestLoggingOptions())
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice"),
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastUserName.Should().Be("alice");
        logger.LastUserId.Should().Be("user-123");
    }

    [Test]
    public async Task ClaimLogging_InvokeAsync_WithAdditionalClaims_IncludesClaimsInScope()
    {
        // Arrange
        var logger = new FakeLogger<ClaimLoggingMiddleware>();
        var options = Options.Create(new RequestLoggingOptions()
            .IncludeClaim("partner_id", "PartnerId", "enduser.partner_id")
            .IncludeClaim("location_id", "LocationId", "enduser.location_id"));

        var middleware = new ClaimLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice"),
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim("partner_id", "partner-456"),
            new Claim("location_id", "location-789")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastScopeValues.Should().ContainKey("PartnerId").WhoseValue.Should().Be("partner-456");
        logger.LastScopeValues.Should().ContainKey("LocationId").WhoseValue.Should().Be("location-789");
    }

    [Test]
    public async Task ClaimLogging_InvokeAsync_WhenAdditionalClaimIsMissing_OmitsClaimFromScope()
    {
        // Arrange
        var logger = new FakeLogger<ClaimLoggingMiddleware>();
        var options = Options.Create(new RequestLoggingOptions()
            .IncludeClaim("partner_id", "PartnerId", "enduser.partner_id"));

        var middleware = new ClaimLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice"),
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastScopeValues.Should().NotContainKey("PartnerId");
    }

    [Test]
    public async Task ClaimLogging_InvokeAsync_WithAdditionalClaims_SetsActivityTags()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();

        var logger = new FakeLogger<ClaimLoggingMiddleware>();
        var options = Options.Create(new RequestLoggingOptions()
            .IncludeClaim("partner_id", "PartnerId", "enduser.partner_id"));

        var middleware = new ClaimLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice"),
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim("partner_id", "partner-456")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        activity.GetTagItem("enduser.name").Should().Be("alice");
        activity.GetTagItem("enduser.id").Should().Be("user-123");
        activity.GetTagItem("enduser.partner_id").Should().Be("partner-456");
    }

    [Test]
    public async Task ClaimLogging_InvokeAsync_WithOrderedClaimTypes_UsesFirstNonEmptyValue()
    {
        // Arrange
        var logger = new FakeLogger<ClaimLoggingMiddleware>();
        var options = Options.Create(new RequestLoggingOptions()
            .IncludeClaim(["missing_partner_id", "partner_id"], "PartnerId", "enduser.partner_id"));

        var middleware = new ClaimLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: options
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice"),
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim("partner_id", "partner-456")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastScopeValues.Should().ContainKey("PartnerId").WhoseValue.Should().Be("partner-456");
    }

    [Test]
    public async Task ClaimLogging_InvokeAsync_WhenIdentityNameIsSet_UsesIdentityNameForUserName()
    {
        // Arrange
        var logger = new FakeLogger<ClaimLoggingMiddleware>();
        var middleware = new ClaimLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: Options.Create(new RequestLoggingOptions())
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "identity-name"),
            new Claim(ClaimTypes.Email, "email@example.com"),
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastUserName.Should().Be("identity-name");
    }

    [Test]
    public async Task ClaimLogging_InvokeAsync_WhenUserIdClaimIsMissing_UsesResolvedUserName()
    {
        // Arrange
        var logger = new FakeLogger<ClaimLoggingMiddleware>();
        var middleware = new ClaimLoggingMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger,
            options: Options.Create(new RequestLoggingOptions())
        );

        var context = CreateHttpContext("GET", "/api/test");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "alice")
        ],
        authenticationType: "Test");
        context.User = new ClaimsPrincipal(identity);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.LastUserId.Should().Be("alice");
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
        public string? LastUserName { get; private set; }
        public string? LastUserId { get; private set; }
        public Dictionary<string, string?> LastScopeValues { get; private set; } = new(StringComparer.Ordinal);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            if (state is IEnumerable<KeyValuePair<string, object?>> values)
            {
                var scopeValues = values.ToDictionary(x => x.Key, x => x.Value?.ToString(), StringComparer.Ordinal);
                LastScopeValues = scopeValues;
                scopeValues.TryGetValue("UserName", out var userName);
                scopeValues.TryGetValue("UserId", out var userId);
                LastUserName = userName;
                LastUserId = userId;
            }

            return NullScope.Instance;
        }

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

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
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
