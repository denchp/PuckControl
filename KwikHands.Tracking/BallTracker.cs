namespace KwikHands.Tracking
{
    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Structure;
    using Emgu.CV.VideoSurveillance;
    using Emgu.CV.Features2D;

    using System;
    using System.Collections.Generic;
    using System.Drawing;

    public class BallTracker : IDisposable
    {
        public event EventHandler<BlobUpdateEventArgs> BallUpdate;
        public event EventHandler NewCameraImage;

        public Bitmap CameraImage { get { return _CameraImage.ToBitmap(); } }
        public Bitmap TrackingImage { get { return _TrackingImage.ToBitmap(); } }
        public bool DrawBoxes = false;
        private Image<Hsv, byte> _CameraImage;
        private Image<Gray, byte> _TrackingImage;
        
        private const int WIDTH = 400;
        private int height = WIDTH / 4 * 3;

        private Capture _capture = new Capture();
 
        private Image<Gray, byte> _currentImage;
        private Image<Hsv, byte> _color;
        private Image<Gray, byte> _mask;

        private bool _disposed = false;
        private bool _tracking = false;

        private static BlobDetector _detector;
        private static Features2DTracker<Gray> _tracker;
        
        private BlobSeq _newBlobs = new BlobSeq();
        private BlobSeq _oldBlobs = new BlobSeq();
        private BlobSeq _prevOldBlobs = new BlobSeq();

        private Gray _threshold = new Gray(180);
        private Gray _maximum = new Gray(255);

        private Hsv RED = new Hsv(0, 100, 0);
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_SIMPLEX, 1.0, 1.0);
        Point _gravityCenter;

        public BallTracker()
        {
        }

        public void StartTracking()
        {
            _tracking = true;
            
            while (_tracking)
            {
                BallTracking();

                if (BallUpdate != null)
                {   // Fire the update event!
                    var args = new BlobUpdateEventArgs();
                    int Xpct = _gravityCenter.X * 100 / _currentImage.Width - 50;
                    Xpct *= -1;
                    int Ypct = _gravityCenter.Y * 100 / _currentImage.Height - 50;

                    args.MotionVector = new System.Windows.Media.Media3D.Vector3D() { X = Xpct, Y = Ypct, Z = 0 };
                    BallUpdate(this, args);
                }
            }
        }

        public void StopTracking()
        {
            _tracking = false;
        }

        public void BallTracking()
        {

            UpdateCurrentImage();

            MCvMoments moment = _currentImage.GetMoments(true);
            _gravityCenter = new Point((int)(moment.m10 / moment.m00), (int)(moment.m01 / moment.m00));

            
            if (DrawBoxes)
            {
                _color.Draw(new CircleF(_gravityCenter, 10), RED, 2);
                _currentImage.Draw(new CircleF(_gravityCenter, 10), _maximum, 2);
            }

            _CameraImage = _color;
            _TrackingImage = _currentImage;
            
            if (NewCameraImage != null)
                NewCameraImage(this, new EventArgs());
        }

        private void UpdateCurrentImage()
        {
            _color = _capture.QueryFrame().Convert<Hsv, byte>();
            _color._SmoothGaussian(11);
            _mask = GetMask();
            _currentImage = _color.And(_mask.Convert<Hsv, byte>())
                 .ThresholdBinary(new Hsv(0, 100, 5), new Hsv(0, 0, 255)).Convert<Gray, byte>();
        }

        private Image<Gray, byte> GetMask()
        {
            Image<Gray, byte>[] channels = _color.Split();
            Image<Gray, byte> HueMask;
            Image<Gray, byte> SatMask;
            Image<Gray, byte> ValMask;
            Image<Gray, byte> Mask;

            try
            {
                HueMask = channels[0].ThresholdBinary(new Gray(100), _maximum);
                SatMask = channels[1].ThresholdBinary(new Gray(160), _maximum);
                ValMask = channels[2].ThresholdBinary(new Gray(25), _maximum);

                Mask = SatMask.And(ValMask);
            }
            finally
            {
                //channels[2].Dispose();
                //channels[1].Dispose();
            }
            return Mask;
        }

        #region IDispose Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _capture.Dispose();
                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion
    }
}
