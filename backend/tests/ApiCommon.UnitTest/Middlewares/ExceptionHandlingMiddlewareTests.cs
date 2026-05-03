using Cars.ApiCommon.Exceptions;
using Cars.ApiCommon.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace ApiCommon.UnitTest.Middlewares;

public class ExceptionHandlingMiddlewareTests
{
    private static ExceptionHandlingMiddleware MiddlewareThrowing(Exception ex)
        => new(_ => throw ex);

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_Returns404_WhenDataNotFoundExceptionThrown()
    {
        var context = CreateContext();
        await MiddlewareThrowing(new DataNotFoundException(message: "Not found"))
            .InvokeAsync(context);

        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task InvokeAsync_Returns400_WhenBadRequestExceptionThrown()
    {
        var context = CreateContext();
        await MiddlewareThrowing(new BadRequestException(message: "Bad request"))
            .InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_Returns500_WhenUnhandledExceptionThrown()
    {
        var context = CreateContext();
        await MiddlewareThrowing(new Exception("Unexpected error"))
            .InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_SetsXMsErrorCodeHeader_OnApplicationException()
    {
        var context = CreateContext();
        await MiddlewareThrowing(new DataNotFoundException(message: "Not found"))
            .InvokeAsync(context);

        context.Response.Headers.ContainsKey("x-ms-error-code").Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_DoesNotIntercept_WhenNoExceptionThrown()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }
}
