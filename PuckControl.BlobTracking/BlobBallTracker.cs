using System;
using System.Linq;

[assembly: CLSCompliant(true)]
namespace PuckControl.Tracking
{
    using AForge.Imaging;
    using AForge.Imaging.Filters;
    using AForge.Video;
    using AForge.Video.DirectShow;
    using PuckControl.Domain.Entities;
    using PuckControl.Domain.EventArg;
    using PuckControl.Domain.Interfaces;
    using System.Collections.Generic;
    using System.Drawing;

    public class BlobBallTracker : IDisposable, IBallTracker, ISettingsModule
    {
        const string MODULE_NAME = "Tracking";
        public event EventHandler<BallUpdateEventArgs> BallUpdate;
        public event EventHandler NewCameraImage;
        public event EventHandler LostBall;

        public Bitmap CameraImage { get { return (Bitmap)_colorImage.Clone(); } }
        public Bitmap TrackingImage { get { return (Bitmap)_trackingImage.Clone(); } }
        public bool DrawBoxes { get; set; }
        public bool DrawVectors { get; set; }

        private Bitmap _colorImage;
        private Bitmap _trackingImage;
        private VideoCaptureDevice _capture;
        private HSLFiltering _hslFilter;
        private BlobsFiltering _blobFilter;
        private Rectangle[] _rectangles;
        private BlobCounter _blobCounter;
        private bool _disposed = false;
        private BallUpdateEventArgs _outlier;
        private BallUpdateEventArgs _lastUpdate;
        private Point _blobCenter;
        private bool lastFrameProcessed = true;
        private IDataService _dataService;
        private HashSet<Setting> _settings;
        private int _droppedUpdates;

        const Int32 OUTLIER_LENGTH = 50;

        public BlobBallTracker(IDataService dataService)
        {
            DrawBoxes = true;
            DrawVectors = true;
            _dataService = dataService;

            _settings = new HashSet<Setting>(_dataService.SettingRespository.Find(x => x.Module == "Tracking"));

            LoadCameraSettings();
            LoadFilterSettings();
            SaveSettings();
        }

        public void StartTracking()
        {
            _capture.Start();
            _capture.NewFrame -= _capture_NewFrame;
            _capture.NewFrame += _capture_NewFrame;
        }

        public void StopTracking()
        {
            _capture.NewFrame -= _capture_NewFrame;
            _capture.SignalToStop();
        }

        public bool BallTracking(System.Drawing.Image newFrame)
        {
            if (newFrame == null)
                return false;

            Bitmap cleanImage = (Bitmap)newFrame.Clone();
            Bitmap trackingImage = (Bitmap)newFrame.Clone();

            _hslFilter.ApplyInPlace(trackingImage);
            //_jitterFilter.ApplyInPlace(trackingImage);
            //_contrastFilter.ApplyInPlace(trackingImage);
            _blobFilter.ApplyInPlace(trackingImage);

            _blobCounter.ProcessImage(trackingImage);

            _rectangles = _blobCounter.GetObjectsRectangles();

            if (DrawBoxes)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                using (Graphics t = Graphics.FromImage(trackingImage))
                using (Graphics c = Graphics.FromImage(cleanImage))
                {

                    foreach (Rectangle rect in _rectangles)
                    {
                        t.DrawRectangle(pen, rect);
                        c.DrawRectangle(pen, rect);
                    }
                }
            }

            _colorImage = cleanImage;
            _trackingImage = trackingImage;

            if (NewCameraImage != null)
                NewCameraImage(this, new EventArgs());

            return true;
        }

        private Rectangle FindBall()
        {
            Rectangle ballRectangle = new Rectangle();
            Rectangle secondClosest = new Rectangle();

            foreach (var rect in _rectangles)
            {
                // first find the rectangle closest to the camera
                if (rect.Location.Y > ballRectangle.Location.Y)
                {
                    secondClosest = ballRectangle;
                    ballRectangle = rect;
                }
            }

            // if the 'ball' is far enough in front of the other rectangles, we assume we've got it and return that one.
            if (ballRectangle.Y - secondClosest.Y > 0)
                return ballRectangle;

            // if it isn't 'THAT' far in front of the other options (presumably the player's red shoes)
            // then we'll check it against the _travelVector +/- 10? degrees


            return ballRectangle;
        }

        private void LoadFilterSettings()
        {
            if (_settings.Count(x => x.Section == "Tracking Color") == 0)
            {
                _settings.Add(new Setting() { Module = MODULE_NAME, Section = "Tracking Color", Key = "Minimum Hue", Options = new List<SettingOption>() { new SettingOption() { Value = "345", IsSelected = true } } });
                _settings.Add(new Setting() { Module = MODULE_NAME, Section = "Tracking Color", Key = "Maximum Hue", Options = new List<SettingOption>() { new SettingOption() { Value = "30", IsSelected = true } } });
                _settings.Add(new Setting() { Module = MODULE_NAME, Section = "Tracking Color", Key = "Minimum Saturation", Options = new List<SettingOption>() { new SettingOption() { Value = ".3", IsSelected = true } } });
                _settings.Add(new Setting() { Module = MODULE_NAME, Section = "Tracking Color", Key = "Maximum Saturation", Options = new List<SettingOption>() { new SettingOption() { Value = ".8", IsSelected = true } } });
                _settings.Add(new Setting() { Module = MODULE_NAME, Section = "Tracking Color", Key = "Minimum Luminance", Options = new List<SettingOption>() { new SettingOption() { Value = ".3", IsSelected = true } } });
                _settings.Add(new Setting() { Module = MODULE_NAME, Section = "Tracking Color", Key = "Maximum Luminance", Options = new List<SettingOption>() { new SettingOption() { Value = ".85", IsSelected = true } } });

                _settings.Add(new Setting() { Module = MODULE_NAME, Section = "Tracking Color", Key = "Minimum Object Size", Options = new List<SettingOption>() { new SettingOption() { Value = "12", IsSelected = true } } });
                _dataService.SettingRespository.Save(_settings);
            }


            _hslFilter = new HSLFiltering();
            int minHue, maxHue;
            float minSat, maxSat, minLum, maxLum;

            Int32.TryParse(_settings.First(s => s.Key == "Minimum Hue").Options.First(x => x.IsSelected).Value, out minHue);
            Int32.TryParse(_settings.First(s => s.Key == "Maximum Hue").Options.First(x => x.IsSelected).Value, out maxHue);
            float.TryParse(_settings.First(s => s.Key == "Minimum Saturation").Options.First(x => x.IsSelected).Value, out minSat);
            float.TryParse(_settings.First(s => s.Key == "Maximum Saturation").Options.First(x => x.IsSelected).Value, out maxSat);
            float.TryParse(_settings.First(s => s.Key == "Minimum Luminance").Options.First(x => x.IsSelected).Value, out minLum);
            float.TryParse(_settings.First(s => s.Key == "Maximum Luminance").Options.First(x => x.IsSelected).Value, out maxLum);

            _hslFilter.Hue = new AForge.IntRange(minHue, maxHue);
            _hslFilter.Saturation = new AForge.Range(minSat, maxSat);
            _hslFilter.Luminance = new AForge.Range(minLum, maxLum);

            _blobFilter = new BlobsFiltering();

            int minimumSize;
            Int32.TryParse(_settings.First(s => s.Key == "Minimum Object Size").Options.First(x => x.IsSelected).Value, out minimumSize);

            _blobFilter.CoupledSizeFiltering = true;
            _blobFilter.MinWidth = minimumSize;
            _blobFilter.MinHeight = minimumSize;

            _blobCounter = new BlobCounter();
        }

        private void LoadCameraSettings()
        {
            if (_capture != null)
                _capture.Stop(); // make sure we aren't capping video, otherwise it can cause the program to hang around in memory after exiting.

            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
                throw new NotSupportedException("No video devices found.");

            string camera = String.Empty;
            Setting cameraSetting = null;

            if ((cameraSetting = _settings.FirstOrDefault(x => x.Key == "Camera")) == null)
            {
                // no camera settings found, load the camera names and set the default to be the last in the list.
                cameraSetting = new Setting();
                cameraSetting.Key = "Camera";
                cameraSetting.Section = "General";
                cameraSetting.Module = MODULE_NAME;

                foreach (FilterInfo cam in videoDevices)
                    cameraSetting.Options.Add(new SettingOption() { Name = cam.Name, Value = cam.MonikerString });

                cameraSetting.Options.First(x => x.Name == videoDevices[videoDevices.Count - 1].Name).IsSelected = true;
                _settings.Add(cameraSetting);
            }

            camera = cameraSetting.Options.First(x => x.IsSelected).Value;

            _capture = new VideoCaptureDevice(camera);

            Setting imageSetting = null;
            VideoCapabilities capabilities = null;

            if ((imageSetting = _settings.FirstOrDefault(x => x.Key == "Tracking Image Size")) == null)
            {
                imageSetting = new Setting();
                imageSetting.Key = "Tracking Image Size";
                imageSetting.Module = MODULE_NAME;
                imageSetting.Section = "General";
                _settings.Add(imageSetting);

                foreach (var cap in _capture.VideoCapabilities)
                {
                    imageSetting.Options.Add(new SettingOption() { Name = cap.FrameSize.ToString(), Value = cap.GetHashCode().ToString() });

                    if (cap.FrameSize.Height <= 480 && cap.FrameSize.Width <= 640 && capabilities == null)
                    {
                        imageSetting.Options.FirstOrDefault(x => x.Name == cap.FrameSize.ToString()).IsSelected = true;
                    }
                }
            }

            if (imageSetting.Options.Any(x => x.IsSelected))
                capabilities = _capture.VideoCapabilities.FirstOrDefault(x => x.GetHashCode().ToString() == imageSetting.Options.First(o => o.IsSelected).Value);

            if (capabilities == null)
            {
                // if the capabilities are null here, then the selected image options aren't valid for the camera, so we need to clear the selected option.
                imageSetting.Options.ForEach(x => x.IsSelected = false);

                _dataService.OptionRepository.Delete(imageSetting.Options);
                imageSetting.Options.Clear();

                foreach (var cap in _capture.VideoCapabilities)
                {
                    imageSetting.Options.Add(new SettingOption() { Name = cap.FrameSize.ToString(), Value = cap.GetHashCode().ToString() });

                    if (cap.FrameSize.Height <= 480 && cap.FrameSize.Width <= 640 && capabilities == null)
                    {
                        capabilities = cap;
                        if (imageSetting.Options.Any(x => x.IsSelected))
                            imageSetting.Options.First(x => x.Value == cap.FrameSize.ToString()).IsSelected = true;
                    }
                }
            }

            _capture.VideoResolution = capabilities;
        }

        private void SaveSettings()
        {
            _dataService.SettingRespository.Save(_settings);
        }

        private void _capture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!lastFrameProcessed)
                return;

            if (!BallTracking(eventArgs.Frame))
                return;

            lastFrameProcessed = false;

            if (_rectangles.Length > 0 && BallUpdate != null)
            {
                Rectangle ballRectangle;
                var args = new BallUpdateEventArgs();

                if (_rectangles.Length > 1)
                    ballRectangle = FindBall();
                else
                    ballRectangle = _rectangles[0];

                _blobCenter.X = ballRectangle.X + (ballRectangle.Width / 2);
                _blobCenter.Y = ballRectangle.Y + (ballRectangle.Height / 2);

                int halfWidth = (int)(.5 * _trackingImage.Width);
                int halfHeight = (int)(.5 * _trackingImage.Height);

                int Xpct = (int)(100 * (_blobCenter.X - halfWidth) / halfWidth);
                int Ypct = (int)(100 * (_blobCenter.Y - halfHeight) / halfHeight);

                args.PositionVector = new System.Windows.Media.Media3D.Vector3D() { X = Xpct, Y = Ypct, Z = 0 };

                if (Xpct < 99 && Ypct < 99 && Xpct > -99 && Ypct > -99)
                {// 100% in any direction and we're assuming we've lost the ball

                    if (_lastUpdate != null)
                    {
                        if (Math.Abs((_lastUpdate.PositionVector - args.PositionVector).Length) < OUTLIER_LENGTH)
                        {
                            _droppedUpdates = 0;
                            _lastUpdate = args;
                            BallUpdate(this, args);
                            _outlier = null;
                        }
                        else
                        {
                            if (_outlier == null)
                            {
                                _outlier = args;
                            }
                            else
                            {
                                if (_outlier != null && (args.PositionVector - _outlier.PositionVector).Length < OUTLIER_LENGTH)
                                {
                                    _droppedUpdates = 0;
                                    _lastUpdate = args;
                                    BallUpdate(this, args);
                                    _outlier = null;
                                }
                                else
                                {
                                    _outlier = args;
                                }
                            }
                        }
                    }
                    else
                    {
                        _droppedUpdates = 0;
                        _lastUpdate = args;
                        BallUpdate(this, args);
                    }
                }
            }

            if (_rectangles.Length == 0)
                _droppedUpdates++;

            if (LostBall != null && _droppedUpdates > 10)
                LostBall(this, new EventArgs());

            lastFrameProcessed = true;
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
                _trackingImage.Dispose();
                _colorImage.Dispose();

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion

        #region ISettingsModule
        public IEnumerable<Setting> Settings
        {
            get
            {
                return _settings;
            }
        }

        public void ReloadSettings()
        {
            var moduleSettings = _dataService.SettingRespository.All.Where(x => x.Module == MODULE_NAME).ToList(); ;

            _settings = new HashSet<Setting>(moduleSettings);

            LoadCameraSettings();
            LoadFilterSettings();
        }
        #endregion
    }
}
