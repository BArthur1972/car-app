using System.Net;

namespace Cars.ApiCommon.Exceptions;

public class DataNotFoundException(
    string statusCode = "NotFound",
    int httpStatusCode = (int)HttpStatusCode.NotFound,
    string? message = null,
    Exception? innerException = null)
    : ApplicationException(statusCode, httpStatusCode, message, innerException)
{
}
