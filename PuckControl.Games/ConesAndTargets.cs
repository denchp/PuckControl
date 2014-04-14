using PuckControl.Domain;
using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

[assembly: CLSCompliant(true)]
namespace PuckControl.Games
{
    public class ConesAndTargets : AbstractGame, IGame
    {
        private HUDItem _scoreHUD;
        private HUDItem _countdownHUD;
        private HUDItem _livesHUD;
        private GameStage _currentStage;
        private Random rand;
        private Timer _gameTimer;

        private Uri _bonusSoundUri;
        private Uri _buzzerSoundUri;
        private string AssemblyName = "PuckControl.Games";
        
        public ConesAndTargets() : base()
        {
            TileColor = Color.FromRgb(0, 255, 0);
            Name = "Cones And Targets";
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
                _gameTimer = new Timer();
                ControlType = ControlType.Absolute;
                rand = new Random();

                _bonusSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/bonus.wav");
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
                _scoreHUD.Size = 2;

                _livesHUD = new HUDItem();
                _livesHUD.DefaultValue = 3;
                _livesHUD.Value = 3;
                _livesHUD.HorizontalPosition = HorizontalAlignment.Left;
                _livesHUD.VerticalPosition = VerticalAlignment.Top;
                _livesHUD.ItemType = HUDItemType.Numeric;
                _livesHUD.Name = "Lives";
                _livesHUD.Label = "Lives:";
                _livesHUD.Size = 2;

                _countdownHUD = new HUDItem();
                _countdownHUD.DefaultValue = 3;
                _countdownHUD.Value = 3;
                _countdownHUD.HorizontalPosition = HorizontalAlignment.Center;
                _countdownHUD.VerticalPosition = VerticalAlignment.Center;
                _countdownHUD.ItemType = HUDItemType.Numeric;
                _countdownHUD.Name = "Countdown";
                _countdownHUD.Size = 3;
                

                HUDItems.Add(_livesHUD);
                HUDItems.Add(_countdownHUD);
                HUDItems.Add(_scoreHUD);

                return true;
            }
            catch (Exception)
            {
                if (_livesHUD != null)
                    _livesHUD.Dispose();
                if (_countdownHUD != null)
                    _countdownHUD.Dispose();
                if (_gameTimer != null)
                    _gameTimer.Dispose();
                if (_scoreHUD != null)
                    _scoreHUD.Dispose();

                throw;
            }
        }

        public override void Collision(GameObject obj, GameObject obj2)
        {
            if (obj2 == null || CurrentStage != GameStage.Playing || !obj2.Active)
                return;

            obj2.Active = false;

            switch (obj.ObjectType)
            {
                case "Cone":
                    _livesHUD.Value -= 1;
                    PlayAudio(_buzzerSoundUri);
                    if (_livesHUD.Value == 0)
                    {
                        CurrentStage = GameStage.GameOver;
                    }
                    break;

                case "Target":
                    _scoreHUD.Value += 1;
                    PlayAudio(_bonusSoundUri);
                    AddPairing();
                    break;
            }

            GameObjects.Remove(obj2);
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

        public override int? Score
        {
            get
            {
                if (_scoreHUD.Value > 0)
                    return _scoreHUD.Value;

                return null;
            }
        }

        private void NewCone(Vector3D position)
        {
            GameObject newCone = new GameObject();

            newCone.ObjectType = "Cone";
            newCone.Active = true;
            newCone.Position = position;
            newCone.Model.ModelFile = "cone_highdef.3ds";

            GameObjects.Add(newCone);
        }

        private void NewTarget(Vector3D position)
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

        private void _gameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (CurrentStage)
            {
                case GameStage.Countdown: UpdateCountdown(); break;
                case GameStage.Playing: break;
                case GameStage.GameOver: GameOver(); break;
            }
        }

        private void GameOver()
        {
            PlayAudio(_buzzerSoundUri);

            _countdownHUD.Text = "GAME OVER!";
            _countdownHUD.Visible = true;
            _countdownHUD.ItemType = HUDItemType.Text;

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
                _countdownHUD.Text = "GO!";
                _countdownHUD.ItemType = HUDItemType.Text;
                NewCone(new Vector3D(5, 5, 0));
                NewTarget(new Vector3D(80, 5, 0));
            }
            else
            {
                _countdownHUD.Visible = false;
                _countdownHUD.Reset();
                CurrentStage = GameStage.Playing;
            }
        }

        private void AddPairing()
        {
            Vector3D newLocation;
            bool tooClose = false;

            do{
                newLocation = new Vector3D(rand.Next(-80, 80), rand.Next(-80, 80), 0);
                tooClose = GameObjects.Where(x => (x.Position - newLocation).Length < 20).Count() > 0;
            }
            while (tooClose);

            NewCone(newLocation);
            Int32 XOffset = rand.Next(-25, 25);
            Int32 YOffset = (Int32)Math.Sqrt(Math.Abs(500 - (XOffset * XOffset)));
            NewTarget(newLocation - new Vector3D(XOffset, YOffset, 0));
        }
    }
}

