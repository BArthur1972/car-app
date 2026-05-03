using System.Net;

namespace Cars.ApiCommon.Exceptions;

public class BadRequestException(
    string statusCode = "BadRequest",
    int httpStatusCode = (int)HttpStatusCode.BadRequest,
    string? message = null,
    Exception? innerException = null)
    : ApplicationException(statusCode, httpStatusCode, message, innerException)
{
}
