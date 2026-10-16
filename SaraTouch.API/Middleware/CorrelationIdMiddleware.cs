namespace SaraTouch.API.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // אם הלקוח שלח מזהה, ממשיכים איתו. אחרת יוצרים חדש
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value) &&
                            !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : Guid.NewGuid().ToString("N");

        // TraceIdentifier: מכאן כל השרת, כולל ErrorResponse, משתמש במזהה הזה
        context.TraceIdentifier = correlationId;

        // המזהה חוזר גם בכותרת של התשובה
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // scope: כל שורת לוג שנכתבת בזמן הבקשה, בכל שכבה, נושאת את המזהה
        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}