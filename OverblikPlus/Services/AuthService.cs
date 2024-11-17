using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace OverblikPlus.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = (CustomAuthStateProvider)authStateProvider;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var loginDto = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null)
            {
                await _authStateProvider.SetTokenAsync(result.Token, result.RefreshToken);
                return true;
            }
        }

        return false;
    }

    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync("api/Auth/logout", null);
        await _authStateProvider.RemoveTokenAsync();
    }

    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await _authStateProvider.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken)) return false;

        var response = await _httpClient.PostAsJsonAsync("api/Auth/refresh", new { refreshToken });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null)
            {
                await _authStateProvider.SetTokenAsync(result.Token, result.RefreshToken);
                return true;
            }
        }

        return false;
    }

}

public class LoginResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}