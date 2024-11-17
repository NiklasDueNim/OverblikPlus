using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using OverblikPlus.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private string _jwtToken;
    private string _refreshToken;

    public async Task SetTokenAsync(string token, string refreshToken)
    {
        _jwtToken = token;
        _refreshToken = refreshToken;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task RemoveTokenAsync()
    {
        _jwtToken = null;
        _refreshToken = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = string.IsNullOrEmpty(_jwtToken)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(ParseClaimsFromJwt(_jwtToken), "jwt");

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public Task<string> GetTokenAsync() => Task.FromResult(_jwtToken);

    public Task<string> GetRefreshTokenAsync() => Task.FromResult(_refreshToken);
    
    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken)) return false;

        // Brug en HttpClient til at kontakte backend og få en ny token
        var httpClient = new HttpClient { BaseAddress = new Uri("https://overblikplus-auth-api.azurewebsites.net") };
        var response = await httpClient.PostAsJsonAsync("api/Auth/refresh", new { refreshToken });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null)
            {
                await SetTokenAsync(result.Token, result.RefreshToken);
                return true;
            }
        }

        return false;
    }
    
    public Task<string> GetUserIdAsync()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            return Task.FromResult<string>(null);
        }

        var claims = ParseClaimsFromJwt(_jwtToken);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

        return Task.FromResult(userIdClaim);
    }



    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(AddPadding(payload));
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    private string AddPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: return base64 + "==";
            case 3: return base64 + "=";
            default: return base64;
        }
    }
}