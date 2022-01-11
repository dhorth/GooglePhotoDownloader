using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleDownloader.Irc;
using GoogleDownloader.Model;
using Serilog;

namespace GoogleDownloader.Services
{
    /// <summary>
    /// This service calls the photo api
    /// expand with more calls
    /// </summary>
    public class GooglePhotoService
    {
        private readonly IGooglePhotosApi _googlePhotosApi;
        private readonly GoogleAuthorizationManager _token;


        /// <summary>
        /// Service wrapper for google photos api
        /// <see cref="GoogleAuthorizationManager"/> for authenication
        /// <see cref="IGooglePhotosApi"/> for implentatio of REST API
        /// </summary>
        /// <param name="token"></param>
        /// <param name="googlePhotosApi"></param>
        public GooglePhotoService(
            GoogleAuthorizationManager token,
            IGooglePhotosApi googlePhotosApi
            )
        {
            _token = token;
            _googlePhotosApi = googlePhotosApi;
        }


        /// <summary>
        /// Get all the images for any particular year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<List<MediaItem>> GetImagesByYear(int year)
        {
            Log.Logger.Debug($"Calling GetImagesByYear({year})");

            if (!_token.IsActive)
            {
                Log.Logger.Error($"Need Access Token first");
                return null;
            }
            var titles = new List<MediaItem>();
            try
            {
                const int pageSize = 25; //20; // limit is 50

                SearchMediaItemsResponse response;
                string pageToken = string.Empty;
                do
                {
                    Log.Logger.Verbose($"GetImagesByYear token=>{pageToken}");

                    var start = new DateTime(year, 1, 1);
                    var request = new SearchMediaItemsRequest(start, start.AddYears(1).AddDays(-1)) { pageSize = pageSize, pageToken = pageToken };
                    response = await _googlePhotosApi.SearchMediaItems(request);
                    if (response == null)
                        throw new Exception($"SearchMediaItems returned null response");

                    if (response.MediaItems != null)
                    {
                        Log.Logger.Debug($"Adding {response.MediaItems.Length} to result set");
                        titles.AddRange(response.MediaItems.ToList());
                    }
                    pageToken = response.NextPageToken;

                } while (response.NextPageToken != null);

                Log.Logger.Information($"GetFavorites()=>{titles.Count()}");
            }
            catch (Refit.ApiException ex)
            {
                Log.Error("Refit Get Favorites", ex);
            }
            catch (Exception ex)
            {
                Log.Error($"General Exception Get Favorites - {ex.ToString()}", ex);
                return null;
            }
            return titles;
        }
    }
}

