using System.Net.Http.Json;
using AutoMapper;
using OverblikPlus.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using OverblikPlus.AuthHelpers;
using OverblikPlus.Common;
using OverblikPlus.Models;
using OverblikPlus.Models.Dtos.Auth;
using OverblikPlus.Models.Dtos.User;

namespace OverblikPlus.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly CustomAuthStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, IMapper mapper)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        public async Task<Result<LoginResponse>> LoginAsync(string email, string password)
        {
            var loginDto = new { Email = email, Password = password };
            Console.WriteLine("Sending login request...");

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);
                Console.WriteLine($"Login response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        Console.WriteLine($"Login successful. Token: {result.Token}");
                        
                        var user = _mapper.Map<User>(result.User);
                        _authStateProvider.SetLogin(result.Token, result.RefreshToken, user);
                        return Result<LoginResponse>.SuccessResult(result);
                    }
                    else
                    {
                        Console.WriteLine("Login response is null or token is empty.");
                        return Result<LoginResponse>.ErrorResult("Login response is null or token is empty");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login failed. Status: {response.StatusCode}, Error: {errorContent}");
                    return Result<LoginResponse>.ErrorResult("Login failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during login: {ex.Message}");
                return Result<LoginResponse>.ErrorResult("Login failed");
            }
        }
    

    public async Task LogoutAsync()
        {
            await _authStateProvider.RemoveTokenAsync();
            Console.WriteLine("User logged out successfully.");
        }

        public async Task<bool> RegisterAsync(CreateUserDto createUserDto)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during registration: {ex.Message}");
                return false;
            }
        }
    }
}