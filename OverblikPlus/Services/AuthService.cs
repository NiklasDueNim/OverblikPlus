using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace OverblikPlus.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    
    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }
    
    public async Task<bool> LoginAsync(string username, string password)
    {
        var loginDto = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);

        if (response.IsSuccessStatusCode)
        {
            var jwtToken = await response.Content.ReadAsStringAsync();
            await ((CustomAuthStateProvider)_authStateProvider).SetTokenAsync(jwtToken);
            return true;
        }

        return false;
    }
    
    public async Task LogoutAsync()
    {
        await ((CustomAuthStateProvider)_authStateProvider).RemoveTokenAsync();
    }
}