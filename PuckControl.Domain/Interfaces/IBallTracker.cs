using PuckControl.Domain.EventArg;
using System;
using System.Drawing;

namespace PuckControl.Domain.Interfaces
{
    public interface IBallTracker : ISettingsModule
    {
        event EventHandler<BallUpdateEventArgs> BallUpdate;

        Bitmap CameraImage { get; }
        Bitmap TrackingImage { get; }
        void Dispose();
        event EventHandler NewCameraImage;
        void StartTracking();
        void StopTracking();
        bool DrawBoxes { get; set; }

    }
}
