using HelixToolkit.Wpf;
using PuckControl.Domain;
using PuckControl.Domain.Entities;
using PuckControl.Domain.EventArg;
using PuckControl.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace PuckControl.Windows
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Window, IDisposable
    {
        private GameEngine _engine;
        private int _fps = 0;
        private int _ups = 0;
        private bool _liveView = true;
        private Dictionary<GameObject, ModelVisual3D> _gameObjects;
        private SoundPlayer _player;
        private double ViewportScalingFactor = .1;
        
        public Game(GameEngine engine)
        {
            InitializeComponent();
            _gameObjects = new Dictionary<GameObject, ModelVisual3D>();
            _player = new SoundPlayer();

            var oneSecondTimer = new System.Windows.Threading.DispatcherTimer();
            
            if (engine == null)
                throw new ArgumentException("Game cannot be initialized with null engine");

            _engine = engine;
            _engine.ObjectMotion += _engine_ObjectMotion;
            _engine.PlayMedia += _engine_MediaEvent;
            _engine.UpdateHUDItem += _engine_UpdateHUDItem;
            _engine.NewHUDItem += _engine_NewHUDItem;
            _engine.RemoveHUDItem += _engine_RemoveHUDItem;
            _engine.NewObject += _engine_NewObject;
            _engine.RemoveObject += _engine_RemoveObject;
            _engine.TrackingUpdateReceived += _engine_TrackingUpdateReceived;

            oneSecondTimer.Tick += (s, e) =>
            {
                this.txtFPS.Text = "FPS: " + _fps.ToString();
                this.txtUPS.Text = "UPS: " + _ups.ToString();

                _fps = 0;
                _ups = 0;
            };

            oneSecondTimer.Interval = new TimeSpan(0, 0, 1);
            oneSecondTimer.Start();
            this.KeyDown += Game_KeyDown;
        }

        void Game_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C:
                    ToggleCameraView();
                    break;
                case Key.L:
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
                case Key.F:
                    txtFPS.Visibility = txtFPS.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case Key.U:
                    txtUPS.Visibility = txtUPS.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case Key.Escape:
                    _engine.EndGame();
                    break;
            }
        }

        private void ToggleCameraView()
        {
            bool showing = this.imgCameraView.Visibility == System.Windows.Visibility.Collapsed;
            this.imgCameraView.Visibility = this.imgCameraView.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

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

        private void _engine_TrackingUpdateReceived(object sender, EventArgs e)
        {
            _ups = ++_ups;
        }

        private void _engine_NewCameraImage(object sender, ImageEventArgs e)
        {
            _fps++;
            
            this.Dispatcher.Invoke((Action)(() =>
            {
                IntPtr hBmp = e.Image.GetHbitmap();

                try
                {
                    this.imgCameraView.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBmp,
                        IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    NativeMethods.DeleteObject(hBmp);
                    e.Image.Dispose();
                }
            }));
        }

        private void _engine_RemoveHUDItem(object sender, HUDItemEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                var hudItems = this.HUDGrid.Children;
                List<FrameworkElement> removeList = new List<FrameworkElement>();

                foreach (FrameworkElement element in hudItems)
                {
                    if (element.Name == e.Item.Name)
                    {
                        removeList.Add(element);
                    }
                }

                removeList.ForEach(x => HUDGrid.Children.Remove(x));
            }));
        }

        private void _engine_RemoveObject(object sender, ObjectEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                var viewportObject = _gameObjects[e.Obj];
                hlxViewport.Children.Remove(viewportObject);
                _gameObjects.Remove(e.Obj);
            }));
        }

        private void _engine_NewObject(object sender, ObjectEventArgs e)
        {
            AddObject(e.Obj);
        }

        private void _engine_NewHUDItem(object sender, HUDItemEventArgs e)
        {
            AddHUDItem(e.Item);
        }

        private void _engine_MediaEvent(object sender, MediaEventArgs e)
        {
            _player.Stream = Application.GetResourceStream(e.MediaFile).Stream;
            _player.Play();
        }

        private void _engine_ObjectMotion(object sender, ObjectEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
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
                    foreach (var gameObject in _gameObjects.Keys.Where(x => x != e.Obj && x.Model.IsGameWorld == false).ToList())
                    {
                        if (gameObject.Bounds.IntersectsWith(e.Obj.Bounds))
                        {
                            _engine.PuckCollision(gameObject);
#if DEBUG
                            using (StreamWriter w = File.AppendText("log.txt"))
                            {
                                Startup.Log("Collision:", w);
                                Startup.Log("Puck Location: " + offsetVector.ToString(), w);
                                Startup.Log("Collided with: " + gameObject.ObjectType + " at " + gameObject.Position.ToString(), w);
                            }
#endif
                        }
                    }
                }
            }));
        }

        private void _engine_UpdateHUDItem(object sender, HUDItemEventArgs e)
        {
            UpdateHUDItem(e.Item);
        }

        private void AddObject(GameObject newObject)
        {
            this.hlxViewport.Dispatcher.Invoke((Action)(() =>
            {
                Model3DGroup newModel = GetModel(newObject, newObject.Model.ModelFile, newObject.Model.ModelFile.Substring(newObject.Model.ModelFile.LastIndexOf('.') + 1));
                ModelVisual3D visual3d = new ModelVisual3D();
                visual3d.Content = newModel;

                _gameObjects.Add(newObject, visual3d);
                this.hlxViewport.Children.Add(visual3d);
            }));
        }

        private void AddHUDItem(HUDItem newItem)
        {
            TextBlock newFrameworkItem = null;

            switch (newItem.ItemType)
            {
                case HUDItemType.Text:
                    newFrameworkItem = new TextBlock();
                    newFrameworkItem.Text = newItem.Label + newItem.Text;
                    newFrameworkItem.FontSize = 64 * newItem.Size;
                    newFrameworkItem.Name = newItem.Name;
                    this.HUD.Children.Add(newFrameworkItem);
                    break;
                case HUDItemType.Numeric:
                    newFrameworkItem = new TextBlock();
                    newFrameworkItem.Text = newItem.Label + newItem.Value.ToString();
                    newFrameworkItem.FontSize = 64 * newItem.Size;
                    newFrameworkItem.Name = newItem.Name;

                    //Canvas.SetTop(newFrameworkItem, absY);
                    this.HUDGrid.Children.Add(newFrameworkItem);
                    break;
                case HUDItemType.Timer: break;
            }
            if (newFrameworkItem != null)
            {
                newFrameworkItem.HorizontalAlignment = newItem.HorizontalPosition;
                newFrameworkItem.VerticalAlignment = newItem.VerticalPosition;
            }
        }

        private void UpdateHUDItem(HUDItem updatedItem)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                foreach (TextBlock element in this.HUDGrid.Children.OfType<TextBlock>())
                {
                    if (element.Name == updatedItem.Name)
                    {
                        if (updatedItem.ItemType == HUDItemType.Text)
                            element.Text = updatedItem.Label + updatedItem.Text;
                        else
                            element.Text = updatedItem.Label + updatedItem.Value.ToString();
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
            string assemblyLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string resourceBase = "pack://application:,,,/PuckControl;component/GameAssets/";
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
                    newGroup = objReader.Read(assemblyLocation + @"\GameAssets\" + resourceName);
                    break;
                case "3ds":
                    ModelImporter importer = new ModelImporter();
                    newGroup = importer.Load(assemblyLocation + @"\GameAssets\" + resourceName);
                    foreach (var material in newObject.Model.Materials)
                    {
                        MaterialGroup materialGroup = new MaterialGroup();

                        Brush materialBrush = new SolidColorBrush(material.DiffuseColor);
                        materialBrush.Opacity = material.Opacity;
                        materialGroup.Children.Add(MaterialHelper.CreateMaterial(materialBrush, material.SpecularPower));

                        if (!String.IsNullOrWhiteSpace(material.TextureFile))
                        {
                            if (File.Exists(assemblyLocation + @"\GameAssets\" + material.TextureFile))
                            {
                                var texture = MaterialHelper.CreateImageMaterial(new BitmapImage(new Uri(assemblyLocation + @"\GameAssets\" + material.TextureFile, UriKind.Relative)), material.Opacity);
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

        #region IDispose Implementation
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _engine.Dispose();
                _player.Dispose();
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion
    }
}
