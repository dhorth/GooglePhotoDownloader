using GoogleDownloader.Irc;
using GoogleDownloader.Model;
using GoogleDownloader.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GoogleDownloader
{
    /// <summary>
    /// This is where the magic is realized, the implementation here
    /// is hard coded in the Run method
    /// </summary>
    public class Client : ConsoleApplication
    {
        private readonly GooglePhotoService _service;
        private readonly GoogleAuthorizationManager _authenticationManager;
        public Client(GooglePhotoService service, GoogleAuthorizationManager authenticationManager)
        {
            _service = service;
            _authenticationManager = authenticationManager;
        }


        /// <summary>
        /// Download all images for any particular year.  I have hard coded
        /// in an example to setup for this years images, but you could pass 
        /// in your own command line arguments
        /// Using http client to download multiple files
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task<int> Run(string[] args)
        {
            int year = DateTime.Now.Year;
            int endYear = DateTime.Now.AddYears(1).Year;
            if (!_authenticationManager.IsActive)
                await _authenticationManager.Authorize();

            while (year <= endYear)
            {
                try
                {
                    Log.Logger.Information($"Starting {year}");
                    var path = DirectoryHelper.CreateDirectory("images", year.ToString());

                    //call our service to get the list of images this will return
                    //a list of media items.  DO NOT cache the url in the media item
                    //google uses temporary urls there is little value in saving them
                    var images = await _service.GetImagesByYear(year);
                    Log.Logger.Information($"Found {images.Count} in {year}");

                    var sw = new Stopwatch();
                    sw.Start();
                    await DownlodFiles(path, images);
                    Log.Logger.Debug($"Completed {year} took {sw.Elapsed.TotalSeconds} seconds");
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, year.ToString());
                }
                year++;
            }
            Log.Logger.Information("All done!  Press any key to exit");
            Console.ReadKey();
            return 0;
        }

        /// <summary>
        /// Walk the list of images return from the google api
        /// then async download the image using the temporary url
        /// </summary>
        /// <param name="path"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        private static async Task DownlodFiles(string path, List<MediaItem> images)
        {
            //use the web client to download the images using the temporary urls
            using (var web = new HttpClient())
            {
                var taskList = new List<Task>();
                foreach (var image in images)
                {
                    var fileName = Path.Combine(path, image.Filename);
                    if (!File.Exists(fileName))
                    {
                        var task = DownloadImage(web, image, fileName);
                        if (task != null)
                            taskList.Add(task);
                    }
                    else
                    {
                        Log.Logger.Warning($"{image.Filename} already exists");
                    }
                }
                await Task.WhenAll(taskList);
            }
        }

        /// <summary>
        /// Download the image 
        /// </summary>
        /// <param name="web"></param>
        /// <param name="image"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static Task DownloadImage(HttpClient web, MediaItem image, string fileName)
        {
            Task task = null;
            try
            {
                Log.Logger.Verbose($"Starting download {image.BaseUrl}");
                task = web.GetAsync($"{image.BaseUrl}=d")
                    .ContinueWith((a) =>
               {
                   try
                   {
                       using (var stream = a.Result.Content.ReadAsStream())
                       {
                           var fi = new FileInfo(fileName);
                           using (var fileStream = fi.OpenWrite())
                           {
                               stream.CopyTo(fileStream);
                           }
                           if (fi.Exists && fi.Length > 0)
                               Log.Logger.Information($"{image.Filename} successfully downloaded");
                           else
                               Log.Logger.Warning($"There was a problem downloading {image.Filename}");
                       }
                   }
                   catch (Exception ex)
                   {
                       Log.Error(ex, $"Download {fileName}");
                   }
               });
            }
            catch (Exception ex)
            {
                Log.Error(ex, image.BaseUrl);
            }
            return task;
        }

    }
}
