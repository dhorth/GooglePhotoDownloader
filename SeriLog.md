# Super Simple Serilog Example

Here is a simple speed guide to implementing Serilog advanced tools into your application.  The goal of this document is not to provide a deep dive into Serilog, it is meant to provide a quick easy to understand guide to get you up and running with Serilog. For a more detailed description of Serilog I recommend the serilog github repository https://github.com/serilog. Serilog is a great tool and works well with all types of applications, Web, Console, Desktop, etc



## Required packages

This is the list of packages I use in my application, but again refer to Serilog they have a wide variety of sinks available for all kinds of different uses.  As always check for the latest versions

```xml
<ItemGroup>
    <PackageReference Include="Serilog.Enrichers.Environment" Version="2.2.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="3.3.0" />
    <PackageReference Include="Serilog.Sinks.Debug" Version="2.0.0" />
    <PackageReference Include="Serilog.Sinks.Http" Version="7.2.0" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="5.1.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="4.0.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="4.1.0" />
</ItemGroup>
```





## Load Serilog

This is where you load the logger, here I show it here in a static function called InitializeConsole, but it could just as easy be the first few lines of you program.  

```c#
public static IConfiguration InitializeConsole(string appName)
{
    var config = ConfigurationHelper.GetConfiguration();
    Log.Logger = LoggingHelper.CreateSerilogLogger(config, appName);
    Log.Logger.Information($"Starting {appName} application");
    return config;
}
```



- Step one is to get the IConfiguration, in the  above is the snippet I load the available appsettings into my IConfiguration.  Notice that the only required file is the appsettings.json, the rest are optional.  

```c#
public static IConfiguration GetConfiguration()
{
    var env = "Production";
    #if DEBUG
        env = "Development";
    #endif

        if (File.Exists("appsettings.secrets.json"))
            Log.Logger.Debug("Using appsettings.secrets.json");

    if (File.Exists("appsettings.Shared.json"))
        Log.Logger.Debug("Using appsettings.shared.json");

    if (File.Exists($"appsettings.{env}.json"))
        Log.Logger.Debug($"Using appsettings.{ env}.json");

    if (File.Exists($"appsettings.Shared.{env}.json"))
        Log.Logger.Debug($"Using appsettings.Shared.{ env}.json");

    var builder = new ConfigurationBuilder()
        //.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Shared.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.Shared.{env}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}

```





- Step Two is loading Serilog using our configuration file.  

  ```c#
  public static ILogger CreateSerilogLogger(IConfiguration configuration, string appName)
  {
      var logger = new LoggerConfiguration()
          .Enrich.WithProperty("ApplicationContext", appName)
          .Enrich.FromLogContext()
          .ReadFrom.Configuration(configuration);
  
      return logger.CreateLogger();
  }
  ```

  

  Notice the I have included two Enrich statements in by base load function.  Enrich.WithProperty - Used as an example but in this case I was every log line to have a available property call ApplicationContext, which I set to the running applications name.  For me this is valuable because I aggregate events from a number of different application into my self hosted Seq site.  The second Enrich.FromLogContext allows me to dynamically add additional properties to the log event at the time of logging.  For example

  ```C#
  using (LogContext.PushProperty("A", 1))
  {
      log.Information("Carries property A = 1");
  
      using (LogContext.PushProperty("A", 2))
      using (LogContext.PushProperty("B", 1))
      {
          log.Information("Carries A = 2 and B = 1");
      }
  
      log.Information("Carries property A = 1, again");
  }
  ```

  





## Serilog Configuration File

This is a snippet out of the appsettings.json, In practical use I comment out the  WriteTo's that do not make sense in my application.  For instance a console app, I would only keep Console and maybe File depending on how the application is used.  If I run the console application in an unattended mode then I would add in either the file or seq sinks.  Bottom line is I prefer to setup and configure my logger in the appsettings, rather than the direct code base.  It provides me with the flexibility and extensibility that I am looking for in a log tool.

```json
    "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Debug", "Serilog.Sinks.File", "Serilog.Sinks.RollingFile" ],
    "MinimumLevel": "Verbose",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme::Literate, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "Debug",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme::Literate, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "..\\logs\\Horth.GoogleDownload.log",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}",
          "rollingInterval": "Day"
        }
      },
      {
        "Name": "RollingFile",
        "Args": {
          "pathFormat": "..\\logs\\Horth.GoogleDownload -{Date}.json",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
          "textFormatter": "JsonFormatter",
          "fileSizeLimitBytes": 2147483648,
          "retainedFileCountLimit": 5
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId", "WithExceptionDetails" ]
  }

```



### Console

https://github.com/serilog/serilog-sinks-console

Write output to the console.  Color formatting is applied by adding the theme 

​        "theme": "Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme::Literate, Serilog.Sinks.Console"



### Visual Studio Output window

https://github.com/serilog/serilog-sinks-debug

Output log to the visual studio output window



### Text File Log

https://github.com/serilog/serilog-sinks-file

Write to a standard text file. In this example  I am using a output template to set the format of the line written to the log file. For more details on setting the output format see https://github.com/serilog/serilog-expressions



### Rolling Json File

https://github.com/serilog/serilog-sinks-rollingfile

Write to a rolling date stamped file, I formatting this as json for this example, but it would be just as easy to roll a standard text file.



### Seq

https://github.com/datalust/serilog-sinks-seq



See datalush Seq for more information on the host

https://datalust.co/seq

Can't recommend this tool enough





## Implementation

```c#
Log.Logger.Information("All done!  Press any key to exit");
```

Super easy to log, Log is the static entry point for logger, and is guaranteed not to throw an exception. 



Good luck and for a real world example(s) take a look at my github projects

https://github.com/dhorth/DiConsoleApp or 

https://github.com/dhorth/GooglePhotoDownloader