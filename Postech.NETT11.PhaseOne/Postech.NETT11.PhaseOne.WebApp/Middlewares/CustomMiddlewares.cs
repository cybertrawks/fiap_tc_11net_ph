namespace Postech.NETT11.PhaseOne.WebApp.Middlewares;

public static class CustomMiddlewares
{
    // NOVO - Adicionar CorrelationId
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    // Manter método existente
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path}");
            await next.Invoke();
            Console.WriteLine($"Outgoing response: {context.Response.StatusCode}");
        });
    }
    
    // Atualizar para usar logger do Serilog
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                await next.Invoke();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var correlationId = context.Items["CorrelationId"]?.ToString() ?? "Unknown";

                logger.LogError(ex,
                    "Unhandled exception. CorrelationId: {CorrelationId}",
                    correlationId);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "An unexpected error occurred",
                    correlationId,
                    timestamp = DateTime.UtcNow
                });
            }
        });
    }
}