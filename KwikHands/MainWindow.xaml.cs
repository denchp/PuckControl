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
    using System.Media;
    using System.Windows.Resources;
    using System.Windows.Markup;
    using HelixToolkit.Wpf;
    using System.IO;
    using System.Resources;
    using KwikHands.Engine;

    public partial class MainWindow : Window
    {

        private Dictionary<GameObject, ModelVisual3D> _gameObjects = new Dictionary<GameObject, ModelVisual3D>();
        KwikEngine _engine;

        private List<HudItem> _hudItems = new List<HudItem>();
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private double ViewportScalingFactor = .1;
        private DebugWindow _debugWindow;
        private Menu _menuWindow;

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            _engine = new KwikEngine();
            _debugWindow = new DebugWindow(_engine);
            _menuWindow = new Menu(_engine);

            _engine.Init();
            _engine.ObjectMotion += _engine_ObjectMotion;
            _engine.PlayMedia += _engine_MediaEvent;
            _engine.UpdateHudItem += _engine_UpdateHudItem;
            _engine.NewHudItem += _engine_NewHudItem;
            _engine.NewObject += _engine_NewObject;
            _engine.RemoveObject += _engine_RemoveObject;
            this.KeyDown += MainWindow_KeyDown;

            _menuWindow.Visibility = System.Windows.Visibility.Visible;
        }

        void _engine_RemoveObject(object sender, ObjectEventArgs e)
        {
            var viewportObject = _gameObjects[e.Obj];
            hlxViewport.Children.Remove(viewportObject);
            _gameObjects.Remove(e.Obj);
        }

        void _engine_NewObject(object sender, ObjectEventArgs e)
        {
            AddObject(e.Obj);
            if (_debugWindow != null)
            {
                _debugWindow.Dispatcher.Invoke((Action)((() =>
                    {
                        _debugWindow.AddObject(e.Obj.Position, e.Obj.ID);
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
                      Vector3D offsetVector = new Vector3D(e.Obj.Position.X * -1, e.Obj.Position.Y, e.Obj.Position.Z) * ViewportScalingFactor;
                      TranslateTransform3D TranslateTransform = new TranslateTransform3D(offsetVector);

                      TransformGroup.Children.Add(TranslateTransform);
                      int objIndex = this.hlxViewport.Children.IndexOf(_gameObjects[e.Obj]);

                      this.hlxViewport.Children[objIndex].Transform = TransformGroup;

                      if (e.ObjType == ObjectType.Puck)
                      {
                          var puckModel = _gameObjects[e.Obj];
                          Rect3D puckBoundingBox = puckModel.Content.Bounds;
                          puckBoundingBox.Offset(offsetVector);

                          // puck moved so we'll check to see if we have hit any cones or targets
                          foreach (var gameObject in _gameObjects.Keys.Where(x => x.Type == ObjectType.Cone || x.Type == ObjectType.Target))
                          {
                              var coneModel = _gameObjects[gameObject];
                              Rect3D objectBoundingBox = coneModel.Content.Bounds;
                              objectBoundingBox.Offset(gameObject.Position);

                              if (puckBoundingBox.IntersectsWith(objectBoundingBox))
                              {
                                  _engine.PuckCollision(gameObject);
                                using (StreamWriter w = File.AppendText("log.txt"))
                                {
                                    Startup.Log("Collision:", w);
                                    Startup.Log("Puck Location: " + offsetVector.ToString(), w);
                                    Startup.Log("Collided with: " + gameObject.ID + " at " + gameObject.Position.ToString(), w);
                                    Startup.Log("Puck Bounding: " + puckBoundingBox.ToString(), w);
                                    Startup.Log("Object Bound:  " + objectBoundingBox.ToString(), w);
                                }
                              }
                          }
                      }
                  }
                  catch { }
              }));
        }

        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F12)
                _debugWindow.Visibility = _debugWindow.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.ObjectMotion -= _engine_ObjectMotion;
            _engine.UpdateHudItem -= _engine_UpdateHudItem;
            _engine.NewHudItem -= _engine_NewHudItem;

            _engine.StopTracking();
            _debugWindow.Close();
            _menuWindow.Close();
        }

        private void _engine_UpdateHudItem(object sender, HudItemEventArgs e)
        {
            UpdateHudItem(e.Item);
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

        public void AddObject(GameObject newObject)
        {
            string resourceName = "";
            switch (newObject.Type)
            {
                case ObjectType.Cone:
                    resourceName = "cone_highdef.3ds";
                    break;
                case ObjectType.Rink:
                    resourceName = "Rink.xaml";
                    break;
                case ObjectType.Target:
                    resourceName = "Target.3ds";
                    break;
                case ObjectType.Puck:
                    resourceName = "puck.xaml";
                    break;
            }
            newObject.Position *= ViewportScalingFactor;

            this.hlxViewport.Dispatcher.Invoke((Action)(() =>
            {
                Model3DGroup newModel = GetModel(newObject, resourceName, resourceName.Substring(resourceName.LastIndexOf('.') + 1));
                ModelVisual3D visual3d = new ModelVisual3D();
                visual3d.Content = newModel;
                _gameObjects.Add(newObject, visual3d);

                this.hlxViewport.Children.Add(visual3d);
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
            TranslateTransform3D TranslateTransform = new TranslateTransform3D(newObject.Position);
            ModelVisual3D newModel = null;
            Model3DGroup newGroup = new Model3DGroup();
            ModelImporter importer = new ModelImporter();

            string resourceBase = "pack://application:,,,/KwikHands;component/GameAssets/";
            switch (fileExt)
            {
                case "xaml":
                    String resourceUri = resourceBase + resourceName;
                    var info = Application.GetResourceStream(new Uri(resourceUri));

                    newModel = (ModelVisual3D)XamlReader.Load(info.Stream);
                    newGroup.Children.Add(newModel.Content);
                    break;
                case "3ds":
                    newGroup = importer.Load(@"GameAssets\" + resourceName);
                    try
                    {
                        if (File.Exists(@"GameAssets\" + resourceName + ".jpg"))
                        {
                            var textureBrush = new ImageBrush(new BitmapImage(new Uri(@"GameAssets\" + resourceName + ".jpg", UriKind.Relative)));
                            ((GeometryModel3D)newGroup.Children[0]).Material = new DiffuseMaterial(textureBrush);
                        }
                    }
                    catch (Exception ex) { }
                    break;
            }

#if DEBUG
            if (resourceName.IndexOf("Rink") < 0)
            {
                HelixToolkit.Wpf.MeshBuilder meshBuilder = new MeshBuilder();

                var boundingRect = newGroup.Bounds;
                meshBuilder.AddBox(boundingRect);

                var mesh = meshBuilder.ToMesh();
                var newGeo = new GeometryModel3D();
                var boxMat = MaterialHelper.CreateMaterial(Colors.Blue);

                newGeo.Material = boxMat;
                newGeo.BackMaterial = boxMat;
                newGeo.Geometry = mesh;
                newGroup.Children.Add(newGeo);
            }
#endif

            TransformGroup.Children.Add(TranslateTransform);
            newGroup.Transform = TransformGroup;

            return newGroup;
        }
    }
}
