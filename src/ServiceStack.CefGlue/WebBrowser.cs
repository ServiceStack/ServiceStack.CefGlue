using System;
using System.IO;
using System.Runtime.InteropServices;
using Xilium.CefGlue;

namespace ServiceStack.CefGlue
{
    public sealed class WebBrowser
    {
        private readonly object owner;
        private readonly CefBrowserSettings settings;
        private CefClient client;
        private CefBrowser browser;
        private IntPtr windowHandle;

        private bool created;

        public WebBrowser(object owner, CefBrowserSettings settings, string startUrl)
        {
            this.owner = owner;
            this.settings = settings;
            StartUrl = startUrl;
        }

        public string StartUrl { get; set; }

        public CefBrowser CefBrowser => browser;

        public void Create(CefWindowInfo windowInfo)
        {
            if (client == null)
            {
                client = new WebClient(this);
            }

            CefBrowserHost.CreateBrowser(windowInfo, client, settings, StartUrl);
        }

        public event EventHandler Created;

        internal void OnCreated(CefBrowser browser)
        {
            created = true;
            this.browser = browser;

            var handler = Created;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            if (browser != null)
            {
                var host = browser.GetHost();
                host.CloseBrowser(true);
                host.Dispose();
                browser.Dispose();
                browser = null;
            }
        }

        public event EventHandler<TitleChangedEventArgs> TitleChanged;

        internal void OnTitleChanged(string title)
        {
            var handler = TitleChanged;
            handler?.Invoke(this, new TitleChangedEventArgs(title));
        }

        public event EventHandler<AddressChangedEventArgs> AddressChanged;

        internal void OnAddressChanged(string address)
        {
            var handler = AddressChanged;
            handler?.Invoke(this, new AddressChangedEventArgs(address));
        }

        public event EventHandler<TargetUrlChangedEventArgs> TargetUrlChanged;

        internal void OnTargetUrlChanged(string targetUrl)
        {
            var handler = TargetUrlChanged;
            handler?.Invoke(this, new TargetUrlChangedEventArgs(targetUrl));
        }

        public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;

        internal void OnLoadingStateChanged(bool isLoading, bool canGoBack, bool canGoForward)
        {
            var handler = LoadingStateChanged;
            handler?.Invoke(this, new LoadingStateChangedEventArgs(isLoading, canGoBack, canGoForward));
        }
    }

    internal sealed class WebClient : CefClient
    {
        internal static bool DumpProcessMessages { get; set; }

        private readonly WebBrowser core;
        private readonly WebLifeSpanHandler lifeSpanHandler;
        private readonly WebDisplayHandler displayHandler;
        private readonly WebLoadHandler loadHandler;

        public WebClient(WebBrowser core)
        {
            this.core = core;
            lifeSpanHandler = new WebLifeSpanHandler(this.core);
            displayHandler = new WebDisplayHandler(this.core);
            loadHandler = new WebLoadHandler(this.core);
        }

        protected override CefLifeSpanHandler GetLifeSpanHandler()
        {
            return lifeSpanHandler;
        }

        protected override CefDisplayHandler GetDisplayHandler()
        {
            return displayHandler;
        }

        protected override CefLoadHandler GetLoadHandler()
        {
            return loadHandler;
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
        {
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
        }

        protected override bool DoClose(CefBrowser browser)
        {
            // TODO: dispose core
            return false;
        }

        protected override void OnBeforeClose(CefBrowser browser)
        {
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
        }

        protected override void OnAddressChange(CefBrowser browser, CefFrame frame, string url)
        {
            if (frame.IsMain)
            {
                core.OnAddressChanged(url);
            }
        }

        protected override void OnStatusMessage(CefBrowser browser, string value)
        {
            core.OnTargetUrlChanged(value);
        }

        protected override bool OnTooltip(CefBrowser browser, string text)
        {
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

}