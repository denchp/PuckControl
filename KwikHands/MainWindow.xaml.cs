using KwikHands.Domain;
using KwikHands.Domain.EventArg;
using KwikHands.Tracking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace KwikHands
{
    public partial class MainWindow : Window, IGameWindow
    {

        private Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        private int _fps = 0;
        private Dictionary<int, ModelVisual3D> _models = new Dictionary<int, ModelVisual3D>();
        KwikEngine _engine;
        private bool _liveView = true;

        public MainWindow()
        {
            InitializeComponent();

            this.btnToggleCameraView.Click += ToggleCameraView;
            this.btnToggleLiveImage.Click += ToggleLiveImage;
            this.btnToggleTracking.Click += btnToggleTracking_Click;
            this.btnToggleBoxes.Click += btnToggleBoxes_Click;
            this.Closing += MainWindow_Closing;

            _flags.Add("cameraViewVisible", false);
            pnlCameraView.Visibility = System.Windows.Visibility.Collapsed;

            var fpsTimer = new System.Windows.Threading.DispatcherTimer();
            fpsTimer.Tick += (s, e) => { this.txtFPS.Text = _fps.ToString(); _fps = 0; };
            fpsTimer.Interval = new TimeSpan(0, 0, 1);
            fpsTimer.Start();

            _engine = new KwikEngine();
            _engine.Init(this);
            _engine.LoadGame<Cones.ConeAvoidance>();
            _engine.ObjectMotionEvent += _engine_ObjectMotionEvent;

            XSlider.ValueChanged += Slider_ValueChanged;
            YSlider.ValueChanged += Slider_ValueChanged;
            ZSlider.ValueChanged += Slider_ValueChanged;

            this.KeyDown += MainWindow_KeyDown;
        }

        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F12)
                this.Controls.Visibility = this.Controls.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Camera.Position = new Point3D(XSlider.Value, YSlider.Value, ZSlider.Value);
            Camera.LookDirection = new Vector3D(XSlider.Value, YSlider.Value, ZSlider.Value);
        }
       
        void _engine_ObjectMotionEvent(object sender, ObjectEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
              {
                  Transform3DGroup TransformGroup = new Transform3DGroup();
                  TranslateTransform3D TranslateTransform = new TranslateTransform3D(e.Obj.Position);

                  TransformGroup.Children.Add(TranslateTransform);

                  int objIndex = this.Viewport.Children.IndexOf(e.Obj.Model);
                  this.Viewport.Children[objIndex].Transform = TransformGroup;
              }));
            
            
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.NewTrackingImage -= _game_NewCameraImage;
            _engine.NewCameraImage -= _game_NewCameraImage;
            _engine.ObjectMotionEvent -= _engine_ObjectMotionEvent;
            _engine.StopTracking();
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

        private void _game_NewCameraImage(object sender, ImageEventArgs e)
        {
            _fps = ++_fps;

            if (_flags["cameraViewVisible"])
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    this.imgCameraView.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(e.Image.GetHbitmap(),
                            IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }));
        
            }
        }

        public void AddObject(GameObject newObject)
        {
            Transform3DGroup TransformGroup = new Transform3DGroup();
            TranslateTransform3D TranslateTransform = new TranslateTransform3D(newObject.Position);

            TransformGroup.Children.Add(TranslateTransform);
            newObject.Model.Transform = TransformGroup;

            this.Viewport.Children.Add(newObject.Model);
        }
    }
}
