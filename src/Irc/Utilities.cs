using System;
using System.Text;
using System.Security.Cryptography;

namespace GoogleDownloader.Irc
{
    internal class Utilities
    {
        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string GenerateRandomBase64url(int length)
        {
            var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlEncodeNoPadding(bytes);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string, which is assumed to be ASCII.
        /// </summary>
        public static string Sha256Base64(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            using (var sha256 = SHA256.Create())
            {
                var data= sha256.ComputeHash(bytes);
                return Base64UrlEncodeNoPadding(data);
            }
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static string Base64UrlEncodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
    }
}
