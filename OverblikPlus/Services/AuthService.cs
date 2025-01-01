using System.Net.Http.Json;
using OverblikPlus.Dtos.User;
using OverblikPlus.Dtos.Auth;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;


namespace OverblikPlus.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        public async Task<(string Token, string RefreshToken)> LoginAsync(string email, string password)
        {
            var loginDto = new { Email = email, Password = password };
            Console.WriteLine("Sending login request...");

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);
                Console.WriteLine($"Login response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        Console.WriteLine($"Login successful. Token: {result.Token}");
                        await _authStateProvider.SetTokenAsync(result.Token, result.RefreshToken);
                        return (result.Token, result.RefreshToken);
                    }
                    else
                    {
                        Console.WriteLine("Login response is null or token is empty.");
                        return (null, null);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login failed. Status: {response.StatusCode}, Error: {errorContent}");
                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during login: {ex.Message}");
                return (null, null);
            }
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.RemoveTokenAsync();
            Console.WriteLine("User logged out successfully.");
        }

        public async Task<bool> RegisterAsync(CreateUserDto createUserDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", createUserDto);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Registration failed: {errorContent}");
            return false;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = await _authStateProvider.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                Console.WriteLine("No refresh token available.");
                return false;
            }

            var response = await _httpClient.PostAsJsonAsync("api/Auth/refresh", new { refreshToken });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    await _authStateProvider.SetTokenAsync(result.Token, result.RefreshToken);
                    return true;
                }
            }

            Console.WriteLine($"Failed to refresh token. Status code: {response.StatusCode}");
            return false;
        }
    }
}
