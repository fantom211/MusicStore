using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicStore.Web.Data;
using MusicStore.Web.Helpers;
using MusicStore.Web.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StoreConnection")));

builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<SpinService>();

builder.Services.AddHttpClient<IdentityApiClient>(client =>
{
    var baseUrl = builder.Configuration["IdentityService:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("Не задан адрес IdentityService (IdentityService:BaseUrl).");
    }

    client.BaseAddress = new Uri(baseUrl);
});

// Основной сайт проверяет JWT, выпущенный IdentityService. Токен приходит в заголовке
// Authorization либо берётся из HttpOnly cookie, которую сайт сохраняет после входа.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            IssuerSigningKey = JwtHelper.GetSigningKey(builder.Configuration),
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token ??= context.Request.Cookies[AuthCookie.Name];
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Неаутентифицированный пользователь на обычной странице — на форму входа;
                // API продолжает отвечать 401.
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Account/Login");
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
    options.FallbackPolicy = policy;
});

builder.Services.AddHttpContextAccessor();

// MVC-страницы и API проверяют один и тот же JWT.
builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<ApiExceptionFilter>();
    })
    // Enum-значения в JSON передаются строками: "Guitars", "New" и т.д.
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();
