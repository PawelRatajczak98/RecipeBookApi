using Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace RecipeBook.Api.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";

                ApiException response;
                int statusCode;

                if (ex is ApiException apiEx)
                {
                    statusCode = apiEx.StatusCode;
                    response = env.IsDevelopment()
                        ? new ApiException(statusCode, apiEx.Message, apiEx.Details ?? ex.StackTrace)
                        : new ApiException(statusCode, apiEx.Message);
                }
                else
                {
                    statusCode = (int)HttpStatusCode.InternalServerError;

                    response = env.IsDevelopment()
                        ? new ApiException(statusCode, ex.Message, ex.StackTrace)
                        : new ApiException(statusCode, "An unexpected error occurred.");
                }

                context.Response.StatusCode = statusCode;
                

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(response, options);
                await context.Response.WriteAsync(json);
            }
        }
    }
}