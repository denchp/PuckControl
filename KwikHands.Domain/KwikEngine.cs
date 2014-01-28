using KwikHands.Domain.EventArg;
using KwikHands.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KwikHands.Domain
{
    public class KwikEngine
    {
        public event EventHandler<ImageEventArgs> NewCameraImage;
        public event EventHandler<ImageEventArgs> NewTrackingImage;
        public event EventHandler<ObjectEventArgs> ObjectMotionEvent;

        public bool Tracking = false;
        private Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
        private IGame _game;
        private IGameWindow _gameWindow;
        private Random _rand = new Random();
        private BallTracker _tracker = new BallTracker();
        private Thread _trackingThread;

        public void Init(IGameWindow gameWindow)
        {
            _gameWindow = gameWindow;
            _tracker.NewCameraImage += _tracker_NewCameraImages;
        }

        void _tracker_NewCameraImages(object sender, EventArgs e)
        {
            if (NewCameraImage != null)
            {
                NewCameraImage(this, new ImageEventArgs() { Image = _tracker.CameraImage });
            }

            if (NewTrackingImage != null)
            {
                NewTrackingImage(this, new ImageEventArgs() { Image = _tracker.TrackingImage });
            }
        }

        public void LoadGame<T>() where T : IGame, new()
        {
            _game = new T();
            _game.NewObjectEvent += _game_NewObjectEvent;
            _game.RemoveObjectEvent += _game_RemoveObjectEvent;
            _game.ObjectCollisionEvent += _game_ObjectCollisionEvent;
            _game.ObjectMotionEvent += _game_ObjectMotionEvent;
            _game.Init();
        }

        void _game_ObjectMotionEvent(object sender, ObjectEventArgs e)
        {
            if (this.ObjectMotionEvent != null)
                ObjectMotionEvent(this, e);
        }

        public void StartTracking()
        {
            Tracking = true;
            _trackingThread = new Thread(delegate() { _tracker.StartTracking(); });
            _trackingThread.Start();
            _tracker.BallUpdate += _tracker_BallUpdate;
        }

        void _tracker_BallUpdate(object sender, BlobUpdateEventArgs e)
        {
            _game.UpdateBall(e.MotionVector);
        }

        public void StopTracking()
        {
            Tracking = false;
            _tracker.StopTracking();
        }


        void _game_ObjectCollisionEvent(object sender, ObjectEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _game_RemoveObjectEvent(object sender, ObjectEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _game_NewObjectEvent(object sender, ObjectEventArgs e)
        {
            int newKey;
            while (_objects.ContainsKey(newKey = _rand.Next())) { }

            _objects.Add(newKey, e.Obj);
            _gameWindow.AddObject(e.Obj);
        }

        public void ToggleBoxes()
        {
            _tracker.DrawBoxes = !_tracker.DrawBoxes;
        }
    }
}
