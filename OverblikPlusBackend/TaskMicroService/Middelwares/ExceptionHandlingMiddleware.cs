using System.Net;
using Newtonsoft.Json;
using OverblikPlus.Shared.Interfaces;
using TaskMicroService.Common;

namespace TaskMicroService.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerService _logger; 

        public ExceptionHandlingMiddleware(RequestDelegate next, ILoggerService logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError("Unhandled exception occurred.", exception);

            var response = Result<object>.ErrorResult("An unexpected error occurred. Please try again later.");
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                
            }
            
            response.Error += $" Detail: {exception.Message}";

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}