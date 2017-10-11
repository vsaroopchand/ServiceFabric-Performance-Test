using System;

namespace Common
{
    public class HubMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class HubErrorEventArgs : EventArgs
    {
        public Exception Error { get; set; }
    }
}
