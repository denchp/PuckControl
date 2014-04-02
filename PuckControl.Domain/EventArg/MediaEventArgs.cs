using System;

namespace PuckControl.Domain.EventArg
{
    public class MediaEventArgs : EventArgs
    {
        public Uri MediaFile { get; set; }
    }
}
