namespace Herta.Auth;

internal sealed class JwtProvider(HttpClient httpClient)
{
    public async Task<string> GetForCredentialsAsync(string email, string password)
    {
        var request = new
        {
            email,
            password,
            returnSecureToken = true
        };

        var response = await httpClient.PostAsJsonAsync("", request);
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        return tokenResponse?.IdToken ?? string.Empty;
    }
}