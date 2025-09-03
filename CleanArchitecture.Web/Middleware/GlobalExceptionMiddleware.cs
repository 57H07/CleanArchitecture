using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CleanArchitecture.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = new
            {
                message = GetErrorMessage(exception),
                details = exception.Message
            }
        };

        context.Response.StatusCode = GetStatusCode(exception);

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        EntityNotFoundException => (int)HttpStatusCode.NotFound,
        DuplicateEntityException => (int)HttpStatusCode.Conflict,
        BusinessRuleViolationException => (int)HttpStatusCode.BadRequest,
        DomainException => (int)HttpStatusCode.BadRequest,
        _ => (int)HttpStatusCode.InternalServerError
    };

    private static string GetErrorMessage(Exception exception) => exception switch
    {
        EntityNotFoundException => "The requested resource was not found.",
        DuplicateEntityException => "A resource with the same identifier already exists.",
        BusinessRuleViolationException => "Business rule violation.",
        DomainException => "Domain rule violation.",
        _ => "An error occurred while processing your request."
    };
}
