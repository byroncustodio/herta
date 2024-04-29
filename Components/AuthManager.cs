using System.Security.Authentication;
using System.Security.Claims;
using Herta.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using MvposSDK;

namespace Herta.Components;

public class AuthManager(Mvpos mvpos)
{
    public async Task ValidateUserAsync(string email, string password)
    {
        await mvpos.Users.Login(email, password);
    }

    public async Task ReValidateUserAsync(ClaimsPrincipal user)
    {
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            var email = user.Claims.FirstOrDefault(c => c.Type == "Email") ??
                        throw new Exception("Failed to find user claims");
                
            var password = user.Claims.FirstOrDefault(c => c.Type == "Password") ??
                           throw new Exception("Failed to find user claims");
                
            await ValidateUserAsync(email.Value, password.Value);
        }
    }

    private ApplicationUser GetUser(string email)
    {
        return new ApplicationUser
        {
            Email = email,
            UserName = email
        };
    }
    
    public ClaimsPrincipal AuthenticateUser(string email, string password)
    {
        // get user
        var user = GetUser(email);
        
        // validate user
        if (string.IsNullOrEmpty(user.Email))
        {
            throw new InvalidCredentialException("Email is required.");
        }

        var claims = new List<Claim>
        {
            new("Email", user.Email),
            new("Password", password),
            new("Role", "Vendor")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}