using System.Net;
using Cars.ApiCommon.Errors;
using Microsoft.AspNetCore.Diagnostics;
using ApplicationException = Cars.ApiCommon.Exceptions.ApplicationException;

namespace Cars.ApiCommon.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            ApplicationException appEx => (appEx.HttpStatusCode, appEx.StatusCode, appEx.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "InternalServerError", 
                "An unexpected error occurred. Please try again later.")
        };

        var errorResponse = new ErrorResponse(new ErrorDetail(errorCode, message));

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.Headers.TryAdd("x-ms-error-code", errorResponse.Error.Code);

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        
        if (statusCode >= 500)
        {
            logger.LogError(exception, 
                "Server error in {RequestMethod} {RequestPath}: {StatusCode} {ErrorCode} - {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                statusCode,
                errorCode,
                exception.Message);
        }
        else
        {
            logger.LogInformation(
                "Client error in {RequestMethod} {RequestPath}: {StatusCode} {ErrorCode}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                statusCode,
                errorCode);
        }

        return true;
    }
}
