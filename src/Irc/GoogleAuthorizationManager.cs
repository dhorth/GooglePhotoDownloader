using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using GoogleDownloader.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDownloader.Irc
{

    /// <summary>
    /// Google Token Management class
    /// Holds a refresh token from an initial authorization
    /// then uses that refresh token to get an access token
    /// That access token is then injected into http calls
    /// usins <see cref="AuthenticatedHttpClientHandler"/>
    /// </summary>
    public class GoogleAuthorizationManager
    {
        private string accessToken;
        private string refreshToken;
        private const string token = "googlePhotoApiRefresh.token";
        private readonly AppSettings _appSettings;
        private readonly AuthenticationService _authorizationService;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="authorizationService"></param>
        public GoogleAuthorizationManager(AppSettings appSettings, AuthenticationService authorizationService)
        {
            _appSettings = appSettings;
            _authorizationService = authorizationService;
            if (File.Exists(token))
            {
                refreshToken = File.ReadAllText(token);
                Log.Logger.Information($"Using cached refresh token {refreshToken.Substring(0, 15)}...");
            }
            else
            {
                Log.Logger.Warning($"No cached refresh token");
            }
        }

        /// <summary>
        /// Check to see if we have a refresh token, if so
        /// then we can get an access token and should be 
        /// good to go
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    if (File.Exists(token))
                    {
                        refreshToken = File.ReadAllText(token);
                        Log.Logger.Information($"Using cached refresh token {refreshToken.Substring(0, 15)}...");
                    }
                    else
                    {
                        Log.Logger.Warning($"No cached refresh token");
                    }
                }
                return !string.IsNullOrWhiteSpace(refreshToken);
            }
        }

        /// <summary>
        /// Pass thru to the authenication service to
        /// authorize access with google
        /// <see cref="AuthenticationService"/>
        /// </summary>
        /// <returns></returns>
        public async Task Authorize()
        {
            Log.Logger.Information($"Authorize with google");
            var ret = await _authorizationService.Authorize();
            SetAccessToken(ret.Item1);
            SetRefreshToken(ret.Item2);
        }

        public string GetAccessToken()
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                GetAccessTokenFromRefeshToken();
            }
            return accessToken;
        }

        public void SetAccessToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Log.Logger.Error($"access token is empty");
            }
            else
            {
                Log.Logger.Information($"Setting access token to {value.Substring(0, 15)}...");
                accessToken = value;
            }
        }


        public string GetRefreshToken()
        {
            return refreshToken;
        }

        public void SetRefreshToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Log.Logger.Error($"Refresh token is empty");
            }
            else
            {
                Log.Logger.Information($"Setting refresh token to {value.Substring(0, 15)}...");
                refreshToken = value;
                File.WriteAllText(token, refreshToken);
            }
        }

        private bool GetAccessTokenFromRefeshToken()
        {
            Log.Logger.Information($"Using refresh token {refreshToken.Substring(0, 15)} to get new access token");
            var token = new TokenResponse { RefreshToken = refreshToken };
            var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _appSettings.GoogleClientID,
                        ClientSecret = _appSettings.GoogleClientSecret
                    }
                }), _appSettings.GoogleUser, token);

            accessToken = credentials.GetAccessTokenForRequestAsync().Result;
            return true;
        }

    }
}
