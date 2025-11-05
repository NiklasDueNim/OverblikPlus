using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OverblikPlus.Shared.Common;
using OverblikPlus.Shared.Interfaces;

namespace TaskMicroService.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerService _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next, 
            ILoggerService logger,
            IHostEnvironment environment)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError($"Unhandled exception occurred: {exception.Message}", exception);

            var statusCode = GetStatusCode(exception);
            var errorMessage = GetErrorMessage(exception);

            var response = Result<object>.ErrorResult(errorMessage);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        private HttpStatusCode GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                DbUpdateException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private string GetErrorMessage(Exception exception)
        {
            if (_environment.IsDevelopment())
            {
                return $"An unexpected error occurred: {exception.Message}\n" +
                       $"Stack Trace: {exception.StackTrace}\n" +
                       $"Inner Exception: {exception.InnerException?.Message}";
            }

            return exception switch
            {
                ArgumentNullException argEx => $"Invalid request: {argEx.ParamName} is required.",
                ArgumentException argEx => $"Invalid request: {argEx.Message}",
                KeyNotFoundException => "The requested resource was not found.",
                UnauthorizedAccessException => "You are not authorized to perform this action.",
                DbUpdateException => "An error occurred while saving data. Please check your input.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }
}
