using System.Net;
using System.Text.Json;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Web.Extensions;
using CleanArchitecture.Web.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CleanArchitecture.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private static readonly PathString ErrorRedirectPath = "/";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "An unhandled exception occurred after the response had started");
                throw;
            }

            var statusCode = GetStatusCode(ex);

            if (statusCode == (int)HttpStatusCode.InternalServerError && _environment.IsDevelopment())
            {
                throw;
            }

            await HandleExceptionAsync(context, ex, statusCode);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
    {
        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogInformation(
                "Request rejected ({StatusCode}) on {Method} {Path}: {Message}",
                statusCode,
                context.Request.Method,
                context.Request.Path,
                exception.Message);
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        var message = GetUserMessage(exception, statusCode);

        if (ExpectsJson(context))
        {
            context.Response.ContentType = "application/json";

            var payload = new
            {
                error = new
                {
                    message,
                    details = _environment.IsDevelopment() ? exception.ToString() : null
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            return;
        }

        var factory = context.RequestServices.GetService<ITempDataDictionaryFactory>();
        if (factory != null)
        {
            var tempData = factory.GetTempData(context);

            tempData[ToastMessage.ErrorKey] = new ToastMessage
            {
                Header = "Error",
                Message = message
            }.Serialize();

            tempData.Save();
        }

        if (context.Request.Path.Equals(ErrorRedirectPath, StringComparison.OrdinalIgnoreCase))
        {
            await context.Response.WriteAsync(message);
            return;
        }

        context.Response.Redirect(ErrorRedirectPath);
    }

    private static bool ExpectsJson(HttpContext context)
    {
        if (context.Request.IsAjaxRequest())
        {
            return true;
        }

        var accept = context.Request.Headers.Accept.ToString();

        return !string.IsNullOrEmpty(accept)
            && accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            && !accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        RessourceNotFoundException => (int)HttpStatusCode.NotFound,
        InsufficientRightsException => (int)HttpStatusCode.Forbidden,
        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
        ValidationDomaineException => (int)HttpStatusCode.UnprocessableEntity,
        DuplicateEntityException => (int)HttpStatusCode.Conflict,
        BusinessRuleViolationException => (int)HttpStatusCode.UnprocessableEntity,
        DomainException => (int)HttpStatusCode.UnprocessableEntity,
        OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
        _ => (int)HttpStatusCode.InternalServerError
    };

    private static string GetUserMessage(Exception exception, int statusCode) =>
        statusCode == (int)HttpStatusCode.InternalServerError
            ? "An error occurred while processing your request."
            : exception.Message;
}
