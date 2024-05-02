using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using Herta.Components;
using Radzen;

namespace Herta;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration.AddUserSecrets<Program>().Build();
        
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddRadzenComponents();
        
        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddAppAuthentication(config);
        
        builder.Services.AddMvpos();

        builder.Services.AddSecretManagerServiceClient();

        var app = builder.Build();

        var secretManager = app.Services.GetService<SecretManagerServiceClient>();
        SecretVersionName secretVersionName;
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            
            secretVersionName = new SecretVersionName(config["googleCloud:projectId"],
                config["googleCloud:secrets:firebaseAuth"], "latest");
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();

            secretVersionName = new SecretVersionName(Environment.GetEnvironmentVariable("googleCloud:projectId"),
                Environment.GetEnvironmentVariable("googleCloud:secrets:firebaseAuth"), "latest");
        }

        var secretResponse = secretManager?.AccessSecretVersion(secretVersionName);

        if (secretResponse != null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromStream(new MemoryStream(secretResponse.Payload.Data.ToByteArray()))
            });
        }
        else
        {
            throw new NullReferenceException("Failed to find secret for initializing auth");
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