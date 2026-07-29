using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JewelryHub.Application.Common.Behaviors;

/// <summary>Logs every request's name, elapsed time, and (on failure) the exception — one place instead of try/catch/log boilerplate in every handler.</summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Long running request: {RequestName} ({ElapsedMs} ms)", requestName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation("Handled {RequestName} in {ElapsedMs} ms", requestName, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request {RequestName} failed", requestName);
            throw;
        }
    }
}
