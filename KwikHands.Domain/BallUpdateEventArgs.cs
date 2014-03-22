using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KwikHands.Domain
{
    public class BallUpdateEventArgs : EventArgs
    {
        public int BlobId { get; set; }
        public Vector3D PositionVector { get; set; }
    }
}
