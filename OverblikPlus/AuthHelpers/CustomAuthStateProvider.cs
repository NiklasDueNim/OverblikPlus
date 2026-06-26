using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using OverblikPlus.Models;
using OverblikPlus.Models.Dtos.Auth;

namespace OverblikPlus.AuthHelpers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "op_jwt";
    private const string RefreshTokenKey = "op_refresh";
    private const string UserKey = "op_user";

    public User User { get; private set; }
    private string _jwtToken;
    private string _refreshToken;
    private bool _loadedFromStorage;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly ILocalStorageService _localStorage;


    public CustomAuthStateProvider(HttpClient httpClient, IMapper mapper, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _localStorage = localStorage;
    }

    public async Task SetLoginAsync(string token, string refreshToken, User user)
    {
        _jwtToken = token;
        _refreshToken = refreshToken;
        User = user;
        _loadedFromStorage = true;

        await PersistSessionAsync();

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task RemoveTokenAsync()
    {
        _jwtToken = null;
        _refreshToken = null;
        User = null;
        _loadedFromStorage = true;

        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
        await _localStorage.RemoveItemAsync(UserKey);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await EnsureLoadedFromStorageAsync();

        if (string.IsNullOrEmpty(_jwtToken) || IsTokenExpired(_jwtToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(_jwtToken), "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<string> GetTokenAsync()
    {
        await EnsureLoadedFromStorageAsync();
        return _jwtToken;
    }

    public async Task<string> GetRefreshTokenAsync()
    {
        await EnsureLoadedFromStorageAsync();
        return _refreshToken;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
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
                    var user = _mapper.Map<User>(result.User);
                    await SetLoginAsync(result.Token, result.RefreshToken, user);
                    return true;
                }
            }
        }
        catch
        {
            // Refresh failed; caller falls back to re-authentication.
        }

        return false;
    }

    public string GetUserId()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            return null;
        }

        var claims = ParseClaimsFromJwt(_jwtToken);
        return claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
    }

    public string GetRole()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            return null;
        }

        var claims = ParseClaimsFromJwt(_jwtToken);
        return claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
    }

    public int? GetBostedId()
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            return null;
        }

        var claims = ParseClaimsFromJwt(_jwtToken);
        var bostedIdClaim = claims.FirstOrDefault(c => c.Type == "bostedId")?.Value;
        if (!string.IsNullOrEmpty(bostedIdClaim) && int.TryParse(bostedIdClaim, out var bostedId))
        {
            return bostedId;
        }
        return null;
    }

    private async Task EnsureLoadedFromStorageAsync()
    {
        if (_loadedFromStorage)
        {
            return;
        }
        _loadedFromStorage = true;

        try
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (string.IsNullOrEmpty(token) || IsTokenExpired(token))
            {
                return;
            }

            _jwtToken = token;
            _refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
            User = await _localStorage.GetItemAsync<User>(UserKey);
        }
        catch
        {
            // Corrupt or unavailable storage: stay logged out rather than crash.
            _jwtToken = null;
            _refreshToken = null;
            User = null;
        }
    }

    private async Task PersistSessionAsync()
    {
        await _localStorage.SetItemAsync(TokenKey, _jwtToken);
        await _localStorage.SetItemAsync(RefreshTokenKey, _refreshToken);
        await _localStorage.SetItemAsync(UserKey, User);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(AddPadding(payload));
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }
        catch
        {
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
