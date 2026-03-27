using System.Text;

namespace AgenticDemo.Api.Middleware;

public sealed class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestBody = string.Empty;
        if (context.Request.ContentLength is > 0)
        {
            using var requestReader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await requestReader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        logger.LogInformation("Incoming request: {Method} {Path} body={Body}", context.Request.Method, context.Request.Path, requestBody);

        var originalBody = context.Response.Body;
        await using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("Outgoing response: status={StatusCode} body={Body}", context.Response.StatusCode, responseBody);

        await memoryStream.CopyToAsync(originalBody);
    }
}
