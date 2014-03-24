using KwikHands.Domain;
using KwikHands.Domain.EventArg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace KwikHands.Games
{
    public class SpeedChallenge : IGame
    {
        GameObject _puck = new GameObject();
        GameObject _rink = new GameObject();

        public event EventHandler<ObjectEventArgs> NewObjectEvent;
        public event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        public event EventHandler<ObjectEventArgs> ObjectCollisionEvent;
        public event EventHandler<ObjectEventArgs> ObjectMotionEvent;
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
        private GameStages _currentStage;
        private Random rand;
        private Timer _gameTimer;

        private Uri _bonusSoundUri;
        private Uri _buzzerSoundUri;
        private string AssemblyName = "KwikHands.SpeedChallenge";
        private HudItem _gameTimeHud;

        public SpeedChallenge()
        {
            _gameTimer = new Timer();
            _countdownHud = new HudItem();
            ControlType = ControlTypeEnum.Absolute;
            rand = new Random();
        }

        
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

            _scoreHud = new HudItem()
            {
                DefaultValue = 0,
                HorizontalPosition = HudItem.HorizontalAlignment.Right,
                VerticalPosition = HudItem.VerticalAlignment.Top,
                Type = HudItem.HudItemType.Numeric,
                Name = "Score",
                Label = "Score:",
            };


            _gameTimeHud = new HudItem()
            {
                HorizontalPosition = HudItem.HorizontalAlignment.Center,
                VerticalPosition = HudItem.VerticalAlignment.Top,
                DefaultValue = 30,
                Value = 30,
                Visible = false,
                Type = HudItem.HudItemType.Numeric,
                Name = "TimeRemaining",
            };

            _countdownHud = new HudItem()
            {
                DefaultValue = 4,
                Value = 4,
                HorizontalPosition = HudItem.HorizontalAlignment.Center,
                VerticalPosition = HudItem.VerticalAlignment.Middle,
                Type = HudItem.HudItemType.Numeric,
                Name = "Countdown",
            };

            _hudItems.Add(_gameTimeHud);
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
            UpdateInterface();
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
                case GameStages.Playing: UpdateGameState(); break;
                case GameStages.GameOver: GameOver(); break;
            }
        }

        private void UpdateGameState()
        {
            _gameTimeHud.Value--;
            UpdateInterface();

            if (_gameTimeHud.Value == 0)
            {
                _countdownHud.Text = "GAME OVER!";
                _countdownHud.Visible = true;
                _countdownHud.Type = HudItem.HudItemType.Text;
                UpdateHudItemEvent(this, new HudItemEventArgs() { Item = _countdownHud });
                CurrentStage = GameStages.GameOver;
            }
        }

        private void GameOver()
        {
            foreach (var item in _hudItems)
            {
                item.Reset();
            }
        }

        private void UpdateInterface()
        {
            if (UpdateHudItemEvent == null)
                return;

            foreach (var hudItem in _hudItems)
            {
                if (hudItem.Changed)
                    UpdateHudItemEvent(this, new HudItemEventArgs() { Item = hudItem });
            }
        }

        private void UpdateCountdown()
        {
            if (_countdownHud.Value > 1)
            {
                _countdownHud.Value -= 1;
                UpdateInterface();
            }
            else if (_countdownHud.Value == 1)
            {
                _countdownHud.Value = 0;
                _countdownHud.Text = "GO!";
                _countdownHud.Type = HudItem.HudItemType.Text;
                AddTarget();
                
                UpdateInterface();
            }
            else
            {
                _countdownHud.Visible = false;
                _countdownHud.Reset();
                CurrentStage = GameStages.Playing;
                UpdateInterface();
            }
        }

        public void PuckCollision(GameObject obj)
        {
            if (CurrentStage != GameStages.Playing || !obj.Active)
                return;

            obj.Active = false;

            _scoreHud.Value += 1;
            PlayAudio(_bonusSoundUri);
            AddTarget();

            if (RemoveObjectEvent != null)
                RemoveObjectEvent(this, new ObjectEventArgs(obj, obj.Type));

            UpdateInterface();
        }

        private void PlayAudio(Uri _soundUri)
        {
            if (MediaEvent != null)
                MediaEvent(this, new MediaEventArgs() { MediaFile = _soundUri });
        }

        private void AddTarget()
        {
            if (this.NewObjectEvent == null)
                return;

            Vector3D newLocation = new Vector3D(rand.Next(-95, 95), rand.Next(-95, 95), 0);
            NewTarget(newLocation);
        }

    }
}
