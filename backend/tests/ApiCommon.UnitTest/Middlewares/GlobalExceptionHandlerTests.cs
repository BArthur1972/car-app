using Cars.ApiCommon.Exceptions;
using Cars.ApiCommon.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace ApiCommon.UnitTest.Middlewares;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler sut =
        new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task TryHandleAsync_MapsApplicationExceptionStatusCode()
    {
        var context = CreateContext();
        await sut.TryHandleAsync(context, new DataNotFoundException(message: "Not found"), default);
        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_Returns500_WhenUnhandledExceptionThrown()
    {
        var context = CreateContext();
        await sut.TryHandleAsync(context, new Exception("Unexpected"), default);
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_SetsXMsErrorCodeHeader_OnApplicationException()
    {
        var context = CreateContext();
        await sut.TryHandleAsync(context, new DataNotFoundException(message: "Not found"), default);
        context.Response.Headers.ContainsKey("x-ms-error-code").Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_SetsXMsErrorCodeHeader_OnUnhandledException()
    {
        var context = CreateContext();
        await sut.TryHandleAsync(context, new Exception("Unexpected"), default);
        context.Response.Headers.ContainsKey("x-ms-error-code").Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_SetsJsonContentType()
    {
        var context = CreateContext();
        await sut.TryHandleAsync(context, new BadRequestException(message: "Bad"), default);
        context.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        var context = CreateContext();
        var handled = await sut.TryHandleAsync(context, new Exception("Any"), default);
        handled.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_WritesBody_OnApplicationException()
    {
        var context = CreateContext();
        await sut.TryHandleAsync(context, new DataNotFoundException(message: "Not found"), default);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_WritesBody_OnUnhandledException()
    {
        var context = CreateContext();
        await sut.TryHandleAsync(context, new Exception("Unexpected"), default);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().NotBeNullOrEmpty();
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
