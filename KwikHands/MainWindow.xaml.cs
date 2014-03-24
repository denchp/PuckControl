namespace KwikHands
{
    using HelixToolkit.Wpf;
    using KwikHands.Domain;
    using KwikHands.Domain.Entities;
    using KwikHands.Domain.EventArg;
    using KwikHands.Engine;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Reflection;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;

    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private Dictionary<GameObject, ModelVisual3D> _gameObjects = new Dictionary<GameObject, ModelVisual3D>();
        KwikEngine _engine;

        private List<HudItem> _hudItems = new List<HudItem>();
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private double ViewportScalingFactor = .1;
        private DebugWindow _debugWindow;
        private List<IGame> _games;
        private bool _liveView = true;
        private int _fps = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            _games = FindGames();

            foreach (IGame gameType in _games)
            {
                IGame game = (IGame)Activator.CreateInstance(gameType.GetType());

                if (String.IsNullOrWhiteSpace(game.Name))
                    continue;

                Button tileButton = new Button();
                tileButton.Background = new SolidColorBrush(game.TileColor);
                tileButton.BorderThickness = new Thickness(0,0,0,0);
                tileButton.Height = 200;
                tileButton.Width = 200;

                Label buttonText = new Label();
                buttonText.Content = game.Name;

                tileButton.Content = buttonText;
                tileButton.Click += (s, e) => {
                    _engine.LoadGame(gameType);
                    GameList.Visibility = System.Windows.Visibility.Hidden;
                    MenuBar.Visibility = System.Windows.Visibility.Collapsed;
                    _engine.StartGame();
                    };

                GameList.Children.Add(tileButton);
            }

            _engine = new KwikEngine();
            _debugWindow = new DebugWindow(_engine);
            
            _engine.Init();
            _engine.ObjectMotion += _engine_ObjectMotion;
            _engine.PlayMedia += _engine_MediaEvent;
            _engine.UpdateHudItem += _engine_UpdateHudItem;
            _engine.NewHudItem += _engine_NewHudItem;
            _engine.RemoveHudItem += _engine_RemoveHudItem;
            _engine.NewObject += _engine_NewObject;
            _engine.RemoveObject += _engine_RemoveObject;
            _engine.GameOver += _engine_GameOver;

            btnEnableCameraView.Click += btnEnableCameraView_Click;
            btnSettings.Click += btnSettings_Click;
            btnExit.Click += (s, e) => { this.Close(); };

            this.KeyDown += MainWindow_KeyDown;

            var fpsTimer = new System.Windows.Threading.DispatcherTimer();
            fpsTimer.Tick += (s, e) => { this.txtFPS.Text = _fps.ToString(); _fps = 0; };
            fpsTimer.Interval = new TimeSpan(0, 0, 1);
            fpsTimer.Start();
        }


        void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void btnEnableCameraView_Click(object sender, RoutedEventArgs e)
        {
            bool showing = this.pnlCameraView.Visibility == System.Windows.Visibility.Collapsed;
            this.pnlCameraView.Visibility = this.pnlCameraView.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            if (showing)
            {
                if (_liveView)
                {
                    _engine.NewTrackingImage -= _engine_NewCameraImage;
                    _engine.NewCameraImage += _engine_NewCameraImage;
                }
                else
                {
                    _engine.NewTrackingImage += _engine_NewCameraImage;
                    _engine.NewCameraImage -= _engine_NewCameraImage;
                }
            }
            else
            {
                _engine.NewTrackingImage -= _engine_NewCameraImage;
                _engine.NewCameraImage -= _engine_NewCameraImage;
            }
        }

        private void _engine_NewCameraImage(object sender, ImageEventArgs e)
        {
            _fps = ++_fps;
            this.Dispatcher.Invoke((Action)(() =>
            {
                try
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
                }
                catch { }
            }));
        }

        void _engine_RemoveHudItem(object sender, HudItemEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                var hudItems = this.HUDGrid.Children;
                List<FrameworkElement> removeList = new List<FrameworkElement>();

                foreach (var element in hudItems)
                {
                    if (((FrameworkElement)element).Name == e.Item.Name)
                    {
                        removeList.Add((FrameworkElement)element);
                    }
                }

                removeList.ForEach(x => HUDGrid.Children.Remove(x));
            }));
        }

        void _engine_GameOver(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                var delay = new System.Windows.Threading.DispatcherTimer();
                delay.Tick += (s, args) => {
                    this.GameList.Visibility = System.Windows.Visibility.Visible;
                    this.MenuBar.Visibility = System.Windows.Visibility.Visible;
                    this.GameList.SetValue(Panel.ZIndexProperty, 100);
                    this.MenuBar.SetValue(Panel.ZIndexProperty, 100);
                    delay.Stop();
                };
                delay.Interval = new TimeSpan(0, 0, 4);
                delay.Start();
                //this.HighScoreList.Visibility = System.Windows.Visibility.Visible;
                
            }));

        }

        void _engine_RemoveObject(object sender, ObjectEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    var viewportObject = _gameObjects[e.Obj];
                    hlxViewport.Children.Remove(viewportObject);
                    _gameObjects.Remove(e.Obj);
                }
                catch { }
            }));
        }

        void _engine_NewObject(object sender, ObjectEventArgs e)
        {
            AddObject(e.Obj);
            if (_debugWindow != null)
            {
                _debugWindow.Dispatcher.Invoke((Action)((() =>
                    {
                        _debugWindow.AddObject(e.Obj.Position, e.Obj.Type);
                    })));
            }
        }

        void _engine_NewHudItem(object sender, HudItemEventArgs e)
        {
            AddHudItem(e.Item);
        }

        void _engine_MediaEvent(object sender, MediaEventArgs e)
        {
            SoundPlayer player = new SoundPlayer(Application.GetResourceStream(e.MediaFile).Stream);
            player.Play();
        }

        void _engine_ObjectMotion(object sender, ObjectEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
                {
                    try
                    {
                        Transform3DGroup TransformGroup = new Transform3DGroup();
                        Vector3D offsetVector = new Vector3D(e.Obj.Position.X * -1, e.Obj.Position.Y, e.Obj.Position.Z);
                        TranslateTransform3D TranslateTransform = new TranslateTransform3D(offsetVector * ViewportScalingFactor);

                        TransformGroup.Children.Add(TranslateTransform);
                        e.Obj.Bounds = TranslateTransform.TransformBounds(_gameObjects[e.Obj].Content.Bounds);

                        int objIndex = this.hlxViewport.Children.IndexOf(_gameObjects[e.Obj]);

                        this.hlxViewport.Children[objIndex].Transform = TransformGroup;

                        if (e.Obj.TrackCollisions)
                        {
                            foreach (var gameObject in _gameObjects.Keys.Where(x => x != e.Obj && x.Model.IsGameWorld == false))
                            {
                                if (gameObject.Bounds.IntersectsWith(e.Obj.Bounds))
                                {
                                    _engine.PuckCollision(gameObject);
                                    using (StreamWriter w = File.AppendText("log.txt"))
                                    {
                                        Startup.Log("Collision:", w);
                                        Startup.Log("Puck Location: " + offsetVector.ToString(), w);
                                        Startup.Log("Collided with: " + gameObject.Type + " at " + gameObject.Position.ToString(), w);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }));
        }

        private Rect To2D(Rect3D rect3D)
        {
            return new Rect(rect3D.X, rect3D.Y, rect3D.SizeX, rect3D.SizeY);
        }

        private Vector To2D(Vector3D vector3d)
        {
            return new Vector(vector3d.X, vector3d.Y);
        }

        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.F12:
                    _debugWindow.Visibility = _debugWindow.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case System.Windows.Input.Key.C:
                    _liveView = !_liveView;
                    if (_liveView)
                    {
                        _engine.NewTrackingImage -= _engine_NewCameraImage;
                        _engine.NewCameraImage += _engine_NewCameraImage;
                    }
                    else
                    {
                        _engine.NewTrackingImage += _engine_NewCameraImage;
                        _engine.NewCameraImage -= _engine_NewCameraImage;
                    }
                break;
                case System.Windows.Input.Key.Escape:
                    _engine.EndGame();
                break;
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.ObjectMotion -= _engine_ObjectMotion;
            _engine.UpdateHudItem -= _engine_UpdateHudItem;
            _engine.NewHudItem -= _engine_NewHudItem;

            _engine.StopTracking();
            _debugWindow.Close();
        }

        private void _engine_UpdateHudItem(object sender, HudItemEventArgs e)
        {
            UpdateHudItem(e.Item);
        }

        public void AddObject(GameObject newObject)
        {
            this.hlxViewport.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    Model3DGroup newModel = GetModel(newObject, newObject.Model.ModelFile, newObject.Model.ModelFile.Substring(newObject.Model.ModelFile.LastIndexOf('.') + 1));
                    ModelVisual3D visual3d = new ModelVisual3D();
                    visual3d.Content = newModel;

                    _gameObjects.Add(newObject, visual3d);
                    this.hlxViewport.Children.Add(visual3d);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }));
        }

        private void AddHudItem(HudItem newItem)
        {
            FrameworkElement newFrameworkItem = null;

            switch (newItem.Type)
            {
                case HudItem.HudItemType.Text:
                    newFrameworkItem = new TextBlock();
                    ((TextBlock)newFrameworkItem).Text = newItem.Label + newItem.Text;
                    ((TextBlock)newFrameworkItem).FontSize = 64 * newItem.Size;
                    newFrameworkItem.Name = newItem.Name;
                    this.HUD.Children.Add(newFrameworkItem);
                    break;
                case HudItem.HudItemType.Numeric:
                    newFrameworkItem = new TextBlock();
                    ((TextBlock)newFrameworkItem).Text = newItem.Label + newItem.Value.ToString();
                    ((TextBlock)newFrameworkItem).FontSize = 64 * newItem.Size;
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

        private void UpdateHudItem(HudItem updatedItem)
        {
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

        private Model3DGroup GetModel(GameObject newObject, String resourceName, String fileExt)
        {
            Transform3DGroup TransformGroup = new Transform3DGroup();
            TranslateTransform3D TranslateTransform = new TranslateTransform3D(newObject.Position * ViewportScalingFactor);
            ModelVisual3D newModel = null;
            Model3DGroup newGroup = new Model3DGroup();

            string resourceBase = "pack://application:,,,/KwikHands;component/GameAssets/";
            switch (fileExt)
            {
                case "xaml":
                    String resourceUri = resourceBase + resourceName;
                    var info = Application.GetResourceStream(new Uri(resourceUri));

                    newModel = (ModelVisual3D)XamlReader.Load(info.Stream);
                    newGroup.Children.Add(newModel.Content);
                    break;
                case "obj":
                    ObjReader objReader = new ObjReader();
                    newGroup = objReader.Read(@"GameAssets\" + resourceName);
                    break;
                case "3ds":
                    ModelImporter importer = new ModelImporter();
                    newGroup = importer.Load(@"GameAssets\" + resourceName);
                    try
                    {
                        foreach (var material in newObject.Model.Materials)
                        {
                            MaterialGroup materialGroup = new MaterialGroup();

                            Brush materialBrush = new SolidColorBrush(material.DiffuseColor);
                            materialBrush.Opacity = material.Opacity;
                            materialGroup.Children.Add(MaterialHelper.CreateMaterial(materialBrush, material.SpecularPower));
                            
                            if (!String.IsNullOrWhiteSpace(material.TextureFile))
                            {
                                if (File.Exists(@"GameAssets\" + material.TextureFile))
                                {
                                    var texture = MaterialHelper.CreateImageMaterial(new BitmapImage(new Uri(@"GameAssets\" + material.TextureFile, UriKind.Relative)), material.Opacity);
                                    materialGroup.Children.Add(texture);
                                }
                            }
                            
                            var specular = new SpecularMaterial();
                            specular.SpecularPower = material.SpecularPower;
                            specular.Color = material.SpecularColor;

                            var emissive = new EmissiveMaterial();
                            emissive.Color = material.EmissiveColor;
                            
                            materialGroup.Children.Add(specular);
                            materialGroup.Children.Add(emissive);

                            ((GeometryModel3D)newGroup.Children[material.MeshIndex]).Material = materialGroup;
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
            }

#if DEBUG
            if (resourceName.IndexOf("Rink") < 0)
            {
                HelixToolkit.Wpf.MeshBuilder meshBuilder = new MeshBuilder();

                var boundingRect = newGroup.Bounds;
                meshBuilder.AddBoundingBox(boundingRect, 3);
            }
#endif
            double CenterX = newGroup.Bounds.SizeX / 2;
            double CenterY = newGroup.Bounds.SizeY / 2;
            double CenterZ = newGroup.Bounds.SizeZ / 2;
            ScaleTransform3D ScaleTransform = new ScaleTransform3D(newObject.Scale.X, newObject.Scale.Y, newObject.Scale.Z, CenterX, CenterY, CenterZ);

            TransformGroup.Children.Add(TranslateTransform);
            TransformGroup.Children.Add(ScaleTransform);

            newGroup.Transform = TransformGroup;

            if (newObject.ApplyPhysics)
            {
                var rinkBounds = _gameObjects.Where(x => x.Key.Model.IsGameWorld).First().Value.Content.Bounds;
                TranslateTransform.OffsetZ -= (newGroup.Bounds.Z - rinkBounds.Z);
            }
            newObject.Bounds = newGroup.Bounds;

            return newGroup;
        }

        private List<IGame> FindGames()
        {
            List<IGame> games = new List<IGame>();
            string folder = System.AppDomain.CurrentDomain.BaseDirectory;

            string[] files = Directory.GetFiles(folder, "*.dll");

            foreach (string file in files)
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);

                    foreach (Type type in assembly.GetTypes())
                    {
                        Type iface = type.GetInterface("IGame");

                        if (iface != null)
                        {
                            try
                            {
                                IGame plugin = (IGame)Activator.CreateInstance(type);
                                games.Add(plugin);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
                catch { };
            return games;
        }
    }
}
