using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace OverblikPlus.AuthHelpers;

/// <summary>
/// En HttpClient message handler der automatisk tilføjer JWT Bearer tokens til requests
/// til autoriserede API endpoints. Håndterer også automatisk token refresh ved 401 fejl.
/// </summary>
public sealed class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly ILogger<JwtAuthorizationMessageHandler> _logger;
    private readonly HashSet<(string Scheme, string Host, int? Port)> _authorizedOrigins;

    /// <summary>
    /// Opretter en ny JWT Authorization Message Handler
    /// </summary>
    /// <param name="authStateProvider">Provider til at hente og forny JWT tokens</param>
    /// <param name="logger">Logger til at logge operationer</param>
    /// <param name="authorizedUrls">Liste over base URLs hvor JWT tokens skal tilføjes (fx "http://localhost:5002")</param>
    public JwtAuthorizationMessageHandler(
        CustomAuthStateProvider authStateProvider,
        ILogger<JwtAuthorizationMessageHandler> logger,
        IEnumerable<string> authorizedUrls)
    {
        _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (authorizedUrls is null) 
            throw new ArgumentNullException(nameof(authorizedUrls));

        // Parse autoriserede URLs til (Scheme, Host, Port) tuples for sikker matching
        _authorizedOrigins = new HashSet<(string, string, int?)>(
            authorizedUrls
                .Select(url =>
                {
                    var uri = new Uri(url, UriKind.Absolute);
                    // Normaliser port: hvis default port (80 for http, 443 for https), så null
                    var port = uri.IsDefaultPort ? (int?)null : uri.Port;
                    return (uri.Scheme, uri.Host, port);
                }),
            OriginComparer.Instance);

        _logger.LogDebug(
            "Initialized JwtAuthorizationMessageHandler with {Count} authorized origins: {Origins}",
            _authorizedOrigins.Count,
            string.Join(", ", authorizedUrls));
    }

    /// <summary>
    /// Intercepter alle HTTP requests og tilføjer JWT token hvis request går til en autoriseret origin.
    /// Håndterer også automatisk token refresh ved 401 Unauthorized fejl.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Valider request
        if (request?.RequestUri is null)
        {
            _logger.LogWarning("Request or RequestUri is null, passing through without modification");
            return await base.SendAsync(request!, cancellationToken);
        }

        var requestUri = request.RequestUri;

        // Tjek om request går til en autoriseret origin
        var isAuthorized = IsAuthorizedOrigin(requestUri);

        // Hvis request går til en autoriseret origin, tilføj JWT token
        if (isAuthorized)
        {
            // Respekter eksisterende Authorization header (fx hvis sat manuelt for service-to-service calls)
            if (request.Headers.Authorization is null)
            {
                var token = await _authStateProvider.GetTokenAsync();
                
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogTrace("Added Bearer token to request to {Uri}", requestUri);
                }
                else
                {
                    _logger.LogDebug("No token available for request to {Uri}, attempting refresh", requestUri);
                    
                    // Prøv at refresh token før første request
                    var refreshed = await _authStateProvider.RefreshTokenAsync();
                    if (refreshed)
                    {
                        token = await _authStateProvider.GetTokenAsync();
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            _logger.LogDebug("Token refreshed successfully, added to request to {Uri}", requestUri);
                        }
                    }
                }
            }
            else
            {
                _logger.LogTrace("Request already has Authorization header, skipping token addition");
            }
        }

        // Send første request
        var response = await base.SendAsync(request, cancellationToken);

        // Hvis vi får 401 Unauthorized og request går til autoriseret origin, prøv token refresh + retry
        if (response.StatusCode == HttpStatusCode.Unauthorized && isAuthorized)
        {
            _logger.LogInformation(
                "Received 401 Unauthorized for request to {Uri}. Attempting token refresh and retry.",
                requestUri);

            // Dispose response for at undgå socket leaks
            response.Dispose();

            // Prøv at refresh token
            var refreshed = await _authStateProvider.RefreshTokenAsync();
            if (refreshed)
            {
                var newToken = await _authStateProvider.GetTokenAsync();
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    // Klon request hvis nødvendigt (for at undgå "content already consumed" fejl)
                    var clonedRequest = await CloneRequestIfNeededAsync(request, cancellationToken);

                    // Sæt nyt token (kun hvis det ikke allerede er sat eller hvis det er en Bearer token)
                    if (clonedRequest.Headers.Authorization is null ||
                        clonedRequest.Headers.Authorization.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                    {
                        clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    }

                    _logger.LogDebug("Token refreshed, retrying request to {Uri}", requestUri);
                    return await base.SendAsync(clonedRequest, cancellationToken);
                }
            }

            _logger.LogWarning(
                "Token refresh failed or no new token available for request to {Uri}. Returning 401.",
                requestUri);
            
            // Hvis refresh fejler, returner en ny 401 response
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        return response;
    }

    /// <summary>
    /// Tjekker om en URI matcher en af de autoriserede origins.
    /// Matching sker på Scheme (http/https), Host (fx localhost), og Port (fx 5002).
    /// </summary>
    private bool IsAuthorizedOrigin(Uri uri)
    {
        var origin = (
            Scheme: uri.Scheme,
            Host: uri.Host,
            Port: uri.IsDefaultPort ? (int?)null : uri.Port
        );

        var isAuthorized = _authorizedOrigins.Contains(origin);
        
        _logger.LogTrace(
            "Checking authorization for {Scheme}://{Host}{Port} -> {Authorized}",
            origin.Scheme,
            origin.Host,
            origin.Port.HasValue ? $":{origin.Port.Value}" : "",
            isAuthorized ? "Authorized" : "Not authorized");

        return isAuthorized;
    }

    /// <summary>
    /// Kloner en HttpRequestMessage hvis den har content, så vi kan sende den igen efter token refresh.
    /// HttpClient's HttpContent kan kun læses én gang, så vi skal buffer'e det hvis vi skal retry.
    /// </summary>
    private static async Task<HttpRequestMessage> CloneRequestIfNeededAsync(
        HttpRequestMessage original, 
        CancellationToken cancellationToken)
    {
        // Hvis der ikke er content, kan vi bruge originalen direkte
        if (original.Content == null)
        {
            return original;
        }

        // Opret en klon af request
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version,
            VersionPolicy = original.VersionPolicy
        };

        // Kopiér alle request headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Kopiér alle request properties/options (fx custom headers)
        foreach (var option in original.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);
        }

        // Buffer content så vi kan læse det igen
        var memoryStream = new MemoryStream();
        await original.Content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        
        var clonedContent = new StreamContent(memoryStream);

        // Kopiér alle content headers
        foreach (var header in original.Content.Headers)
        {
            clonedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        clone.Content = clonedContent;

        return clone;
    }

    /// <summary>
    /// Comparer til at sammenligne (Scheme, Host, Port) tuples case-insensitive.
    /// </summary>
    private sealed class OriginComparer : IEqualityComparer<(string Scheme, string Host, int? Port)>
    {
        public static readonly OriginComparer Instance = new();

        public bool Equals((string Scheme, string Host, int? Port) x, (string Scheme, string Host, int? Port) y)
        {
            return string.Equals(x.Scheme, y.Scheme, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Host, y.Host, StringComparison.OrdinalIgnoreCase)
                && x.Port == y.Port;
        }

        public int GetHashCode((string Scheme, string Host, int? Port) obj)
        {
            var schemeHash = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Scheme ?? "");
            var hostHash = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Host ?? "");
            var portHash = obj.Port?.GetHashCode() ?? 0;
            
            return HashCode.Combine(schemeHash, hostHash, portHash);
        }
    }
}
