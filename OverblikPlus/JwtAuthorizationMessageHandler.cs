using System.Net.Http.Headers;

public class JwtAuthorizationMessageHandler : DelegatingHandler
{
    private readonly CustomAuthStateProvider _authStateProvider;
    private HashSet<string> _authorizedUrls;

    public JwtAuthorizationMessageHandler(CustomAuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
        _authorizedUrls = new HashSet<string>();
    }

    // Metode til at konfigurere autoriserede URL'er
    public JwtAuthorizationMessageHandler ConfigureHandler(IEnumerable<string> authorizedUrls)
    {
        _authorizedUrls = new HashSet<string>(authorizedUrls, StringComparer.OrdinalIgnoreCase);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Tjek om URL'en er i de autoriserede URL'er
        if (_authorizedUrls.Contains(request.RequestUri?.GetLeftPart(UriPartial.Authority)))
        {
            var token = await _authStateProvider.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}