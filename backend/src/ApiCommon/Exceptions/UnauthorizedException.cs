using System.Net;

namespace Cars.ApiCommon.Exceptions;

public class UnauthorizedException(
    string statusCode = "Unauthorized",
    int httpStatusCode = (int)HttpStatusCode.Unauthorized,
    string? message = null,
    Exception? innerException = null)
    : ApplicationException(statusCode, httpStatusCode, message, innerException)
{
}
