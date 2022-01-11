using GoogleDownloader.Model;
using Refit;
using System.IO;
using System.Threading.Tasks;

namespace GoogleDownloader.Services
{

    /// <summary>
    /// Refit interface definitions for google
    /// photo apis
    /// 
    /// <see cref="https://developers.google.com/photos/library/reference/rest"/>
    /// also <see cref="AuthenticatedHttpClientHandler"/> for the access token 
    /// injection on each call
    /// </summary>
    [Headers("Authorization: Bearer X")]
    public interface IGooglePhotosApi
    {
        /* Albums */
        [Get("/v1/albums")]
        Task<GetAlbumsResponse> GetAlbums([Query] GetAlbumsRequest request);

        [Post("/v1/albums")]
        Task<GoogleAlbum> CreateAlbum([Body] PostAlbumRequest request);

        /* Uploads */
        [Post("/v1/uploads")]
        [Headers("Content-type: application/octet-stream", "X-Goog-Upload-Protocol: raw")]
        Task<string> UploadFile([Body] Stream request, [Header("X-Goog-Upload-Content-Type")] string mimeType);

        /* Media Items */
        [Post("/v1/mediaItems:batchCreate")]
        Task<BatchCreateMediaItemsResponse> BatchCreateMediaItems([Body] BatchCreateMediaItemsRequest request);

        [Post("/v1/mediaItems:search")]
        Task<SearchMediaItemsResponse> SearchMediaItems([Body] SearchMediaItemsRequest request);

        [Post("/v1/mediaItems:search")]
        Task<SearchMediaItemsResponse> FavoriteMediaItems([Body] FavoriteMediaItemsRequest request);


    }

}