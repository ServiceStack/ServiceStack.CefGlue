using System;
using System.IO;
using System.Reflection;
using Xilium.CefGlue;

namespace ServiceStack.CefGlue
{
    public class CefConfig
    {
        public CefConfig() : this(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)) {}
        
        public CefConfig(string cefPath)
        {
            CefPath = cefPath;
            var processPath = File.Exists(Path.Combine(cefPath, "cefclient.exe"))
                ? Path.Combine(cefPath, "cefclient.exe")
                : File.Exists(Path.Combine(cefPath, "webapp.exe"))
                    ? Path.Combine(cefPath, "webapp.exe")
                    : null;
            CefSettings = new CefSettings
            {
                LocalesDirPath = Path.Combine(cefPath, "locales"),
                Locale = "en-US",
                MultiThreadedMessageLoop = CefRuntime.Platform == CefRuntimePlatform.Windows,
                LogSeverity = CefLogSeverity.Info,
                LogFile = "cef.log",
                ResourcesDirPath = cefPath,
                //NoSandbox = true,
                BrowserSubprocessPath = processPath,
                RemoteDebuggingPort = 20480,
            };
            CefBrowserSettings = new CefBrowserSettings
            {
                DefaultEncoding = "UTF-8",
                FileAccessFromFileUrls = CefState.Enabled,
                UniversalAccessFromFileUrls = CefState.Enabled,
                WebSecurity = CefState.Enabled,
            };

            WindowTitle = "Web App";
            Icon = "favicon.ico";
            Args = new string[0];
            StartUrl = "about:blank";
            CenterToScreen = true;
            HideConsoleWindow = true;
        }

        public string WindowTitle { get; set; }
        public string Icon { get; set; }
        public string CefPath { get; set; }
        public string[] Args { get; set; }
        public CefSettings CefSettings { get; set; }
        public CefBrowserSettings CefBrowserSettings { get; set; }
        public string StartUrl { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool CenterToScreen { get; set; }
        public bool HideConsoleWindow { get; set; }

        public bool DevTools
        {
            get => CefSettings.RemoteDebuggingPort != 0;
            set => CefSettings.RemoteDebuggingPort = 0;
        }
    }
}