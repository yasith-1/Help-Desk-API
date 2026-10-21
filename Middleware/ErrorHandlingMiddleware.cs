using System.Net;
using System.Text.Json;

namespace ProductApi.Middleware;

// Global error handler (Spring: @ControllerAdvice වගේ — errors එක තැනකින්, ලස්සන JSON).
// Service වලින් එන exception වර්ග අදාළ HTTP status code එකට map කරනවා.
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);                      // ඊළඟ layer එකට (Controller දක්වා)
        }
        catch (Exception ex)
        {
            // exception වර්ගය අනුව status code එක තෝරනවා
            var (status, message) = ex switch
            {
                KeyNotFoundException       => (HttpStatusCode.NotFound, ex.Message),        // 404
                UnauthorizedAccessException => (HttpStatusCode.Forbidden, ex.Message),      // 403
                ArgumentException          => (HttpStatusCode.BadRequest, ex.Message),      // 400
                InvalidOperationException  => (HttpStatusCode.BadRequest, ex.Message),      // 400
                _                          => (HttpStatusCode.InternalServerError, "අභ්‍යන්තර දෝෂයක්")
            };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var response = new { status = (int)status, message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
