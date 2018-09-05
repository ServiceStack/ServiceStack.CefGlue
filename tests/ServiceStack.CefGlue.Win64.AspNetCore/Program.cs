using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ServiceStack.CefGlue.Win64.AspNetCore
{
    class Program
    {
        static int Main(string[] args)
        {
            var startUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5000/";

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(startUrl)
                .Build();

            host.StartAsync();

            var config = new CefConfig
            {
                Args = args,
                StartUrl = startUrl,
            };
            
#if DEBUG
            config.HideConsoleWindow = true;
            config.CefSettings.LogSeverity = Xilium.CefGlue.CefLogSeverity.Verbose;
#endif

            return CefPlatformWindows.Start(config);
        }
    }
}
