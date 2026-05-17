using Microsoft.AspNetCore.Diagnostics;
using ReviewerPortal.API.Application.Exceptions;

namespace ReviewerPortal.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            BadRequestException ex => (StatusCodes.Status400BadRequest, ex.Message),
            NotFoundException ex   => (StatusCodes.Status404NotFound,   ex.Message),
            _                      => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);
        return true;
    }
}
