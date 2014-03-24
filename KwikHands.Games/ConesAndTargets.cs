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
    public class ConesAndTargets : AbstractGame, IGame
    {

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
            TileColor = Color.FromRgb(0, 255, 0);
            Name = "Cones And Targets";
        }

        public override GameStages CurrentStage
        {
            get { return _currentStage; }
            set
            {
                _currentStage = value;

                if (_currentStage == GameStages.GameOver)
                    PlayAudio(_buzzerSoundUri);

                base.CurrentStage = value;
            }
        }

        public override bool Init()
        {
            _gameTimer = new Timer();
            _countdownHud = new HudItem();
            ControlType = ControlTypeEnum.Absolute;
            rand = new Random();

            BitmapImage TextureImage = new BitmapImage();

            _bonusSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/bonus.wav");
            _buzzerSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/buzzer.wav");
            
            AddRink();
            AddPuck();

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
            
            _countdownHud = new HudItem()
            {
                DefaultValue = 4,
                Value = 4,
                HorizontalPosition = HudItem.HorizontalAlignment.Center,
                VerticalPosition = HudItem.VerticalAlignment.Middle,
                Type = HudItem.HudItemType.Numeric,
                Name = "Countdown",
            };

            _hudItems.Add(_livesHud);
            _hudItems.Add(_countdownHud);
            _hudItems.Add(_scoreHud);
            _hudItems.Add(_timerHud);

            return true;
        }

        public override void PuckCollision(GameObject obj)
        {
            if (CurrentStage != GameStages.Playing || !obj.Active)
                return;

            obj.Active = false;

            switch (obj.Type)
            {
                case "Cone":
                    _livesHud.Value -= 1;
                    PlayAudio(_buzzerSoundUri);
                    if (_livesHud.Value == 0)
                    {
                        _countdownHud.Text = "GAME OVER!";
                        _countdownHud.Visible = true;
                        _countdownHud.Type = HudItem.HudItemType.Text;

                        CurrentStage = GameStages.GameOver;
                    }
                    break;

                case "Target":
                    _scoreHud.Value += 1;
                    PlayAudio(_bonusSoundUri);
                    AddPairing();
                    break;
            }

            _gameObjects.Remove(obj);
        }

        public override void StartGame()
        {
            _countdownHud.Visible = true;
            CurrentStage = GameStages.Countdown;

            _gameTimer.Elapsed += _gameTimer_Elapsed;
            _gameTimer.Interval = 1000;
            _gameTimer.Start();
        }

        public override int GetScore()
        {
            return _scoreHud.Value;
        }

        private void NewCone(Vector3D position)
        {
            GameObject newCone = new GameObject();

            newCone.Type = "Cone";
            newCone.Active = true;
            newCone.Position = position;
            newCone.Model.ModelFile = "cone_highdef.3ds";

            _gameObjects.Add(newCone);
        }

        private void NewTarget(Vector3D position)
        {
            GameObject newTarget = new GameObject();
            var targetMaterial = new ModelMaterial()
            {
                TextureFile = "Target.3ds.jpg"
            };

            newTarget.Type = "Target";
            newTarget.Active = true;
            newTarget.Position = position;
            newTarget.Model.ModelFile = "target.3ds";
            newTarget.Model.Materials.Add(targetMaterial);

            _gameObjects.Add(newTarget);
        }

        private void _gameTimer_Elapsed(object sender, ElapsedEventArgs e)
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
            _gameTimer.Stop();
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

        private void AddPairing()
        {
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

