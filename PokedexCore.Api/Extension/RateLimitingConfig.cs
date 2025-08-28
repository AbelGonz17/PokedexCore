using System.Threading.RateLimiting;

public static class RateLimitingConfig
{
    public static void AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Rate limiting dinámico (según si está autenticado o no)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userId = httpContext.User.Identity?.Name
                                 ?? httpContext.User.FindFirst("sub")?.Value
                                 ?? "authenticated";

                    return RateLimitPartition.GetFixedWindowLimiter(userId, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20, // 20 req/min para autenticados
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                }
                else
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5, // 5 req/min para invitados
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                }
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }
}


