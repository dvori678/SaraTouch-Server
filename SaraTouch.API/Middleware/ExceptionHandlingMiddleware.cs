using System.Text.Json;
using SaraTouch.API.Models;

namespace SaraTouch.API.Middleware;

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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // הלקוח סגר את החיבור באמצע. זה לא באג, אז לא Error
            _logger.LogInformation("Request was cancelled by the client");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. CorrelationId {CorrelationId}", context.TraceIdentifier);

            if (context.Response.HasStarted)
                throw;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            // ללקוח: הודעה כללית בלבד. הפרטים של החריגה נשארים בלוג
            var error = new ErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Message = "אירעה שגיאה בשרת. אפשר לפנות לתמיכה עם מזהה הבקשה",
                CorrelationId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(error,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}