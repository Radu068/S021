using System.Globalization;
using System.Threading.RateLimiting;
using HotelManager.Data;
using HotelManager.Models;
using HotelManager.Repositories;
using HotelManager.Repositories.Interfaces;
using HotelManager.Security;
using HotelManager.Services;
using HotelManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");

    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Rooms/Index");

    options.Conventions.AuthorizeFolder(
        "/Reception",
        "ReceptionistOnly");

    options.Conventions.AuthorizeFolder(
        "/Guests",
        "ReceptionistOnly");

    options.Conventions.AuthorizeFolder(
        "/Rooms",
        "ReceptionistOnly");
});

var connection = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Lipsește conexiunea SQLite."));

if (!Path.IsPathRooted(connection.DataSource))
{
    connection.DataSource = Path.Combine(
        builder.Environment.ContentRootPath,
        connection.DataSource);
}

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(connection.ConnectionString));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<SqliteWriteScope>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<
    IReservationRepository,
    ReservationRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<
    IReservationService,
    ReservationService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddScoped<AuthCookieEvents>();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";

        options.Cookie.Name = "__Host-HotelManager.Auth";
        options.Cookie.Path = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = false;
        options.EventsType = typeof(AuthCookieEvents);
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy(
        "ReceptionistOnly",
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(nameof(Receptionist))));

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__Host-HotelManager.Antiforgery";
    options.Cookie.Path = "/";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        !HttpMethods.IsPost(context.Request.Method)
            ? RateLimitPartition.GetNoLimiter("auth-get")
            : RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.OnRejected = async (context, token) =>
    {
        var response = context.HttpContext.Response;

        response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        response.ContentType = "text/plain; charset=utf-8";

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out TimeSpan retry))
        {
            response.Headers["Retry-After"] =
                Math.Ceiling(retry.TotalSeconds)
                    .ToString(CultureInfo.InvariantCulture);
        }

        await response.WriteAsync(
            "Prea multe încercări. Reîncearcă peste un minut.",
            token);
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

if (app.Environment.IsDevelopment())
{
    await using var scope =
        app.Services.CreateAsyncScope();

    await DbInitializer.SeedAsync(
        scope.ServiceProvider,
        app.Configuration);
}

app.Run();