using PuckControl.Domain;
using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PuckControl.Games
{
    public class SpeedChallenge : AbstractGame, IGame
    {
        private string AssemblyName = "PuckControl.Games";
        private HUDItem _scoreHUD;
        private HUDItem _countdownHUD;
        private GameStage _currentStage;
        private Random rand;
        private Timer _gameTimer;
        private Uri _bonusSoundUri;
        private Uri _buzzerSoundUri;
        private HUDItem _gameTimeHUD;
        private Vector3D _lastTarget;

        public SpeedChallenge() : base()
        {
            TileColor = Color.FromRgb(255, 0, 0);
            Name = "Speed Challenge";
        }

        public override GameStage CurrentStage
        {
            get { return _currentStage; }
            set
            {
                _currentStage = value;

                if (_currentStage == GameStage.GameOver)
                    PlayAudio(_buzzerSoundUri);

                base.CurrentStage = value;
            }
        }

        public override bool Init()
        {
            try
            {
                if (_gameTimer == null)
                    _gameTimer = new Timer();
                ControlType = ControlType.Absolute;
                rand = new Random();

                if (_bonusSoundUri == null)
                    _bonusSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/bonus.wav");
                if (_buzzerSoundUri == null)
                    _buzzerSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/buzzer.wav");

                AddRink();
                AddPuck();

                _scoreHUD = new HUDItem();
                _scoreHUD.DefaultValue = 0;
                _scoreHUD.HorizontalPosition = HorizontalAlignment.Right;
                _scoreHUD.VerticalPosition = VerticalAlignment.Top;
                _scoreHUD.ItemType = HUDItemType.Numeric;
                _scoreHUD.Name = "Score";
                _scoreHUD.Label = "Score:";

                _countdownHUD = new HUDItem();
                _countdownHUD.DefaultValue = 3;
                _countdownHUD.Value = 3;
                _countdownHUD.HorizontalPosition = HorizontalAlignment.Center;
                _countdownHUD.VerticalPosition = VerticalAlignment.Center;
                _countdownHUD.ItemType = HUDItemType.Numeric;
                _countdownHUD.Name = "Countdown";
                _countdownHUD.Size = 4;


                _gameTimeHUD = new HUDItem();
                _gameTimeHUD.HorizontalPosition = HorizontalAlignment.Center;
                _gameTimeHUD.VerticalPosition = VerticalAlignment.Top;
                _gameTimeHUD.DefaultValue = 30;
                _gameTimeHUD.Value = 30;
                _gameTimeHUD.Visible = false;
                _gameTimeHUD.ItemType = HUDItemType.Numeric;
                _gameTimeHUD.Name = "TimeRemaining";
                _gameTimeHUD.Size = 2;

                HUDItems.Add(_gameTimeHUD);
                HUDItems.Add(_countdownHUD);
                HUDItems.Add(_scoreHUD);

                return true;
            }
            catch (Exception)
            {
                if (_gameTimeHUD != null)
                    _gameTimeHUD.Dispose();
                if (_countdownHUD != null)
                    _countdownHUD.Dispose();
                if (_gameTimer != null)
                    _gameTimer.Dispose();
                if (_scoreHUD != null)
                    _scoreHUD.Dispose();
                
                throw;
            }
        }

        public override void StartGame()
        {
            _countdownHUD.Visible = true;

            CurrentStage = GameStage.Countdown;

            _gameTimer.Elapsed -= _gameTimer_Elapsed;
            _gameTimer.Elapsed += _gameTimer_Elapsed;

            _gameTimer.Interval = 1000;
            _gameTimer.Start();
        }

        public override void Collision(GameObject objectOne, GameObject objectTwo)
        {
            if (objectTwo == null || CurrentStage != GameStage.Playing || !objectTwo.Active)
                return;

            objectTwo.Active = false;

            _scoreHUD.Value += 1;
            PlayAudio(_bonusSoundUri);
            AddTarget();

            GameObjects.Remove(objectTwo);
        }

        public override int? Score
        {
            get
            {
                if (_scoreHUD.Value > 0)
                    return _scoreHUD.Value;

                return null;
            }
        }

        public void NewTarget(Vector3D position)
        {
            GameObject newTarget = new GameObject();
            var targetMaterial = new ModelMaterial()
            {
                TextureFile = "Target.3ds.jpg"
            };

            newTarget.ObjectType = "Target";
            newTarget.Active = true;
            newTarget.Position = position;
            newTarget.Model.ModelFile = "target.3ds";
            newTarget.Model.Materials.Add(targetMaterial);

            GameObjects.Add(newTarget);
        }

        void _gameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (CurrentStage)
            {
                case GameStage.Countdown: UpdateCountdown(); break;
                case GameStage.Playing: UpdateGameState(); break;
                case GameStage.GameOver: GameOver(); break;
            }
        }

        private void UpdateGameState()
        {
            _gameTimeHUD.Value--;

            if (_gameTimeHUD.Value <= 0)
            {
                _countdownHUD.Text = "GAME OVER!";
                _countdownHUD.Visible = true;
                _countdownHUD.ItemType = HUDItemType.Text;

                CurrentStage = GameStage.GameOver;
            }
        }

        private void GameOver()
        {
            _gameTimer.Stop();
            _gameTimer.Elapsed -= _gameTimer_Elapsed;
        }

        private void UpdateCountdown()
        {
            if (_countdownHUD.Value > 1)
            {
                _countdownHUD.Value -= 1;
            }
            else if (_countdownHUD.Value == 1)
            {
                _countdownHUD.Value = 0;
                _countdownHUD.ItemType = HUDItemType.Text;
                _countdownHUD.Text = "GO!";
                AddTarget();
                AddTarget();
            }
            else
            {
                _countdownHUD.Visible = false;
                _countdownHUD.Reset();
                CurrentStage = GameStage.Playing;
            }
        }

        private void AddTarget()
        {
            Vector3D newLocation;

            do
            {
                newLocation = new Vector3D(rand.Next(-80, 80), rand.Next(-80, 80), 0);
            } while ((_lastTarget - newLocation).Length < 30);
            _lastTarget = newLocation;

            NewTarget(newLocation);
            
        }
    }
}
