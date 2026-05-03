using System.Net;

namespace Cars.ApiCommon.Exceptions;

public class InternalServerErrorException(
    string statusCode = "InternalServerError",
    int httpStatusCode = (int)HttpStatusCode.InternalServerError,
    string? message = null,
    Exception? innerException = null)
    : ApplicationException(statusCode, httpStatusCode, message, innerException)
{
}
