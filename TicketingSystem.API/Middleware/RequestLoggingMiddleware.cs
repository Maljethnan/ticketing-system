namespace TicketingSystem.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Request: {Method} {Path} from {Ip}",
            context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);
        await _next(context);
        _logger.LogInformation("Response: {Method} {Path} -> {StatusCode}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
}
