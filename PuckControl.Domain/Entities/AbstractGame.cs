using PuckControl.Domain.EventArg;
using PuckControl.Domain.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PuckControl.Domain.Entities
{
    public abstract class AbstractGame : IGame, IDisposable
    {
        private bool _disposed;
        private GameStage _currentStage;

        public event EventHandler<ObjectEventArgs> NewObjectEvent;
        public event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        public event EventHandler<ObjectEventArgs> ObjectCollisionEvent;
        
        public event EventHandler<HUDItemEventArgs> NewHUDItemEvent;
        public event EventHandler<HUDItemEventArgs> RemoveHUDItemEvent;
        public event EventHandler<HUDItemEventArgs> UpdateHUDItemEvent;

        public event EventHandler<MediaEventArgs> MediaEvent;
        public event EventHandler<GameStageEventArgs> GameStageChange;

        protected ObservableCollection<GameObject> GameObjects { get; private set; }
        protected ObservableCollection<HUDItem> HUDItems { get; private set; }
        
        protected AbstractGame()
        {
            GameObjects = new ObservableCollection<GameObject>();
            HUDItems = new ObservableCollection<HUDItem>();

            GameObjects.CollectionChanged += _gameObjects_CollectionChanged;
            HUDItems.CollectionChanged += _hudItems_CollectionChanged;
        }

        protected void AddRink()
        {
            if (GameObjects.Count(x => x.Model.IsGameWorld) > 0)
                return;

            GameObject _rink = new GameObject();

            _rink.ApplyPhysics = false;
            _rink.Model.ModelFile = "rink.3ds";
            _rink.Scale = new Vector3D(30, 30, 30);
            _rink.Position = new Vector3D(15, 50, -1);
            _rink.Model.IsGameWorld = true;

            ModelMaterial ice = new ModelMaterial();
            ice.TextureFile = "rink.3ds.png";
            ice.MeshIndex = 0;
            ice.SpecularPower = 15000;
            ice.SpecularColor = Color.FromRgb(255, 255, 255);

            ModelMaterial glass = new ModelMaterial();
            glass.Opacity = .12;
            glass.MeshIndex = 2;

            _rink.Model.Materials.Add(glass);
            _rink.Model.Materials.Add(ice);

            GameObjects.Add(_rink);
        }

        protected void AddPuck()
        {
            GameObject _puck = new GameObject();

            _puck.Position = new Vector3D(20, 20, 0);
            _puck.TrackCollisions = true;
            _puck.MotionSmoothingSteps = 2;
            _puck.ControlledObject = true;
            _puck.Model.ModelFile = "puck.3ds";
            _puck.ControlledObject = true;
            _puck.Active = false;
            _puck.ObjectType = "Puck";
            GameObjects.Add(_puck);
        }

        public abstract bool Init();
        public abstract void StartGame();
        public abstract void Collision(GameObject objectOne, GameObject objectTwo);
        public abstract int? Score { get; }
        public ControlType ControlType { get; set; }
        public Color TileColor { get; protected set; }
        public string Name { get; protected set; }

        void _hudItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (NewHUDItemEvent != null && e.NewItems != null)
            {
                var args = new HUDItemEventArgs();

                foreach (HUDItem item in e.NewItems)
                {
                    item.Changed += item_Changed;
                    args.Item = item;
                    NewHUDItemEvent(this, args);
                }
            }
            if (RemoveHUDItemEvent != null && e.OldItems != null)
            {
                var args = new HUDItemEventArgs();
                foreach (HUDItem item in e.OldItems)
                {
                    item.Changed -= item_Changed;
                    args.Item = item;
                    RemoveHUDItemEvent(this, args);
                }
            }
        }

        void item_Changed(object sender, HUDItemEventArgs e)
        {
            HUDItem item = sender as HUDItem;
            if (UpdateHUDItemEvent != null)
                UpdateHUDItemEvent(this, new HUDItemEventArgs() { Item = item });
        }

        void _gameObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0 && NewObjectEvent != null)
            {
                var args = new ObjectEventArgs();

                foreach (GameObject obj in e.NewItems)
                {
                    args.Obj = obj;
                    NewObjectEvent(this, args);
                }
            }

            if (e.OldItems != null && e.OldItems.Count > 0 && RemoveObjectEvent != null)
            {
                if (RemoveObjectEvent != null)
                {
                    var args = new ObjectEventArgs();

                    foreach (GameObject obj in e.OldItems)
                    {
                        args.Obj = obj;
                        RemoveObjectEvent(this, args);
                    }
                }
            }
        }

        public virtual GameStage CurrentStage
        {
            get
            {
                return _currentStage;
            }
            set
            {
                _currentStage = value;
                if (GameStageChange != null)
                {
                    GameStageChange(this, new GameStageEventArgs() { Stage = value });
                }
            }
        }

        protected void PlayAudio(Uri soundUri)
        {
            if (MediaEvent != null)
                MediaEvent(this, new MediaEventArgs() { MediaFile = soundUri });
        }

        public void CleanUp()
        {
            while (GameObjects.Count() > 0)
                GameObjects.RemoveAt(0);

            while (HUDItems.Count() > 0)
                HUDItems.RemoveAt(0);
        }

        public void EndGame()
        {
            foreach (var item in HUDItems)
            {
                item.Reset();
            }

            this.CurrentStage = GameStage.GameOver;
        }

        public void Reset()
        {
            CleanUp();
            Init();
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

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion
    }
}
