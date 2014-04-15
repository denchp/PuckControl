using PuckControl.Domain.EventArg;
using System;
using System.Drawing;

namespace PuckControl.Domain.Interfaces
{
    public interface IBallTracker : ISettingsModule
    {
        /// <summary>
        /// Occurs when the position of the ball is updated.
        /// </summary>
        event EventHandler<BallUpdateEventArgs> BallUpdate;
        /// <summary>
        /// Occurs when the tracking system loses sight of the ball.
        /// </summary>
        event EventHandler LostBall;

        /// <summary>
        /// Occurs when there is a new camera image.
        /// </summary>
        event EventHandler NewCameraImage;

        /// <summary>
        /// Gets the most recent camera image.
        /// </summary>
        /// <value>
        /// The camera image.
        /// </value>
        Bitmap CameraImage { get; }
        /// <summary>
        /// Gets the most recent tracking image.
        /// </summary>
        /// <value>
        /// The tracking image.
        /// </value>
        Bitmap TrackingImage { get; }

        void Dispose();
        /// <summary>
        /// Starts the tracking.
        /// </summary>
        void StartTracking();
        /// <summary>
        /// Stops the tracking.
        /// </summary>
        void StopTracking();
        /// <summary>
        /// Gets or sets a value indicating whether to draw tracking boxes on the camera images.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [draw boxes]; otherwise, <c>false</c>.
        /// </value>
        bool DrawBoxes { get; set; }

    }
}
