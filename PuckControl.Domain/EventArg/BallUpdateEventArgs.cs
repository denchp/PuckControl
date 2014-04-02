using System;
using System.Windows.Media.Media3D;

namespace PuckControl.Domain.EventArg
{
    public class BallUpdateEventArgs : EventArgs
    {
        public int BlobId { get; set; }
        public Vector3D PositionVector { get; set; }
    }
}
