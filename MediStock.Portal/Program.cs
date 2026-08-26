// ============================================================
//  MediStock.Portal — Program.cs
// ============================================================

using Microsoft.AspNetCore.Authentication.Cookies;
using NLog;
using NLog.Web;

var logger = LogManager.Setup()
    .LoadConfigurationFromFile("NLog.config")
    .GetCurrentClassLogger();

logger.Info("=== MediStock Portal starting ===");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var authApiUrl = builder.Configuration["ApiSettings:AuthApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiSettings:AuthApiBaseUrl is missing.");
    var mainApiUrl = builder.Configuration["ApiSettings:MainApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiSettings:MainApiBaseUrl is missing.");

    builder.Services.AddHttpClient("AuthApi", c =>
    {
        c.BaseAddress = new Uri(authApiUrl);
        c.DefaultRequestHeaders.Add("Accept", "application/json");
        c.Timeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddHttpClient("MainApi", c =>
    {
        c.BaseAddress = new Uri(mainApiUrl);
        c.DefaultRequestHeaders.Add("Accept", "application/json");
        c.Timeout = TimeSpan.FromMinutes(5);
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<MediStock.Portal.Services.ApiClient>();
    builder.Services.AddScoped<MediStock.Portal.Services.MenuService>();
    builder.Services.AddScoped<MediStock.Portal.Services.AuditService>();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
        {
            opt.LoginPath = "/Account/Login";
            opt.LogoutPath = "/Account/Logout";
            opt.AccessDeniedPath = "/Account/AccessDenied";
            opt.ExpireTimeSpan = TimeSpan.FromHours(8);
            opt.SlidingExpiration = true;
            opt.Cookie.HttpOnly = true;
            opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            opt.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = ctx =>
                {
                    var uri = new UriBuilder(ctx.RedirectUri);
                    var qs = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    qs["expired"] = "true";
                    uri.Query = qs.ToString();
                    ctx.Response.Redirect(uri.ToString());
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(opt =>
    {
        opt.IdleTimeout = TimeSpan.FromHours(8);
        opt.Cookie.HttpOnly = true;
        opt.Cookie.IsEssential = true;
    });

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    logger.Info("=== MediStock Portal ready ===");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Portal startup failed");
    throw;
}
finally
{
    LogManager.Shutdown();
}
