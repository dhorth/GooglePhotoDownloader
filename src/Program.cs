using GoogleDownloader.Irc;
using Horth.Shared.Infrastructure.Console;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Refit;
using GoogleDownloader.Services;

namespace GoogleDownloader
{
    class Program
    {
        public static readonly string AppName = "Horth.GoogleDownloader";

        /// <summary>
        /// Using some helper fascade classes I'm going to setup a simple console 
        /// application Main function
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task<int> Main(string[] args)
        {

            //Get the configuration and starts the logging
            var config = ConsoleHelper.InitializeConsole(AppName);

            //setup our DI, the base class will add our configuration, service registry etc
            var ret = await ConsoleHelper.ConfigureServices<Client>(config, (hostContext, services) =>
            {
                //Add all the services my application will need
                services.AddSingleton<GooglePhotoService>();
                services.AddSingleton<AuthenticationService>();
                services.AddSingleton<GoogleAuthorizationManager>();
                services.AddTransient<AuthenticatedHttpClientHandler>();

                //use refit to turn REST api's into callable methods
                services.AddRefitClient<IGooglePhotosApi>()
                    .ConfigureHttpClient((sp, c) => c.BaseAddress = new Uri("https://photoslibrary.googleapis.com/"))
                    //Add in the message handler which with inject our access token into every http call
                    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();
            }, args);
            return ret;
        }

    }
}
