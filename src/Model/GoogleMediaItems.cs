using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// This is a collection of POCO that were converted from the 
/// google json objects  <see cref="https://developers.google.com/photos/library/reference/rest"/>
/// 
/// 
/// I used this site to perform the conversion
/// https://json2csharp.com/
/// </summary>
namespace GoogleDownloader.Model
{

    #region Request Objects
    public class GetAlbumsRequest
    {
        public int pageSize { get; set; }
        public string pageToken { get; set; } = null!;
    }

    public class PostAlbumRequest
    {
        public NewAlbum album { get; set; } = null!;

        public class NewAlbum
        {
            public string title { get; set; } = null!;
        }
    }

    public class BatchCreateMediaItemsRequest
    {
        public string albumId { get; set; } = null!;
        public List<BatchCreateMediaItemRequest> newMediaItems { get; set; } = new List<BatchCreateMediaItemRequest>();
    }

    public class BatchCreateMediaItemRequest
    {
        public string description { get; set; } = null!;
        public MediaItem simpleMediaItem { get; set; } = null!;

        public class MediaItem
        {
            public string fileName { get; set; } = null!;
            public string uploadToken { get; set; } = null!;
        }
    }

    public class SearchMediaItemsRequest
    {
        public SearchMediaItemsRequest(DateTime? start, DateTime? end)
        {
            filters = new SearchMediaItemsFilter(start, end);
        }

        public SearchMediaItemsFilter filters { get; set; } = null!;
        public int pageSize { get; set; }
        public string pageToken { get; set; } = null!;
    }

    public class FavoriteMediaItemsRequest
    {
        public FavoriteMediaItemsRequest(DateTime? since)
        {
            filters = new SearchMediaItemsFilter(since);
        }

        public SearchMediaItemsFilter filters { get; set; }
        public int pageSize { get; set; }
        public string pageToken { get; set; } = null!;
    }
    #endregion

    #region Response Objects

    public class GetAlbumsResponse
    {
        public GoogleAlbum[] Albums { get; set; } = null!;
        public string NextPageToken { get; set; } = null!;
    }
    public class SearchMediaItemsResponse
    {
        public MediaItem[] MediaItems { get; set; } = null!;
        public string NextPageToken { get; set; } = null!;
    }

    public class BatchCreateMediaItemsResponse
    {
        public List<BatchCreateMediaItemResponse> newMediaItemResults { get; set; } = null!;
    }

    public class BatchCreateMediaItemResponse
    {
        public string uploadToken { get; set; } = null!;
        public Status status { get; set; } = null!;

        public class Status
        {
            public string message { get; set; } = null!;
            public int code { get; set; }
        }
    }
    #endregion

    #region Filters
    public class SearchMediaItemsFilter
    {
        public SearchMediaItemsFilter(DateTime? start, DateTime? end = null)
        {
            if (start.HasValue)
                dateFilter = new SearchDateFilter(start.Value, end.GetValueOrDefault(DateTime.Now));
        }
        public SearchDateFilter dateFilter { get; set; }
        public SearchMediaItemsFeatureFilter featureFilter => new SearchMediaItemsFeatureFilter();
    }

    public class FavoritesMediaItemsFilter
    {
        public FavoritesMediaItemsFilter(DateTime? since)
        {
            if (since.HasValue)
                dateFilter = new SearchDateFilter(since.Value);
        }
        public SearchDateFilter dateFilter { get; set; }
        public SearchMediaItemsFeatureFilter featureFilter => new FavoritesMediaItemsFeatureFilter();
    }

    public class SearchMediaItemsFeatureFilter
    {
    }

    public class FavoritesMediaItemsFeatureFilter : SearchMediaItemsFeatureFilter
    {
        public string includedFeatures => "FAVORITES";
    }

    public class SearchDateFilter
    {
        public SearchDateFilter(DateTime start, DateTime? end = null)
        {
            ranges = new DateRangeFilter(start, end.GetValueOrDefault(DateTime.Now));
        }

        public DateRangeFilter ranges { get; set; }
    }

    public class DateRangeFilter
    {
        public DateRangeFilter(DateTime start, DateTime end)
        {
            startDate = new DateFilter(start);
            endDate = new DateFilter(end);
        }
        public DateFilter startDate { get; set; }
        public DateFilter endDate { get; set; }
    }

    public class DateFilter
    {
        public DateFilter(DateTime date)
        {
            year = date.Year;
            month = date.Month;
            day = date.Day;
        }
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
    }
    #endregion

    #region Data Objects

    public class MediaItem
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string ProductUrl { get; set; }
        public string BaseUrl { get; set; }
        public string MimeType { get; set; }
        public string Filename { get; set; }
        public MediaMetadata mediaMetadata { get; set; }
        public ContributorInfo contributorInfo { get; set; }
    }

    public class MediaMetadata
    {
        public DateTimeOffset CreationTime { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public GooglePhoto Photo { get; set; }
    }

    public class ContributorInfo { }

    public class GooglePhoto
    {
        public string CameraMake { get; set; }
        public string CameraModel { get; set; }
        public decimal FocalLength { get; set; }
        public decimal ApertureFNumber { get; set; }
        public decimal IsoEquivalent { get; set; }
        public string ExposureTime { get; set; }
    }

    public class GoogleAlbum
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ProductUrl { get; set; }
        public bool IsWriteable { get; set; }
        public int MediaItemsCount { get; set; }
        public string CoverPhotoBaseUrl { get; set; }
        public string CoverPhotoMediaItemId { get; set; }
        // ShareInfo
    }
    #endregion
}