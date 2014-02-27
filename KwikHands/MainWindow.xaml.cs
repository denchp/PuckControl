namespace KwikHands
{
    using KwikHands.Domain;
    using KwikHands.Domain.EventArg;
    using KwikHands.Tracking;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using System.Linq;
    using System.Windows.Media;

    public partial class MainWindow : Window, IGameWindow
    {

        private Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        private int _fps = 0;
        private List<GameObject> _gameObjects = new List<GameObject>();
        KwikEngine _engine;
        private bool _liveView = true;
        private List<HudItem> _hudItems = new List<HudItem>();

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
            _engine.StartGame();

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

                  if (e.ObjType == ObjectType.Puck)
                  {
                      Rect3D puckBoundingBox = e.Obj.Model.Content.Bounds;
                      puckBoundingBox = e.Obj.Model.Transform.TransformBounds(puckBoundingBox);

                      // puck moved so we'll check to see if we have hit any cones or targets
                      foreach (var gameObject in _gameObjects.Where(x => x.Type == ObjectType.Cone || x.Type == ObjectType.Target))
                      {
                          Rect3D objectBoundingBox = gameObject.Model.Content.Bounds;
                          objectBoundingBox.Location = (Point3D)gameObject.Position;
                          //objectBoundingBox = gameObject.Model.Transform.TransformBounds(objectBoundingBox);

                          if (puckBoundingBox.IntersectsWith(objectBoundingBox))
                          {
                              _engine.PuckCollision(gameObject);
                          }
                      }
                  }
              }));
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.NewTrackingImage -= _game_NewCameraImage;
            _engine.NewCameraImage -= _game_NewCameraImage;
            _engine.ObjectMotionEvent -= _engine_ObjectMotionEvent;
            _engine.StopTracking();
        }

        private HitTestResultBehavior ResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult =
                    rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    // Yes we did!
                }
            }

            return HitTestResultBehavior.Continue;
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
            _gameObjects.Add(newObject);
            Transform3DGroup TransformGroup = new Transform3DGroup();
            TranslateTransform3D TranslateTransform = new TranslateTransform3D(newObject.Position);

            TransformGroup.Children.Add(TranslateTransform);
            newObject.Model.Transform = TransformGroup;

            this.Viewport.Children.Add(newObject.Model);
        }

        public void AddHudItem(HudItem newItem)
        {
            FrameworkElement newFrameworkItem = null;

            switch (newItem.Type)
            {
                case HudItem.HudItemType.Text:
                    newFrameworkItem = new TextBlock();
                    ((TextBlock)newFrameworkItem).Text = newItem.Label + newItem.Text;
                    newFrameworkItem.Name = newItem.Name;
                    this.HUD.Children.Add(newFrameworkItem);
                    break;
                case HudItem.HudItemType.Numeric:
                    newFrameworkItem = new TextBlock();
                    ((TextBlock)newFrameworkItem).Text = newItem.Label + newItem.Value.ToString();
                    newFrameworkItem.Name = newItem.Name;
                    
                    //Canvas.SetTop(newFrameworkItem, absY);
                    this.HUDGrid.Children.Add(newFrameworkItem);
                    break;
                case HudItem.HudItemType.Timer: break;
            }
            if (newFrameworkItem != null)
            {
                switch (newItem.HorizontalPosition)
                {
                    case HudItem.HorizontalAlignment.Left: newFrameworkItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; break;
                    case HudItem.HorizontalAlignment.Center: newFrameworkItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Center; break;
                    case HudItem.HorizontalAlignment.Right: newFrameworkItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Right; break;
                }

                switch (newItem.VerticalPosition)
                {
                    case HudItem.VerticalAlignment.Top: newFrameworkItem.VerticalAlignment = System.Windows.VerticalAlignment.Top; break;
                    case HudItem.VerticalAlignment.Middle: newFrameworkItem.VerticalAlignment = System.Windows.VerticalAlignment.Center; break;
                    case HudItem.VerticalAlignment.Bottom: newFrameworkItem.VerticalAlignment = System.Windows.VerticalAlignment.Bottom; break;
                }
            }
        }

        public void UpdateHudItem(HudItem updatedItem)
        {
#if DEBUG
            if (updatedItem.Type == HudItem.HudItemType.Text)
                System.Diagnostics.Debug.WriteLine(updatedItem.Text);
            else
                System.Diagnostics.Debug.WriteLine(updatedItem.Value);
#endif
            this.Dispatcher.Invoke((Action)(() =>
            {
                foreach (var element in this.HUDGrid.Children)
                {
                    if (((FrameworkElement)element).Name == updatedItem.Name)
                    {
                            if (updatedItem.Type == HudItem.HudItemType.Text)
                                ((TextBlock)element).Text = updatedItem.Label + updatedItem.Text;
                            else
                                ((TextBlock)element).Text = updatedItem.Label + updatedItem.Value.ToString();
                    }
                }
            }));
                    
        }
    }
}
