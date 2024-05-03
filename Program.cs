using System.Reflection;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using Herta.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        SetGoogleAuthentication(config);
        
        builder.Services.AddSecretManagerServiceClient();

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

        var secretManager = app.Services.GetService<SecretManagerServiceClient>();
        
        var secretResponse = secretManager?.AccessSecretVersion(new SecretVersionName(config["googleCloud:projectId"],
            config["googleCloud:secrets:firebaseAuth"], "latest"));

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

    private static void SetGoogleAuthentication(IConfiguration config)
    {
        JObject googleServiceAccount;

        using (var stream = Assembly.GetExecutingAssembly()
                   .GetManifestResourceStream(config["googleCloud:serviceAccount:credentials"] ??
                                              throw new InvalidOperationException()))
        {
            using (var reader = new JsonTextReader(new StreamReader(stream ?? throw new InvalidOperationException())))
            {
                var serializer = new JsonSerializer();
                googleServiceAccount = JObject.FromObject(serializer.Deserialize(reader) ?? throw new InvalidOperationException());
            }
        }

        googleServiceAccount.Add("private_key", config["googleCloud:serviceAccount:key"]);

        Console.WriteLine(config["googleCloud:serviceAccount:key"]);
        Console.WriteLine(googleServiceAccount.ToString());
        
        using (var fs = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.None,
                   4096, FileOptions.Encrypted))
        {
            var data = new UTF8Encoding(true).GetBytes(googleServiceAccount.ToString());
            fs.Write(data, 0, data.Length);
            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fs.Name);
        }
    }
}