namespace Cars.ApiCommon.Exceptions;

public class ApplicationException(
    string statusCode,
    int httpStatusCode,
    string? message = null,
    Exception? innerException = null) 
    : Exception(message, innerException)
{
    public string StatusCode { get; set; } = statusCode;
    public int HttpStatusCode { get; set; } = httpStatusCode;
}
