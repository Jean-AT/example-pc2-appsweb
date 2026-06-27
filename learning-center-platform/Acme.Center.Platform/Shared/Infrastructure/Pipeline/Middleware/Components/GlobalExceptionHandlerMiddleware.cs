using System.Net.Mime;
using System.Text.Json;
using Acme.Center.Platform.Shared.Resources;
using Acme.Center.Platform.Shared.Resources.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Acme.Center.Platform.Shared.Infrastructure.Pipeline.Middleware;

public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IStringLocalizer<ErrorMessage> errorLocalizer, // inject IStringLocalizer for error messages
    IStringLocalizer<CommonMessages> // Corrected to Commons 
        commonLocalizer) // Inject IStringLocalizer for common messages "Internal Server Error"
{
    private readonly IStringLocalizer<CommonMessages> _commonLocalizer = commonLocalizer;
    private readonly IStringLocalizer<ErrorMessage> _errorLocalizer = errorLocalizer;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex,  "Request was cancelled: {Message}", ex.Message);
            await HandleOperationCanceledExceptionAsync(httpContext, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleGenericExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleOperationCanceledExceptionAsync(HttpContext httpContext, OperationCanceledException ex)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict; // Or 204 No Content if appropriate

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = _errorLocalizer["OperationCancelled"],
            Detail = _errorLocalizer["OperationCancelled"],
            Instance = httpContext.Request.Path
        };
        
        var jsonOptions =  new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await httpContext.Response.WriteAsync(result);
    }
    
    
    private async Task HandleGenericExceptionAsync(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = _errorLocalizer["InternalServerError"],
            Detail = _errorLocalizer["GenericError"],
            Instance = httpContext.Request.Path
        };
        
        var jsonOptions =  new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await httpContext.Response.WriteAsync(result);
    }
}