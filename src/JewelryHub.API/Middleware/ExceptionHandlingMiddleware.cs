using System.Net;
using System.Text.Json;
using JewelryHub.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = FluentValidation.ValidationException;

namespace JewelryHub.API.Middleware;

/// <summary>
/// Single place every exception is translated into an HTTP response, so
/// controllers stay free of try/catch and never leak stack traces to
/// clients. Keeps a consistent RFC 7807 ProblemDetails shape across the
/// whole API regardless of which layer threw.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException vex => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                vex.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            BusinessRuleException bre => (
                HttpStatusCode.BadRequest,
                bre.Message,
                bre.Field is not null
                    ? new Dictionary<string, string[]> { [bre.Field] = new[] { bre.Message } }
                    : null),

            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, null),
            AuthenticationFailedException afe => (HttpStatusCode.Unauthorized, afe.Message, null),
            ForbiddenAccessException fae => (HttpStatusCode.Forbidden, fae.Message, null),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Instance = context.Request.Path,
        };

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        // Stack traces only in Development — never leak internals in production.
        if (_environment.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            problemDetails.Extensions["exception"] = exception.ToString();
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
