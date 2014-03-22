using KwikHands.Domain;
using KwikHands.Domain.EventArg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Timers;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Linq;
using KwikHands.Domain.Entities;

namespace KwikHands.Games
{
    public class ConesAndTargets : IGame
    {
        GameObject _puck = new GameObject();
        GameObject _rink = new GameObject();

        public event EventHandler<ObjectEventArgs> NewObjectEvent;
        public event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        public event EventHandler<ObjectEventArgs> ObjectCollisionEvent;
        public event EventHandler<HudItemEventArgs> NewHudItemEvent;
        public event EventHandler<HudItemEventArgs> UpdateHudItemEvent;
        public event EventHandler<MediaEventArgs> MediaEvent;
        public event EventHandler<GameStageEventArgs> GameStageChange;

        private List<GameObject> _gameObjects = new List<GameObject>();
        private List<HudItem> _hudItems = new List<HudItem>();
        private HudItem _timerHud;
        private HudItem _scoreHud;
        private HudItem _countdownHud;
        private HudItem _livesHud;
        
        private Random rand;
        private Timer _gameTimer;

        private Uri _bonusSoundUri;
        private Uri _buzzerSoundUri;
        private string AssemblyName = "KwikHands.Games";


        public ConesAndTargets()
        {
            _gameTimer = new Timer();
            _countdownHud = new HudItem();
            ControlType = ControlTypeEnum.Absolute;
            rand = new Random();
        }

        private GameStages _currentStage;
        public GameStages CurrentStage
        {
            get { return _currentStage; }
            set
            {
                _currentStage = value;
                if (GameStageChange != null) 
                {
                    GameStageChange(this, new GameStageEventArgs() { Stage = value });
                }
            }
        }

        public ControlTypeEnum ControlType { get; set; }

        public bool Init()
        {
            BitmapImage TextureImage = new BitmapImage();

            _bonusSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/bonus.wav");
            _buzzerSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/buzzer.wav");

            _puck.Position = new Vector3D(20, 20, 0);
            _puck.Type = ObjectType.Puck;
            _puck.MotionSmoothingSteps = 2;

            _rink.Type = ObjectType.Rink;
            _rink.ApplyPhysics = false;

            _gameObjects.Add(_rink);
            _gameObjects.Add(_puck);

            _timerHud = new HudItem()
            {
                DefaultValue = 5,
                HorizontalPosition = HudItem.HorizontalAlignment.Center,
                VerticalPosition = HudItem.VerticalAlignment.Top,
                MinValue = 0,
                Type = HudItem.HudItemType.Timer,

                Name = "Timer",
                MinumumTrigger = true
            };
            _timerHud.Changed += UpdateInterface;

            _scoreHud = new HudItem()
            {
                DefaultValue = 0,
                HorizontalPosition = HudItem.HorizontalAlignment.Right,
                VerticalPosition = HudItem.VerticalAlignment.Top,
                Type = HudItem.HudItemType.Numeric,
                Name = "Score",
                Label = "Score:",
            };
            _scoreHud.Changed += UpdateInterface;

            _livesHud = new HudItem()
            {
                DefaultValue = 3,
                Value = 3,
                HorizontalPosition = HudItem.HorizontalAlignment.Left,
                VerticalPosition = HudItem.VerticalAlignment.Top,
                Type = HudItem.HudItemType.Numeric,
                Name = "Lives",
                Label = "Lives:",
            };
            _livesHud.Changed += UpdateInterface;

            _countdownHud = new HudItem()
            {
                DefaultValue = 4,
                Value = 4,
                HorizontalPosition = HudItem.HorizontalAlignment.Center,
                VerticalPosition = HudItem.VerticalAlignment.Middle,
                Type = HudItem.HudItemType.Numeric,
                Name = "Countdown",
            };
            _countdownHud.Changed += UpdateInterface;

            _hudItems.Add(_livesHud);
            _hudItems.Add(_countdownHud);
            _hudItems.Add(_scoreHud);
            _hudItems.Add(_timerHud);

            if (NewHudItemEvent != null)
            {
                var args = new HudItemEventArgs();

                foreach (var item in _hudItems)
                {
                    args.Item = item;
                    NewHudItemEvent(this, args);
                }
            }

            if (NewObjectEvent != null)
            {
                var args = new ObjectEventArgs();

                foreach (var obj in _gameObjects)
                {
                    args.Obj = obj;
                    args.ObjType = obj.Type;
                    NewObjectEvent(this, args);
                }
            }

            return true;
        }

        public void NewCone(Vector3D position)
        {
            GameObject newCone = new GameObject();

            newCone.Type = ObjectType.Cone;
            newCone.ID = "Cone_" + _gameObjects.Where(x => x.Type == ObjectType.Cone).Count();
            newCone.Active = true;
            newCone.Position = position;

            _gameObjects.Add(newCone);

            if (NewObjectEvent != null)
                NewObjectEvent(this, new ObjectEventArgs(newCone, ObjectType.Cone));
        }

        public void NewTarget(Vector3D position)
        {
            GameObject newTarget = new GameObject();

            newTarget.Type = ObjectType.Target;
            newTarget.ID = "Target_" + _gameObjects.Where(x => x.Type == ObjectType.Target).Count();
            newTarget.Active = true;
            newTarget.Position = position;

            _gameObjects.Add(newTarget);

            if (NewObjectEvent != null)
                NewObjectEvent(this, new ObjectEventArgs(newTarget, ObjectType.Target));
        }

        public void StartGame()
        {
            _countdownHud.Visible = true;
            CurrentStage = GameStages.Countdown;

            _gameTimer.Elapsed += _gameTimer_Elapsed;
            _gameTimer.Interval = 1000;
            _gameTimer.Start();
        }

        void _gameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (CurrentStage)
            {
                case GameStages.Countdown: UpdateCountdown(); break;
                case GameStages.Playing: break;
                case GameStages.GameOver: GameOver(); break;
            }
        }

        private void GameOver()
        {
            throw new NotImplementedException();
        }

        private void UpdateInterface(object sender, EventArgs args)
        {
            HudItem hudItem = sender as HudItem;
            if (UpdateHudItemEvent != null)
                UpdateHudItemEvent(this, new HudItemEventArgs() { Item = hudItem });
        }

        private void UpdateCountdown()
        {
            if (_countdownHud.Value > 1)
            {
                _countdownHud.Value -= 1;
            }
            else if (_countdownHud.Value == 1)
            {
                _countdownHud.Value = 0;
                _countdownHud.Text = "GO!";
                _countdownHud.Type = HudItem.HudItemType.Text;
                NewCone(new Vector3D(5, 5, 0));
                NewTarget(new Vector3D(80, 5, 0));
            }
            else
            {
                _countdownHud.Visible = false;
                _countdownHud.Reset();
                CurrentStage = GameStages.Playing;
            }
        }

        public void PuckCollision(GameObject obj)
        {
            if (CurrentStage != GameStages.Playing || !obj.Active)
                return;

            obj.Active = false;

            switch (obj.Type)
            {
                case ObjectType.Cone:
                    _livesHud.Value -= 1;
                    PlayAudio(_buzzerSoundUri);
                    break;

                case ObjectType.Target:
                    _scoreHud.Value += 1;
                    PlayAudio(_bonusSoundUri);
                    AddPairing();
                    break;
            }

            if (RemoveObjectEvent != null)
                RemoveObjectEvent(this, new ObjectEventArgs(obj, obj.Type));
        }

        private void PlayAudio(Uri _buzzerSoundUri)
        {
            if (MediaEvent != null)
                MediaEvent(this, new MediaEventArgs() { MediaFile = _buzzerSoundUri });
        }

        private void AddPairing()
        {
            if (this.NewObjectEvent == null)
                return;

            Vector3D newLocation;
            bool tooClose = false;

            do{
                newLocation = new Vector3D(rand.Next(-80, 80), rand.Next(-80, 80), 0);
                tooClose = _gameObjects.Where(x => (x.Position - newLocation).Length < 20).Count() > 0;
            }
            while (tooClose);

            NewCone(newLocation);
            Int32 XOffset = rand.Next(-25, 25);
            Int32 YOffset = (Int32)Math.Sqrt(Math.Abs(500 - (XOffset * XOffset)));
            NewTarget(newLocation - new Vector3D(XOffset, YOffset, 0));
        }

    }
}

