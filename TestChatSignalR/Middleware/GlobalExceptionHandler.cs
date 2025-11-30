using Microsoft.AspNetCore.Diagnostics;

namespace WebApiDraft.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred during request processing");

            httpContext.Response.ContentType = "application/json";

            object responseObj;

            switch (exception)
            {
                case UnauthorizedAccessException:
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    responseObj = new { error = "Unauthorized access" };
                    break;
                case ArgumentException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    responseObj = new { error = exception.Message };
                    break;
                case InvalidOperationException:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    responseObj = new { error = exception.Message };
                    break;
                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    responseObj = new { error = "An unexpected error occurred" };
                    break;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(responseObj);
            await httpContext.Response.WriteAsync(json, cancellationToken);

            return true;
        }
    }
}
