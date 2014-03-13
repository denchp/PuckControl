using KwikHands.Domain.EventArg;
using KwikHands.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KwikHands.Domain
{
    public class KwikEngine
    {
        public event EventHandler<ImageEventArgs> NewCameraImage;
        public event EventHandler<ImageEventArgs> NewTrackingImage;
        public event EventHandler<ObjectEventArgs> NewObject;
        public event EventHandler<ObjectEventArgs> ObjectMotion;
        public event EventHandler<HudItemEventArgs> NewHudItem;
        public event EventHandler<HudItemEventArgs> UpdateHudItem;
        public event EventHandler<MediaEventArgs> PlayMedia;
        public event EventHandler<ObjectEventArgs> RemoveObject;
        public bool Tracking = false;
        public double MaxSpeed = 4;
        public double ControlDeadZone = 1;

        private List<GameObject> _objects = new List<GameObject>();
        private IGame _game;
        private Random _rand = new Random();
        private BallTracker _tracker = new BallTracker();
        private Thread _trackingThread;

        public void Init()
        {
            _tracker.NewCameraImage += _tracker_NewCameraImages;
        }

        public void StartGame()
        {
            _game.StartGame();
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
            _game.ObjectMotionEvent += _game_ObjectMotionEvent;
            _game.NewHudItemEvent += _game_NewHudItemEvent;
            _game.UpdateHudItemEvent +=_game_UpdateHudItemEvent;
            _game.MediaEvent += _game_MediaEvent;
            _game.GameStageChange += _game_GameStageChange;
            _game.Init();
        }

        void _game_GameStageChange(object sender, GameStages e)
        {
            if (e == GameStages.Playing || e == GameStages.Countdown)
                this.StartTracking();
            else
                this.StopTracking();
        }

        void _game_MediaEvent(object sender, MediaEventArgs e)
        {
            if (PlayMedia != null)
                PlayMedia(this, e);
        }

        public void PuckCollision(GameObject obj)
        {
            _game.PuckCollision(obj);
        }

        void _game_UpdateHudItemEvent(object sender, HudItemEventArgs e)
        {
            if (this.UpdateHudItem != null)
            {
                UpdateHudItem(this, e);
            }
        }

        void _game_NewHudItemEvent(object sender, HudItemEventArgs e)
        {
            if (this.NewHudItem != null)
            {
                NewHudItem(this, e);
            }
        }

        void _game_ObjectMotionEvent(object sender, ObjectEventArgs e)
        {
            if (this.ObjectMotion != null)
                ObjectMotion(this, e);
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
            if (ObjectMotion == null)
                return;
            try
            {
                foreach (GameObject obj in _objects.Where(x => x.Type == ObjectType.Puck))
                {
                    Vector3D motionVector = e.PositionVector - obj.Position;
                    if (motionVector.Length < this.ControlDeadZone)
                        continue;

                    switch (_game.ControlType)
                    {
                        case ControlTypeEnum.Relative:
                            if (motionVector.Length > MaxSpeed)
                                motionVector = motionVector * (MaxSpeed / motionVector.Length);

                            obj.Position = obj.Position + motionVector;
                            ObjectMotion(this, new ObjectEventArgs(obj, obj.Type));
                            break;

                        default:
                            obj.Position = obj.Position + motionVector;
                            ObjectMotion(this, new ObjectEventArgs(obj, obj.Type));
                            break;
                    }
                }
            }
            catch (Exception ex) { }
        }

        public void StopTracking()
        {
            Tracking = false;
            _tracker.StopTracking();
        }

        void _game_RemoveObjectEvent(object sender, ObjectEventArgs e)
        {
            if (RemoveObject != null)
                RemoveObject(this, e);
        }

        void _game_NewObjectEvent(object sender, ObjectEventArgs e)
        {
            _objects.Add(e.Obj);
            if (NewObject != null)
                NewObject(this, e);
        }

        public void ToggleBoxes()
        {
            _tracker.DrawBoxes = !_tracker.DrawBoxes;
        }

        public void ForceTrackingUpdate(int XPosition, int YPosition)
        {
            _tracker_BallUpdate(this, new BlobUpdateEventArgs() { BlobId = 0, PositionVector = new Vector3D(XPosition, YPosition, 0) });
        }
    }
}
