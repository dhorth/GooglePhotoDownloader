using Serilog;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleDownloader.Irc
{
    /// <summary>
    /// Handler to intercept each HttpRequest and Replace Authorization header with with Bearer Token from Google.
    /// </summary>
    public class AuthenticatedHttpClientHandler : DelegatingHandler
    {
        private GoogleAuthorizationManager _token;
        public AuthenticatedHttpClientHandler(GoogleAuthorizationManager accessToken)
        {
            _token = accessToken;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // See if the request has an authorize header
            var auth = request.Headers.Authorization;

            var token = _token.GetAccessToken();
            if (auth != null && !string.IsNullOrWhiteSpace(token))
            {
                //var cred = await _googleAuthProvider.GetCredentialAsync();
                //var token = await cred.UnderlyingCredential.GetAccessTokenForRequestAsync();
                Log.Logger.Information($"Using Access token {token.Substring(0, 15)} for request {request.RequestUri}");
                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}