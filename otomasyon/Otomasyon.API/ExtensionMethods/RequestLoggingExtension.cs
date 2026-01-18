using Otomasyon.API.Logs;
using Otomasyon.Domain.Entities;
using Otomasyon.Shared.Services;

namespace Otomasyon.API.ExtensionMethods;

public static class RequestLoggingExtension
{
    public static IApplicationBuilder UseRequestDbLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            await next(); // Exception olursa UseExceptionHandler handle edip response döner ve buraya geri gelir

            sw.Stop();

            var currentUser = context.RequestServices.GetService<ICurrentUserService>();
            var spool = context.RequestServices.GetRequiredService<ILogSpoolWriter>();

            var errorCode = context.Items.TryGetValue(ExceptionItems.ErrorCode, out var ec) ? ec?.ToString() : null;
            var errorMsg = context.Items.TryGetValue(ExceptionItems.ErrorMessage, out var em) ? em?.ToString() : null;
            var level = context.Items.TryGetValue(ExceptionItems.Level, out var lv) ? lv?.ToString() : "Information";

            // İstersen: StatusCode >= 500 ise otomatik Error seviyesine çek
            if (context.Response.StatusCode >= 500) level = "Error";

            spool.Write(new RequestLog
            {
                CreatedAt = DateTimeOffset.UtcNow,
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                StatusCode = context.Response.StatusCode,
                ErrorCode = errorCode,
                ErrorMessage = errorMsg,
                Duration = sw.ElapsedMilliseconds,
                Level = level ?? "Information",
                TraceId = context.TraceIdentifier,
                UserId = currentUser?.UserId,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                IsDispatched = false,
                DispatchedAt = null
            });
        });

        return app;
    }
}
