using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace RecipeBook.Api.Middlewares
{
    public class ResponseTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await _next(httpContext);
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"[API] {httpContext.Request.Method}{httpContext.Request.Path} took {elapsedMs}");
        }
    }
}