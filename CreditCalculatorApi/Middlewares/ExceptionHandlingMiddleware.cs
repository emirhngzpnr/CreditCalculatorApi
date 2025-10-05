using CreditCalculatorApi.Exceptions;
using System.Net;
using System.Text.Json;

namespace CreditCalculatorApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // sıradaki middleware/controller'a geç
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstisna yakalandı: {Message}", ex.Message);

                context.Response.ContentType = "application/json";

                var (statusCode, code, message) = ex switch
                {
                    BadRequestException => (HttpStatusCode.BadRequest, "BAD_REQUEST", ex.Message),
                    DuplicateEmailException => (HttpStatusCode.BadRequest, "DUPLICATE_EMAIL", ex.Message),
                    KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", ex.Message),
                    ArgumentNullException => (HttpStatusCode.BadRequest, "ARGUMENT_NULL", ex.Message),
                    ApplicationException => (HttpStatusCode.BadRequest, "APPLICATION_ERROR", ex.Message),
                    _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "Sunucu tarafında beklenmeyen bir hata oluştu.")
                };

                context.Response.StatusCode = (int)statusCode;

                var response = new
                {
                    code,
                    message
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
