using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Herta.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using MvposSDK;
using Newtonsoft.Json;
using Radzen;

namespace Herta;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        });
        builder.Services.AddAuthorization();

        builder.Services.AddScoped<AuthManager>();
        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddHttpClient<Mvpos>();
        builder.Services.AddScoped<Mvpos>();

        builder.Services.AddRadzenComponents();

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(new
            {
                type = builder.Configuration["type"],
                project_id = builder.Configuration["project_id"],
                private_key_id = builder.Configuration["private_key_id"],
                private_key = builder.Configuration["private_key"],
                client_email = builder.Configuration["client_email"],
                client_id = builder.Configuration["client_id"],
                auth_uri = builder.Configuration["auth_uri"],
                token_uri = builder.Configuration["token_uri"],
                auth_provider_x509_cert_url = builder.Configuration["auth_provider_x509_cert_url"],
                client_x509_cert_url = builder.Configuration["client_x509_cert_url"],
                universe_domain = builder.Configuration["universe_domain"]
            }))
        });
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}