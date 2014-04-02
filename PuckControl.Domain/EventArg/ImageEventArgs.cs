using System;
using System.Drawing;

namespace PuckControl.Domain.EventArg
{
    public class ImageEventArgs : EventArgs
    {
        public Bitmap Image { get; set; }
    }
}
