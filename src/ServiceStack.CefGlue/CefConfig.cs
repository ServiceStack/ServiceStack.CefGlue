using System;
using System.IO;
using System.Reflection;
using Xilium.CefGlue;

namespace ServiceStack.CefGlue
{
    public class CefConfig
    {
        public CefConfig(bool debug=false) : this(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), debug) {}
        
        public CefConfig(string cefPath, bool debug=false)
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
                LogFile = debug ? "cef.log" : null,
                MultiThreadedMessageLoop = CefRuntime.Platform == CefRuntimePlatform.Windows,
                LogSeverity = CefLogSeverity.Info,
                ResourcesDirPath = cefPath,
                BrowserSubprocessPath = processPath,
                RemoteDebuggingPort = debug ? 20480 : 0,
                NoSandbox = false,
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
            EnableNavigationKeys = true;
            EnableReload = debug;
            Debug = true;
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
        public bool EnableNavigationKeys { get; set; } // ALT+Left + ALT+Right
        public bool EnableReload { get; set; }     // F5 to refresh
        public bool Debug { get; set; }

        public bool DevTools
        {
            get => CefSettings.RemoteDebuggingPort != 0;
            set => CefSettings.RemoteDebuggingPort = value ? 20480 : 0;
        }

        public bool Verbose
        {
            get => CefSettings.LogSeverity == CefLogSeverity.Verbose;
            set => CefSettings.LogSeverity = value ? CefLogSeverity.Verbose : CefLogSeverity.Info;
        }
    }
}