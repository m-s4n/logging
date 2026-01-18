using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Otomasyon.DataAccess.Contexts;
using Otomasyon.Shared.Services;
using Otomasyon.API.Services;
using Otomasyon.API.ExtensionMethods;
using Otomasyon.Service.Services.PasswordHasher;
using Otomasyon.Service.Services.AuthUser;
using Otomasyon.API.Logs;
using Otomasyon.API.Logs.Audit;
using Otomasyon.DataAccess.Logs;
using Otomasyon.Shared.Options;
using Otomasyon.Domain.Entities;
using Otomasyon.Service.Services.Security;
using Otomasyon.Services.Services.Auth;
using Otomasyon.Shared.Security;
using Microsoft.IdentityModel.Tokens;
using Otomasyon.Service.Services.Auth;
using Otomasyon.Service.Services.AuthRole;


var builder = WebApplication.CreateBuilder(args);
// ioc
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserServices>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthUserService, AuthUserService>();
builder.Services.AddScoped<IAuthRoleService, AuthRoleService>();
// logging
builder.Services.AddHostedService<RequestSpoolWorker>();
builder.Services.AddHostedService<ErrorSpoolWorker>();
builder.Services.AddHostedService<ActiveSegmentRotatorWorker>();
builder.Services.Configure<LogSpoolOptions>(builder.Configuration.GetSection("LogSpool"));
builder.Services.AddSingleton<ILogSpoolWriter, LogSpoolWriter>();

// Interceptor singleton + scoped userId getter (Guid?)
builder.Services.AddSingleton<AuditOutboxSaveChangesInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
    options.AddInterceptors(sp.GetRequiredService<AuditOutboxSaveChangesInterceptor>());
});

// hangi entity'ler auditlenecek
AuditEntityRegistry.Register<AuthUser>();
AuditEntityRegistry.Register<AuthRole>();
AuditEntityRegistry.Register<AuthUserRole>();



// pipeline
var app = builder.Build();

await app.ApplyMigrationAsync();
app.UseRequestDbLogging();
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();