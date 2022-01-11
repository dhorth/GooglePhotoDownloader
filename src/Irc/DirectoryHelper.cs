using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDownloader.Irc
{
    /// <summary>
    /// Simple helper to make sure that the desired
    /// path is an actual directory
    /// </summary>
    public static class DirectoryHelper
    {
        public static string CreateDirectory(params string [] args)
        {
            var path="";
            
            foreach (var arg in args)  
                path=Path.Combine(path, arg);

            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);    
            return path;
        }
    }
}
