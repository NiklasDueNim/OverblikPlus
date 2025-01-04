using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using OverblikPlus.Dtos.Auth;

namespace OverblikPlus;

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
        Console.WriteLine($"SetTokenAsync called. JWT: {token}, RefreshToken: {refreshToken}");
        _jwtToken = token;
        _refreshToken = refreshToken;
        
        Console.WriteLine($"SetTokenAsync called. JWT: {_jwtToken}, RefreshToken: {_refreshToken}");

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task RemoveTokenAsync()
    {
        _jwtToken = null;
        _refreshToken = null;

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        
        if (string.IsNullOrEmpty(_jwtToken) || IsTokenExpired(_jwtToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(_jwtToken), "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public Task<string> GetTokenAsync()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            Console.WriteLine("JWT token is missing in GetTokenAsync.");
        }
        else
        {
            Console.WriteLine($"JWT token found in GetTokenAsync: {_jwtToken}");
        }
        return Task.FromResult(_jwtToken);
    }

    public Task<string> GetRefreshTokenAsync() => Task.FromResult(_refreshToken);

    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            Console.WriteLine("Refresh token is missing in RefreshTokenAsync.");
            return false;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/refresh", new { refreshToken });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    Console.WriteLine($"Token refreshed. New JWT: {result.Token}, New RefreshToken: {result.RefreshToken}");
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

    public string GetUserIdAsync()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            Console.WriteLine("JWT token is missing.");
            return null;
        }

        var claims = ParseClaimsFromJwt(_jwtToken);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            Console.WriteLine("User ID claim is missing in the token.");
        }

        return userIdClaim;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(AddPadding(payload));
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            foreach (var kvp in keyValuePairs)
            {
                Console.WriteLine($"Claim: {kvp.Key} = {kvp.Value}");
            }

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing claims from JWT: {ex.Message}");
            return Enumerable.Empty<Claim>();
        }
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

    public async Task<string> GetRoleAsync()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            return null;
        }

        var claims = ParseClaimsFromJwt(_jwtToken);
        return claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
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