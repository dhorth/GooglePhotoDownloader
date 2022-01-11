using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace GoogleDownloader
{
    /// <summary>
    /// I use this class a lot in my applications
    /// I just included it here to use as an example
    /// of the second level injection
    /// </summary>
    public class AppSettings
    {
        private readonly IConfiguration _configuration;


        public AppSettings(IConfiguration c)
        {
            _configuration = c;
        }

        public string GoogleUser => GetValue("Google:User", "<!-- user -->");
        public string GoogleClientID => GetValue("Google:ClientID", "<-- Client ID -->");
        public string GoogleClientSecret => GetValue("Google:ClientSecret", "<!-- Secret -->");

        /// <summary>
        /// Make sure this redirect uri matches the one in use google dev console
        /// </summary>
        public string GoogleRedirectUri => GetValue("Google:RedirectUri", $"http://localhost:5007/Authentication/Callback");



        #region helpders
        protected string GetValue(string name, string defaultValue = "")
        {
            return _configuration.GetValue(name, defaultValue);
        }
        protected T GetValue<T>(string name, T defaultValue)
        {
            return _configuration.GetValue(name, defaultValue);
        }
        protected T GetEnumValue<T>(string name, string defaultValue)
        {
            var strValue = _configuration.GetValue(name, defaultValue);
            var ret = (T)Enum.Parse(typeof(T), strValue);
            return ret;
        }
        #endregion
    }

}
