﻿using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IO;
using Serilog;

namespace WebApi_Serilog
{
    public class ApiLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Serilog.ILogger _logger;
        private RecyclableMemoryStreamManager? _recyclableMemoryStreamManager;

        public ApiLoggerMiddleware(RequestDelegate next, Serilog.ILogger logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            var apiLoggerAttribute = httpContext.GetEndpoint()?.Metadata.GetMetadata<ApiLoggerAttribute>();
            if (apiLoggerAttribute != null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"--- REQUEST {httpContext.TraceIdentifier}: BEGIN ---");
                stringBuilder.AppendLine(
                    $"{httpContext.Request.Method} {httpContext.Request.Path}{httpContext.Request.QueryString.ToUriComponent()} {httpContext.Request.Protocol}");
                var streamReader = new StreamReader(httpContext.Request.Body);
                var result = await streamReader.ReadToEndAsync();
                stringBuilder.AppendLine($"Request Content: {result}");
                httpContext.Request.Body.Position = 0L;
                foreach (var header in httpContext.Request.Headers)
                {
                    stringBuilder.AppendLine($"{header.Key}: {header.Value}");
                }

                stringBuilder.AppendLine();
                stringBuilder.AppendLine(httpContext.Request.Body.ToString());
                stringBuilder.AppendLine($"--- REQUEST {httpContext.TraceIdentifier}: END ---");

                _logger.Information(stringBuilder.ToString());
            }
            var originalStream = httpContext.Response.Body;
            var memoryStream = new MemoryStream();
            httpContext.Response.Body = memoryStream;
            
            await _next(httpContext);
            
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseResult = await new StreamReader(memoryStream).ReadToEndAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalStream);
            httpContext.Response.Body = originalStream;
            _logger.Information($"Response Result: {responseResult}");
            
        }
    }
}