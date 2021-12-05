using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApi_Serilog
{
    public class ApiLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Serilog.ILogger _logger;

        public ApiLoggerMiddleware(RequestDelegate next, Serilog.ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await LogRequest(httpContext);

            await LogResponse(httpContext);
        }

        private async Task LogResponse(HttpContext httpContext)
        {
            var originalStream = httpContext.Response.Body;
            await using (var memoryStream = new MemoryStream())
            {
                httpContext.Response.Body = memoryStream;

                await _next(httpContext);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseResult = await new StreamReader(memoryStream).ReadToEndAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalStream);
                httpContext.Response.Body = originalStream;
                Log(httpContext, $"StatusCode: {httpContext.Response.StatusCode} Response Result: {responseResult}");
            }
        }

        private async Task LogRequest(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            var streamReader = new StreamReader(httpContext.Request.Body);
            var requestBody = await streamReader.ReadToEndAsync();
            httpContext.Request.Body.Position = 0L;
            Log(httpContext, $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString.ToUriComponent()} RequestBody: {requestBody}");
        }

        private void Log(HttpContext httpContext, string? message)
        {
            _logger.Information($"[{httpContext.TraceIdentifier}]{message}");
        }
    }
}