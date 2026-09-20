using System.Net;
using System.Text.Json;
using JobPlatform.API.Common;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Domain.Exceptions;

namespace JobPlatform.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var validationResponse = ApiResponse<object>.FailureResponse("Validation failed.", validationException.Errors);
                await context.Response.WriteAsync(JsonSerializer.Serialize(validationResponse));
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                var notFoundResponse = ApiResponse<object>.FailureResponse(notFoundException.Message);
                await context.Response.WriteAsync(JsonSerializer.Serialize(notFoundResponse));
                break;

            case ForbiddenException forbiddenException:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                var forbiddenResponse = ApiResponse<object>.FailureResponse(forbiddenException.Message);
                await context.Response.WriteAsync(JsonSerializer.Serialize(forbiddenResponse));
                break;

            case ConflictException conflictException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                var conflictResponse = ApiResponse<object>.FailureResponse(conflictException.Message);
                await context.Response.WriteAsync(JsonSerializer.Serialize(conflictResponse));
                break;

            case DomainException domainException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                var domainResponse = ApiResponse<object>.FailureResponse(domainException.Message);
                await context.Response.WriteAsync(JsonSerializer.Serialize(domainResponse));
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                var unauthorizedResponse = ApiResponse<object>.FailureResponse("Unauthorized access.");
                await context.Response.WriteAsync(JsonSerializer.Serialize(unauthorizedResponse));
                break;

            default:
                _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var genericResponse = ApiResponse<object>.FailureResponse("An unexpected error occurred. Please try again later.");
                await context.Response.WriteAsync(JsonSerializer.Serialize(genericResponse));
                break;
        }
    }
}
