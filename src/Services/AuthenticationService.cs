using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using GoogleDownloader.Irc;

namespace GoogleDownloader.Services
{

    /// <summary>
    /// <see cref="https://developers.google.com/photos/library/guides/get-started"/>
    /// </summary>
    public class AuthenticationService
    {
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string codeChallengeMethod = "S256";
        private const string TokenRequestUrl = "https://www.googleapis.com/oauth2/v4/token";
        private const string requiredScope = "https://www.googleapis.com/auth/photoslibrary";

        private readonly string _clientId;
        private readonly string _secret;
        private readonly string _redirectUri;
        private readonly string _state;
        private readonly string _codeVerifier;

        public AuthenticationService(AppSettings appSettings)
        {
            _clientId = appSettings.GoogleClientID;
            _secret = appSettings.GoogleClientSecret;
            _redirectUri = appSettings.GoogleRedirectUri;
            _state = Utilities.GenerateRandomBase64url(32);
            _codeVerifier = Utilities.GenerateRandomBase64url(32);
        }

        public bool IsAuthorized => true;
        public async Task<(string, string)> Authorize()
        {
            Log.Logger.Information($"Calling Authorize()");
            // Generates state and PKCE values.
            string codeChallenge = Utilities.Sha256Base64(_codeVerifier);

            // Creates a redirect URI using an available port on the loopback address.
            Log.Logger.Information("redirect URI: " + _redirectUri);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add($"{_redirectUri}/");
            Log.Logger.Information("Listening..");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = $"{AuthorizationEndpoint}" +
                "?scope=" + HttpUtility.UrlEncode(requiredScope) +
                "&response_type=" + "code" +
                $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                $"&client_id={_clientId}" +
                $"&state={_state}" +
                "&approval_prompt = force" +
                "&access_type=offline" +
                $"&code_challenge={codeChallenge}" +
                $"&code_challenge_method={codeChallengeMethod}";

            // Opens request in the browser.
            var ps = new ProcessStartInfo(authorizationRequest)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings the Console to Focus.
            //BringConsoleToFront();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = "<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            responseOutput.Close();
            http.Stop();
            Log.Logger.Debug("HTTP server stopped.");

            // Checks for errors.
            string error = context.Request.QueryString.Get("error");
            if (error is object)
            {
                Log.Logger.Warning($"OAuth authorization error: {error}.");
                return ("", "");
            }
            if (context.Request.QueryString.Get("code") is null
                || context.Request.QueryString.Get("state") is null)
            {
                Log.Logger.Error($"Malformed authorization response. {context.Request.QueryString}");
                return ("", "");
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incomingState = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incomingState != _state)
            {
                Log.Logger.Warning($"Received request with invalid state ({incomingState})");
                return ("", "");
            }
            Log.Logger.Information("Authorization code: " + code);

            // Starts the code exchange at the Token Endpoint.
            var rc = await PerformCodeExchange(code);

            return rc;
        }


        private async Task<(string, string)> PerformCodeExchange(string code)
        {
            string accessToken = "";
            string refreshToken = "";

            Log.Information("Exchanging code for tokens...");

            // builds the  request
            // ReSharper disable once UseStringInterpolation
            //var tokenRequestBody = $"code={code}" +
            //    $"&redirect_uri={HttpUtility.UrlEncode(_redirectUri)}" +
            //    $"&client_id={_clientId}" +
            //    $"&client_secret={_secret}" +
            //    $"&scope=" +
            //    $"&grant_type=authorization_code" +
            //    $"&accessType=offline" +
            //    $"&approvalPrompt=force";

            var tokenRequestBody = $"code={code}" +
                $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                $"&client_id={_clientId}" +
                $"&code_verifier={_codeVerifier}" +
                $"&client_secret={_secret}" +
                "&scope=" +
                "&grant_type=authorization_code" +
                "&accessType=offline" +
                "&approvalPrompt=force" +
                "";

            // sends the request
            var tokenRequest = (HttpWebRequest)WebRequest.Create(TokenRequestUrl);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = byteVersion.Length;
            var stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(byteVersion, 0, byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                var tokenResponse = await tokenRequest.GetResponseAsync();

                // ReSharper disable once AssignNullToNotNullAttribute
                using var reader = new StreamReader(tokenResponse.GetResponseStream());
                // reads response body
                var responseText = await reader.ReadToEndAsync();
                Log.Information(responseText);

                // converts to dictionary
                var tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                accessToken = tokenEndpointDecoded["access_token"];
                if (tokenEndpointDecoded.ContainsKey("refresh_token"))
                {
                    refreshToken = tokenEndpointDecoded["refresh_token"];
                }
                //var  refreshToken = tokenEndpointDecoded["refresh_token"];
                //_token.SetRefreshToken(refreshToken);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError)
                    return ("", "");

                if (!(ex.Response is HttpWebResponse response))
                    return ("", "");

                Log.Information("HTTP: " + response.StatusCode);
                // ReSharper disable once AssignNullToNotNullAttribute
                using var reader = new StreamReader(response.GetResponseStream());
                var responseText = await reader.ReadToEndAsync();
                Log.Information(responseText);
            }
            return (accessToken, refreshToken);
            //return null;
        }
    }
}
