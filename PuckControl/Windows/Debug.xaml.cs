using PuckControl.Domain.EventArg;
using PuckControl.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace PuckControl.Windows
{
    /// <summary>
    /// Interaction logic for Debug.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private int _fps = 0;
        private Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        private bool _liveView = true;
        private GameEngine _engine;
        private bool _mouseControl = false;

        public DebugWindow(GameEngine engine)
        {
            InitializeComponent(); 
            this.btnToggleCameraView.Click += ToggleCameraView;
            this.btnToggleLiveImage.Click += ToggleLiveImage;
            this.btnToggleTracking.Click += btnToggleTracking_Click;
            this.btnToggleBoxes.Click += btnToggleBoxes_Click;
            
            if (engine != null)
            {
                _engine = engine;
                _engine.ObjectMotion += _engine_ObjectMotion;
                _engine.NewObject += _engine_NewObject;
            }

            _flags.Add("cameraViewVisible", false);
            var fpsTimer = new System.Windows.Threading.DispatcherTimer();

            pnlCameraView.Visibility = System.Windows.Visibility.Collapsed;
            fpsTimer.Tick += (s, e) => { this.txtFPS.Text = _fps.ToString(); _fps = 0; };
            fpsTimer.Interval = new TimeSpan(0, 0, 1);
            fpsTimer.Start();

            this.Closing += DebugWindow_Closing;
            GetFileVersions();
        }

        private void GetFileVersions()
        {
            string folder = System.AppDomain.CurrentDomain.BaseDirectory;

            string[] files = Directory.GetFiles(folder, "PuckControl.*.dll");

            foreach (string file in files)
            {
                var info = FileVersionInfo.GetVersionInfo(file);
                FileVersions.Children.Add(new Label() { Content = file.Substring(file.LastIndexOf("\\") + 1) + ": " + info.FileVersion });
            }
        }

        void _engine_NewObject(object sender, ObjectEventArgs e)
        {
                this.Dispatcher.Invoke((Action)((() =>
                {
                    this.AddObject(e.Obj.Position, e.Obj.ObjectType);
                })));
        }

        internal void AddObject(Vector3D position, String name)
        {
            TextBox newItem = new TextBox();
            newItem.Text = name + "\t" + position.ToString();
            this.ObjectList.Children.Add(newItem);
        }

        private void btnToggleMousecontrol_Click(object sender, RoutedEventArgs e)
        {
            _mouseControl = !_mouseControl;
        }

        private void _engine_ObjectMotion(object sender, ObjectEventArgs e)
        {
            if (e.Obj.TrackCollisions)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    txtLocation.Text = e.Obj.Position.X + ", " + e.Obj.Position.Y;
                }));
            }
        }

        private void DebugWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.NewTrackingImage -= _game_NewCameraImage;
            _engine.NewCameraImage -= _game_NewCameraImage;
        }

        private void btnToggleBoxes_Click(object sender, RoutedEventArgs e)
        {
            _engine.ToggleBoxes();
        }

        private void btnToggleTracking_Click(object sender, RoutedEventArgs e)
        {
            if (_engine.Tracking)
                _engine.StopTracking();
            else
                _engine.StartTracking();
        }

        private void ToggleLiveImage(object sender, RoutedEventArgs e)
        {
            _liveView = !_liveView;

            if (_liveView)
            {
                _engine.NewTrackingImage -= _game_NewCameraImage;
                _engine.NewCameraImage += _game_NewCameraImage;
            }
            else
            {
                _engine.NewTrackingImage += _game_NewCameraImage;
                _engine.NewCameraImage -= _game_NewCameraImage;
            }
        }

        private void _game_NewCameraImage(object sender, ImageEventArgs e)
        {
            _fps = ++_fps;

            if (_flags["cameraViewVisible"])
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    IntPtr hBitmap = e.Image.GetHbitmap();

                    try
                    {
                        var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                                    IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                        this.imgCameraView.Source = source;
                    }
                    finally
                    {
                        e.Image.Dispose();
                        NativeMethods.DeleteObject(hBitmap);
                    }
                }));
            }
        }

        private void ToggleCameraView(object sender, RoutedEventArgs e)
        {
            _flags["cameraViewVisible"] = !_flags["cameraViewVisible"];
            this.pnlCameraView.Visibility = _flags["cameraViewVisible"] ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            if (_flags["cameraViewVisible"])
            {
                if (_liveView)
                {
                    _engine.NewTrackingImage -= _game_NewCameraImage;
                    _engine.NewCameraImage += _game_NewCameraImage;
                }
                else
                {
                    _engine.NewTrackingImage += _game_NewCameraImage;
                    _engine.NewCameraImage -= _game_NewCameraImage;
                }
            }
            else
            {
                _engine.NewTrackingImage -= _game_NewCameraImage;
                _engine.NewCameraImage -= _game_NewCameraImage;
            }
        }
    }
}
