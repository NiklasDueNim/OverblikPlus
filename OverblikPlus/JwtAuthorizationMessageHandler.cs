using System.Net.Http.Headers;
using System.Linq;

namespace OverblikPlus;

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
        // Håndterer preflight OPTIONS-anmodninger uden autorisation
        if (request.Method == HttpMethod.Options)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }

        var requestUri = request.RequestUri;

        if (_authorizedUrls.Any(url => requestUri.ToString().StartsWith(url, StringComparison.OrdinalIgnoreCase)))
        {
            var token = await _authStateProvider.GetTokenAsync();
            Console.WriteLine("Calling URL: " + requestUri);
            Console.WriteLine($"JWT token in handler: {token}");

            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"Using JWT: {token}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("No JWT available. Attempting to refresh token.");

                var refreshed = await _authStateProvider.RefreshTokenAsync();
                if (refreshed)
                {
                    token = await _authStateProvider.GetTokenAsync();
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    Console.WriteLine("Token refresh failed. Proceeding without token.");
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}