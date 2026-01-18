using Otomasyon.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Otomasyon.API.ExtensionMethods;

public static class WebApplicationExtension
{
    public static async Task ApplyMigrationAsync(this WebApplication app)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        await retryPolicy.ExecuteAsync(async () =>
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            await Task.CompletedTask;
        });
    }
}