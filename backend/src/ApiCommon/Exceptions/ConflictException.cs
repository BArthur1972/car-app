using System.Net;

namespace Cars.ApiCommon.Exceptions;

public class ConflictException(
    string statusCode = "Conflict",
    int httpStatusCode = (int)HttpStatusCode.Conflict,
    string? message = null,
    Exception? innerException = null)
    : ApplicationException(statusCode, httpStatusCode, message, innerException)
{
}
