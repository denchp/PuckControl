using KwikHands.Domain;
using KwikHands.Domain.EventArg;
using KwikHands.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KwikHands
{
    /// <summary>
    /// Interaction logic for Debug.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private int _fps = 0;
        private Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        private bool _liveView = true;
        private KwikEngine _engine;
        private bool _mouseControl = false;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public DebugWindow(KwikEngine engine)
        {
            InitializeComponent(); 
            this.btnToggleCameraView.Click += ToggleCameraView;
            this.btnToggleLiveImage.Click += ToggleLiveImage;
            this.btnToggleTracking.Click += btnToggleTracking_Click;
            this.btnToggleBoxes.Click += btnToggleBoxes_Click;
            this.btnToggleMousecontrol.Click += btnToggleMousecontrol_Click;
            _engine = engine;
            _engine.ObjectMotion += _engine_ObjectMotion;
            _flags.Add("cameraViewVisible", false);
            var fpsTimer = new System.Windows.Threading.DispatcherTimer();

            pnlCameraView.Visibility = System.Windows.Visibility.Collapsed;
            fpsTimer.Tick += (s, e) => { this.txtFPS.Text = _fps.ToString(); _fps = 0; };
            fpsTimer.Interval = new TimeSpan(0, 0, 1);
            fpsTimer.Start();

            this.Closing += DebugWindow_Closing;
            this.imgCameraView.MouseMove += imgCameraView_MouseMove;
        }

        void imgCameraView_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseControl)
                return;

            var mousePosition = e.GetPosition(this.imgCameraView);
            double xMax = this.imgCameraView.Width;
            double yMax = this.imgCameraView.Height;

            mousePosition.X = mousePosition.X / xMax * 100;
            mousePosition.Y = mousePosition.Y / yMax * 100;

            mousePosition.X -= 50;
            mousePosition.Y -= 50;

            mousePosition.X *= 2;
            mousePosition.Y *= 2;


            _engine.ForceTrackingUpdate((int)mousePosition.X, (int)mousePosition.Y);
        }

        void btnToggleMousecontrol_Click(object sender, RoutedEventArgs e)
        {
            _mouseControl = !_mouseControl;
        }

        void _engine_ObjectMotion(object sender, ObjectEventArgs e)
        {
            if (e.ObjType == ObjectType.Puck)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    txtLocation.Text = e.Obj.Position.X + ", " + e.Obj.Position.Y;
                }));
            }
        }

        void DebugWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.NewTrackingImage -= _game_NewCameraImage;
            _engine.NewCameraImage -= _game_NewCameraImage;
        }

        private void btnToggleBoxes_Click(object sender, RoutedEventArgs e)
        {
            _engine.ToggleBoxes();
        }

        void btnToggleTracking_Click(object sender, RoutedEventArgs e)
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
                    catch
                    { // hide all errors as updating the image is not-critical.
                    }
                    finally
                    {
                        e.Image.Dispose();
                        DeleteObject(hBitmap);
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
