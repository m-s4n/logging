using Microsoft.AspNetCore.Diagnostics;
using Otomasyon.API.Common.Responses;
using Otomasyon.API.Logs;
using Otomasyon.Domain.Entities;
using Otomasyon.Domain.Exceptions;
using Otomasyon.Shared.Services;

namespace Otomasyon.API.ExtensionMethods;

public static class GlobalExceptionLoggingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async context =>
            {
                var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                var currentUser = context.RequestServices.GetService<ICurrentUserService>();
                var spool = context.RequestServices.GetRequiredService<ILogSpoolWriter>();

                // Business -> 4xx
                if (ex is BusinessException bex)
                {
                    context.Items[ExceptionItems.ErrorCode] = bex.ErrorCode;
                    context.Items[ExceptionItems.ErrorMessage] = bex.Message;
                    context.Items[ExceptionItems.Level] = "Warning";
                    // kullanıcıya domain(business) hatası cevabı
                    context.Response.StatusCode = bex.StatusCode;
                    await context.Response.WriteAsJsonAsync(ApiResponses.Fail<object>(
                        message: bex.Message,
                        errorCode: bex.ErrorCode,
                        traceId: context.TraceIdentifier,
                        statusCode: context.Response.StatusCode
                    ));
                    return;
                }

                // Unhandled -> 500
                context.Items[ExceptionItems.ErrorCode] = "UNHANDLED_EXCEPTION";
                context.Items[ExceptionItems.ErrorMessage] = ex?.Message;
                context.Items[ExceptionItems.Level] = "Error";

                // ErrorLog yaz
                spool.Write(new ErrorLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ErrorCode = "UNHANDLED_EXCEPTION",
                    ErrorMessage = ex?.Message,
                    TraceId = context.TraceIdentifier,
                    UserId = currentUser?.UserId,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    ExceptionType = ex?.GetType().FullName ?? "Exception",
                    StackTrace = ex?.StackTrace ?? "",
                    InnerException = ex?.InnerException?.ToString(),
                    IsDispatched = false,
                    DispatchedAt = null
                });

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                
                // kullanıcıya 500 hatası cevabı
                await context.Response.WriteAsJsonAsync(ApiResponses.Fail<object>(
                    message: "Beklenmeyen bir hata oluştu.",
                    errorCode: "UNHANDLED_EXCEPTION",
                    traceId: context.TraceIdentifier,
                    statusCode: StatusCodes.Status500InternalServerError
                ));
            });
        });

        return app;
    }
}
