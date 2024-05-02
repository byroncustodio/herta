using Herta.Auth;
using Herta.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MvposSDK;

namespace Herta;

public static class ServicesExtensions
{
    public static void AddAppAuthentication(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddCascadingAuthenticationState();
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Audience = configuration["jwtProvider:audience"];
            options.TokenValidationParameters.ValidIssuer = configuration["jwtProvider:validIssuer"];
        });
        
        services.AddAuthorization();
        
        services.AddScoped<AuthManager>();
        
        services.AddHttpClient<JwtProvider>((sp, httpClient) =>
        {
            httpClient.BaseAddress = new Uri(configuration["jwtProvider:tokenUri"] ?? string.Empty);
        });
    }

    public static void AddMvpos(this IServiceCollection services)
    {
        services.AddHttpClient<Mvpos>();
        services.AddScoped<Mvpos>();
    }
}