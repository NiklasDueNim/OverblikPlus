using System.Net.Http.Headers;

namespace OverblikPlus.AuthHelpers;

public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly CustomAuthStateProvider _authStateProvider;
    private HashSet<string> _authorizedUrls;

    public JwtAuthorizationMessageHandler(CustomAuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
        _authorizedUrls = new HashSet<string>();
    }

    public JwtAuthorizationMessageHandler ConfigureHandler(IEnumerable<string> authorizedUrls)
    {
        _authorizedUrls = new HashSet<string>(authorizedUrls, StringComparer.OrdinalIgnoreCase);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Options)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        var requestUri = request.RequestUri;
        if (requestUri == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var fullUrl = requestUri.ToString();
        Console.WriteLine($"[JwtAuthorizationMessageHandler] Processing request to: {fullUrl}");
        Console.WriteLine($"[JwtAuthorizationMessageHandler] Authorized URLs: {string.Join(", ", _authorizedUrls)}");

        // Check if the request URI matches any authorized URL
        var isAuthorizedUrl = _authorizedUrls.Any(url => fullUrl.StartsWith(url, StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"[JwtAuthorizationMessageHandler] Is authorized URL: {isAuthorizedUrl}");

        if (isAuthorizedUrl)
        {
            var token = await _authStateProvider.GetTokenAsync();
            Console.WriteLine($"[JwtAuthorizationMessageHandler] Token present: {!string.IsNullOrEmpty(token)}");

            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"[JwtAuthorizationMessageHandler] Adding Authorization header");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("[JwtAuthorizationMessageHandler] No JWT available. Attempting to refresh token.");

                var refreshed = await _authStateProvider.RefreshTokenAsync();
                if (refreshed)
                {
                    token = await _authStateProvider.GetTokenAsync();
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine("[JwtAuthorizationMessageHandler] Token refreshed, adding Authorization header");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    else
                    {
                        Console.WriteLine("[JwtAuthorizationMessageHandler] Token refresh failed - no token available.");
                    }
                }
                else
                {
                    Console.WriteLine("[JwtAuthorizationMessageHandler] Token refresh failed. Proceeding without token.");
                }
            }
        }
        else
        {
            Console.WriteLine($"[JwtAuthorizationMessageHandler] URL not in authorized list, skipping JWT");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}