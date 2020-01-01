using System;
using System.IO;
using System.Runtime.InteropServices;
using Xilium.CefGlue;

namespace ServiceStack.CefGlue
{
    public sealed class WebBrowser
    {
        private IntPtr windowHandle;

        private bool created;

        public CefClient Client { get; private set; }
        public CefBrowser CefBrowser { get; private set; }

        public CefGlueBrowser Host { get; }
        public CefConfig Config => Host?.Config;
        public CefApp App => Host.App;

        public WebBrowser(CefGlueBrowser host)
        {
            this.Host = host;
        }

        public string StartUrl { get; set; }

        public void Create(CefWindowInfo windowInfo)
        {
            if (Client == null)
            {
                Client = new WebClient(this);
            }

            CefBrowserHost.CreateBrowser(windowInfo, Client, Host.Config.CefBrowserSettings, StartUrl);
        }

        public event EventHandler Created;

        internal void OnCreated(CefBrowser browser)
        {
            created = true;
            this.CefBrowser = browser;
            
            var handler = Created;
            handler?.Invoke(this, EventArgs.Empty);
            
            Config?.OnCreated?.Invoke(this);
        }

        public void Close()
        {
            if (Host.WebBrowser != null)
            {
                var browserHost = Host.CefBrowser.GetHost();
                browserHost.CloseBrowser(true);
                browserHost.Dispose();
                Host.DisposeCefBrowser();
            }
        }

        public event EventHandler<TitleChangedEventArgs> TitleChanged;

        internal void OnTitleChanged(string title)
        {
            var handler = TitleChanged;
            handler?.Invoke(this, new TitleChangedEventArgs(title));

            Config?.OnTitleChanged?.Invoke(this, title);
        }

        public event EventHandler<AddressChangedEventArgs> AddressChanged;

        internal void OnAddressChanged(string address)
        {
            var handler = AddressChanged;
            handler?.Invoke(this, new AddressChangedEventArgs(address));

            Config?.OnAddressChanged?.Invoke(this, address);
        }

        public event EventHandler<TargetUrlChangedEventArgs> TargetUrlChanged;

        internal void OnTargetUrlChanged(string targetUrl)
        {
            var handler = TargetUrlChanged;
            handler?.Invoke(this, new TargetUrlChangedEventArgs(targetUrl));

            Config?.OnTargetUrlChanged?.Invoke(this, targetUrl);
        }

        public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;

        internal void OnLoadingStateChanged(bool isLoading, bool canGoBack, bool canGoForward)
        {
            var handler = LoadingStateChanged;
            var args = new LoadingStateChangedEventArgs(isLoading, canGoBack, canGoForward);
            handler?.Invoke(this, args);

            Config?.OnLoadingStateChanged?.Invoke(this, args);
        }

        public void Log(string message)
        {
            if (!Config.Verbose)
                return;
            
            Console.WriteLine(message);

            Config?.OnLog?.Invoke(this, message);
        }
    }

    public sealed class WebClient : CefClient
    {
        internal static bool DumpProcessMessages { get; set; }

        private readonly WebBrowser core;
        private readonly WebLifeSpanHandler lifeSpanHandler;
        private readonly WebDisplayHandler displayHandler;
        private readonly WebLoadHandler loadHandler;
        private readonly WebKeyboardHandler keyboardHandler;

        public WebClient(WebBrowser core)
        {
            this.core = core;
            lifeSpanHandler = new WebLifeSpanHandler(this.core);
            displayHandler = new WebDisplayHandler(this.core);
            loadHandler = new WebLoadHandler(this.core);
            keyboardHandler = new WebKeyboardHandler(this.core);
        }

        protected override CefLifeSpanHandler GetLifeSpanHandler() => lifeSpanHandler;
        protected override CefDisplayHandler GetDisplayHandler() => displayHandler;
        protected override CefLoadHandler GetLoadHandler() => loadHandler;
        protected override CefKeyboardHandler GetKeyboardHandler() => keyboardHandler;

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            var result = core.Config.OnProcessMessageReceived?.Invoke(this, browser, sourceProcess, message);
            if (result != null)
                return result.Value;
            
            if (DumpProcessMessages)
            {
                Console.WriteLine("Client::OnProcessMessageReceived: SourceProcess={0}", sourceProcess);
                Console.WriteLine("Message Name={0} IsValid={1} IsReadOnly={2}", message.Name, message.IsValid, message.IsReadOnly);
                var arguments = message.Arguments;
                for (var i = 0; i < arguments.Count; i++)
                {
                    var type = arguments.GetValueType(i);
                    object value;
                    switch (type)
                    {
                        case CefValueType.Null: value = null; break;
                        case CefValueType.String: value = arguments.GetString(i); break;
                        case CefValueType.Int: value = arguments.GetInt(i); break;
                        case CefValueType.Double: value = arguments.GetDouble(i); break;
                        case CefValueType.Bool: value = arguments.GetBool(i); break;
                        default: value = null; break;
                    }

                    Console.WriteLine("  [{0}] ({1}) = {2}", i, type, value);
                }
            }

            //var handled = BrowserMessageRouter.OnProcessMessageReceived(browser, sourceProcess, message);
            //if (handled) return true;

            if (message.Name == "myMessage2" || message.Name == "myMessage3") return true;

            return false;
        }        
    }

    public sealed class TitleChangedEventArgs : EventArgs
    {
        public TitleChangedEventArgs(string title) => Title = title;
        public string Title { get; }
    }
    public class AddressChangedEventArgs : EventArgs
    {
        public AddressChangedEventArgs(string address)
        {
            Address = address;
        }
        public string Address { get; private set; }
    }
    public sealed class TargetUrlChangedEventArgs : EventArgs
    {
        public TargetUrlChangedEventArgs(string targetUrl) => TargetUrl = targetUrl;
        public string TargetUrl { get; }
    }
    public sealed class LoadingStateChangedEventArgs : EventArgs
    {
        public LoadingStateChangedEventArgs(bool isLoading, bool canGoBack, bool canGoForward)
        {
            Loading = isLoading;
            CanGoBack = canGoBack;
            CanGoForward = canGoForward;
        }
        public bool Loading { get; }
        public bool CanGoBack { get; }
        public bool CanGoForward { get; }
    }

    internal sealed class WebLifeSpanHandler : CefLifeSpanHandler
    {
        private readonly WebBrowser core;

        public WebLifeSpanHandler(WebBrowser core)
        {
            this.core = core;
        }

        protected override void OnAfterCreated(CefBrowser browser)
        {
            base.OnAfterCreated(browser);

            core.OnCreated(browser);
            
            core.Config.OnLifeSpanAfterCreated?.Invoke(browser);
        }

        protected override bool DoClose(CefBrowser browser)
        {
            core.Config.OnLifeSpanDoClose?.Invoke(browser);

            // TODO: dispose core
            return false;
        }

        protected override void OnBeforeClose(CefBrowser browser)
        {
            core.Config.OnLifeSpanBeforeClose?.Invoke(browser);
        }
    }

    internal sealed class WebDisplayHandler : CefDisplayHandler
    {
        private readonly WebBrowser core;

        public WebDisplayHandler(WebBrowser core)
        {
            this.core = core;
        }

        protected override void OnTitleChange(CefBrowser browser, string title)
        {
            core.OnTitleChanged(title);
            
            core.Config.OnDisplayTitleChange?.Invoke(browser, title);
        }

        protected override void OnAddressChange(CefBrowser browser, CefFrame frame, string url)
        {
            if (frame.IsMain)
            {
                core.OnAddressChanged(url);
            }
            
            core.Config.OnDisplayAddressChange?.Invoke(browser,frame,url);
        }

        protected override void OnStatusMessage(CefBrowser browser, string value)
        {
            core.OnTargetUrlChanged(value);

            core.Config.OnDisplayStatusMessage?.Invoke(browser,value);
        }

        protected override bool OnTooltip(CefBrowser browser, string text)
        {
            var ret = core.Config.OnDisplayTooltip?.Invoke(browser,text);
            if (ret != null)
                return ret.Value;

            return false;
        }
    }

    internal sealed class WebLoadHandler : CefLoadHandler
    {
        private readonly WebBrowser core;

        public WebLoadHandler(WebBrowser core)
        {
            this.core = core;
        }

        protected override void OnLoadingStateChange(CefBrowser browser, bool isLoading, bool canGoBack, bool canGoForward)
        {
            core.OnLoadingStateChanged(isLoading, canGoBack, canGoForward);
        }
    }

    //https://github.com/adobe/webkit/blob/master/Source/WebCore/platform/chromium/KeyboardCodes.h
    internal static class KeyCodes
    {
        internal const int F5 = 0x74;
        internal const int F11 = 0x7A;
        internal const int F12 = 0x7B;
        internal const int Left = 0x25;
        internal const int Up = 0x26;
        internal const int Right = 0x27;
        internal const int Down = 0x28;

        internal const int R = 0x52;
    }

    internal sealed class WebKeyboardHandler : CefKeyboardHandler
    {
        private WebBrowser core;
        public WebKeyboardHandler(WebBrowser core) => this.core = core;

        private class DevToolsWebClient : CefClient {}

        protected override bool OnPreKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, IntPtr os_event, out bool isKeyboardShortcut)
        {
            var ret = core.Config.OnKeyboardPreKeyEvent?.Invoke(browser, keyEvent, os_event);
            if (ret != null)
            {
                isKeyboardShortcut = false;
                return ret.Value;
            }
            
            core.Log($"Key: {keyEvent.NativeKeyCode}, winKey: {keyEvent.WindowsKeyCode}, modifiers: {keyEvent.Modifiers}, type: {keyEvent.EventType} ");

            if (keyEvent.EventType == CefKeyEventType.RawKeyDown)
            {
                if (core.Config.DevTools && (keyEvent.WindowsKeyCode == KeyCodes.F11 || keyEvent.WindowsKeyCode == KeyCodes.F12))
                {
                    var host = core.CefBrowser.GetHost();
                    var windowInfo = CefWindowInfo.Create();
                    windowInfo.SetAsPopup(IntPtr.Zero, "DevTools");
                    host.ShowDevTools(windowInfo, new DevToolsWebClient(), new CefBrowserSettings(), new CefPoint());
                }
    
                if (core.Config.EnableNavigationKeys && keyEvent.Modifiers.HasFlag(CefEventFlags.AltDown))
                {
                    if (keyEvent.WindowsKeyCode == KeyCodes.Left && browser.CanGoBack)
                        browser.GoBack();
                    if (keyEvent.WindowsKeyCode == KeyCodes.Right && browser.CanGoForward)
                        browser.GoForward();
                }

                if ((keyEvent.WindowsKeyCode == KeyCodes.F5 ||
                     keyEvent.WindowsKeyCode == KeyCodes.R && keyEvent.Modifiers.HasFlag(CefEventFlags.ControlDown)) && 
                     core.Config.EnableReload)
                {
                    browser.Reload();
                }
            }

            return base.OnPreKeyEvent(browser, keyEvent, os_event, out isKeyboardShortcut);
        }

        protected override bool OnKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, IntPtr osEvent)
        {
            return base.OnKeyEvent(browser, keyEvent, osEvent);
        }
    }

}