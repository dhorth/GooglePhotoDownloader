using Serilog;
using System.Threading.Tasks;

namespace GoogleDownloader.Irc
{
    /// <summary>
    /// Simple interace for the Console Appliocation
    /// <see cref="https://dhorth.medium.com/true-dependency-injection-for-net5-console-applications-e16eab9ae2d4"/>
    /// </summary>
    public interface IConsoleApplication
    {
        Task<int> Run(string[] args);
    }

    /// <summary>
    /// Not truely required, but its nice to be able to do some base class 
    /// work here, that way the higher level implemenation can focus on the 
    /// business logic
    /// </summary>
    public abstract class ConsoleApplication : IConsoleApplication
    {
        protected ConsoleApplication()
        {
            Log.Logger.Debug("ConsoleApplication created");
        }

        /// <summary>
        /// Execute the business logic in this override
        /// </summary>
        /// <returns></returns>
        public abstract Task<int> Run(string[] args);


    }
}
