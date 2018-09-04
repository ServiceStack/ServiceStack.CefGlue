using System;
using Xilium.CefGlue;

namespace ServiceStack.CefGlue
{
    public abstract class CefPlatform
    {
        public static CefPlatform Instance { get; protected set; }

        public abstract CefSize GetScreenResolution();

        public abstract void HideConsoleWindow();

        public abstract void ResizeWindow(IntPtr handle, int width, int height);
    }
}