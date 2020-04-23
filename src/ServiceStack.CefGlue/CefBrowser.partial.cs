namespace Xilium.CefGlue
{
    public partial class CefBrowser
    {
        public void SendProcessMessage(CefProcessId targetProcess, CefProcessMessage message)
        {
            this.GetFocusedFrame().SendProcessMessage(targetProcess, message);
        }
        
    }
}