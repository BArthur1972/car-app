using System.Net;
using Cars.ApiCommon.Errors;
using ApplicationException = Cars.ApiCommon.Exceptions.ApplicationException;

namespace Cars.ApiCommon.Middlewares
{
    /// <summary>
    /// Middleware to handle exceptions in the ASP.NET Core pipeline.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </remarks>
    /// <param name="next">The next middleware in the pipeline.</param>
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        /// <summary>
        /// The next middleware in the pipeline.
        /// </summary>
        private readonly RequestDelegate _next = next;

        /// <summary>
        /// Invokes the middleware to handle exceptions in the request pipeline.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handles all exceptions thrown in the request pipeline and
        /// returns a JSON response with the error details.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <param name="exception">The exception that was thrown.</param>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ErrorResponse errorResponse;
            HttpResponse response = context.Response;
            
            // All exceptions should either inherit from ApplicationException.
            if (exception is ApplicationException appEx)
            {
                response.StatusCode = appEx.HttpStatusCode;
                errorResponse = new ErrorResponse(
                    new ErrorDetail(
                    appEx.StatusCode,
                    appEx.Message
                ));
            }
            else // For any other unhandled exception, we return 500 Internal Server Error.
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new ErrorResponse(
                    new ErrorDetail(
                    "InternalServerError",
                    "An internal server error has occurred."
                ));
            }

            response.Headers.TryAdd("x-ms-error-code", errorResponse.Error.Code);
            response.ContentType = "application/json";
            await response.WriteAsJsonAsync(errorResponse).ConfigureAwait(false);
        }
    }
}
