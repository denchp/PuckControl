using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace KwikHands.Domain.EventArg
{
    public class ImageEventArgs : EventArgs
    {
        public Bitmap Image { get; set; }
    }
}
