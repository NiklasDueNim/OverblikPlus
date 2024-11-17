using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using OverblikPlus.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private string _jwtToken;
    private string _refreshToken;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
        if (string.IsNullOrEmpty(_jwtToken) || IsTokenExpired(_jwtToken))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(_jwtToken), "jwt");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public Task<string> GetTokenAsync() => Task.FromResult(_jwtToken);

    public Task<string> GetRefreshTokenAsync() => Task.FromResult(_refreshToken);

    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            Console.WriteLine("Refresh token is missing.");
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/refresh", new { refreshToken });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null)
                {
                    await SetTokenAsync(result.Token, result.RefreshToken);
                    return true;
                }
            }
            else
            {
                Console.WriteLine($"Failed to refresh token. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during token refresh: {ex.Message}");
        }

        return false;
    }

    public Task<string> GetUserIdAsync()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            Console.WriteLine("JWT token is missing.");
            return Task.FromResult<string>(null);
        }

        var claims = ParseClaimsFromJwt(_jwtToken);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            Console.WriteLine("User ID claim is missing in the token.");
        }

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

    private bool IsTokenExpired(string jwt)
    {
        var claims = ParseClaimsFromJwt(jwt);
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        if (expClaim != null && long.TryParse(expClaim, out var exp))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
            return expirationTime <= DateTime.UtcNow;
        }

        return true;
    }
}
