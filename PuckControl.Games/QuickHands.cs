using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using PuckControl.Domain;

using System;
using System.Timers;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;

namespace PuckControl.Games
{
    public class QuickHands : AbstractGame, IGame
    {
        private string AssemblyName = "PuckControl.Games";
        private Timer _gameTimer;
        private Uri _buzzerSoundUri;
        private HUDItem _horizontalScore;
        private HUDItem _verticalScore;
        private HUDItem _movingScore;
        private HUDItem _totalScore;
        private HUDItem _timerHUD;
        private HUDItem _countdown;
        private double _separation;
        private Timer _movementTimer;
        private GameLevel _level;
        private GameObject _leftBar;
        private GameObject _rightBar;
        private GameObject _centerBar;
        private Vector3D _leftPosition;
        private Vector3D _rightPosition;
        private List<GameObject> ActivationOrder;
        private int _lastActiveIndex;

        private bool _paused;
        
        private enum GameLevel
        {
            Horizontal, Vertical, TPattern
        }

        public QuickHands() : base()
        {
            TileColor = Color.FromRgb(255, 255, 0);
            Name = "Quick Hands";
        }
        public override bool Init()
        {
            try
            {
                _separation = .5;
                _rightPosition = new Vector3D(-1, 0, 0);
                _leftPosition = new Vector3D(-5, 0, 0);

                if (_movementTimer == null)
                    _movementTimer = new Timer();

                if (_gameTimer == null)
                    _gameTimer = new Timer();

                ControlType = ControlType.Absolute;
                
                if (_buzzerSoundUri == null)
                    _buzzerSoundUri = new Uri("pack://application:,,,/" + AssemblyName + ";component/audio/buzzer.wav");

                AddRink();
                AddPuck();

                _horizontalScore = new HUDItem();
                _horizontalScore.DefaultValue = 0;
                _horizontalScore.HorizontalPosition = HorizontalAlignment.Left;
                _horizontalScore.VerticalPosition = VerticalAlignment.Top;
                _horizontalScore.ItemType = HUDItemType.Numeric;
                _horizontalScore.Name = "Horizontal";
                _horizontalScore.Label = "L/R:";

                _verticalScore = new HUDItem();
                _verticalScore.DefaultValue = 0;
                _verticalScore.HorizontalPosition = HorizontalAlignment.Center;
                _verticalScore.VerticalPosition = VerticalAlignment.Top;
                _verticalScore.ItemType = HUDItemType.Numeric;
                _verticalScore.Name = "Vertical";
                _verticalScore.Label = "F/B:";

                _movingScore = new HUDItem();
                _movingScore.DefaultValue = 0;
                _movingScore.HorizontalPosition = HorizontalAlignment.Right;
                _movingScore.VerticalPosition = VerticalAlignment.Top;
                _movingScore.ItemType = HUDItemType.Numeric;
                _movingScore.Name = "TPattern";
                _movingScore.Label = "T:";

                _totalScore = new HUDItem();
                _totalScore.DefaultValue = 0;
                _totalScore.HorizontalPosition = HorizontalAlignment.Center;
                _totalScore.VerticalPosition = VerticalAlignment.Bottom;
                _totalScore.ItemType = HUDItemType.Numeric;
                _totalScore.Name = "Total";
                _totalScore.Label = "Total:";

                _countdown = new HUDItem();
                _countdown.DefaultValue = 3;
                _countdown.Value = 3;
                _countdown.HorizontalPosition = HorizontalAlignment.Center;
                _countdown.VerticalPosition = VerticalAlignment.Center;
                _countdown.ItemType = HUDItemType.Numeric;
                _countdown.Name = "Countdown";
                _countdown.Size = 4;

                _timerHUD = new HUDItem();
                _timerHUD.HorizontalPosition = HorizontalAlignment.Right;
                _timerHUD.VerticalPosition = VerticalAlignment.Bottom;
                _timerHUD.DefaultValue = 10;
                _timerHUD.Value = 10;
                _timerHUD.Visible = false;
                _timerHUD.ItemType = HUDItemType.Numeric;
                _timerHUD.Name = "TimeRemaining";
                _timerHUD.Size = 2;

                HUDItems.Add(_horizontalScore);
                HUDItems.Add(_verticalScore);
                HUDItems.Add(_movingScore);
                HUDItems.Add(_totalScore);
                HUDItems.Add(_countdown);
                HUDItems.Add(_timerHUD);

                _leftBar = CreateBar(_leftPosition);
                GameObjects.Add(_leftBar);

                _rightBar = CreateBar(_rightPosition);
                GameObjects.Add(_rightBar);

                _centerBar = CreateBar(new Vector3D(-3, -5, 0));

                ActivationOrder = new List<GameObject>();
                ActivationOrder.Add(_leftBar);
                ActivationOrder.Add(_rightBar);

                _rightBar.Active = false;
                _leftBar.Active = false;

                _level = GameLevel.Horizontal;
                return true;
            }
            catch (Exception)
            {
                if (_horizontalScore != null)
                    _horizontalScore.Dispose();

                if (_verticalScore != null)
                    _verticalScore.Dispose();

                if (_gameTimer != null)
                    _gameTimer.Dispose();

                if (_movingScore != null)
                    _movingScore.Dispose();

                if (_countdown != null)
                    _countdown.Dispose();

                if (_timerHUD != null)
                    _timerHUD.Dispose();

                if (_movementTimer != null)
                    _movementTimer.Dispose();

                throw;
            }
        }

        private GameObject CreateBar(Vector3D position)
        {
            var activeMaterial = new ModelMaterial();
            activeMaterial.DiffuseColor = Color.FromRgb(0, 255, 0);

            var inactiveMaterial = new ModelMaterial();
            inactiveMaterial.DiffuseColor = Color.FromRgb(255, 0, 0);

            var bar = new GameObject();
            bar.Position = position;
            bar.TrackCollisions = true;
            bar.MotionSmoothingSteps = 2;
            bar.ControlledObject = false;
            bar.Model.ModelFile = "RedBar.3ds";
            bar.Rotation = new Vector3D(0, 0, 90);
            bar.Scale = new Vector3D(4, 4, 1);
            bar.Model.ActiveMaterial = activeMaterial;
            bar.Model.InactiveMaterial = inactiveMaterial;
            bar.ObjectType = "Bar";

            return bar;
        }

        public override void StartGame()
        {
            _countdown.Visible = true;

            CurrentStage = GameStage.Countdown;

            _movementTimer.Elapsed -= _movementTimer_Elapsed;
            _movementTimer.Elapsed += _movementTimer_Elapsed;

            _movementTimer.Interval = 200;

            _gameTimer.Elapsed -= _gameTimer_Elapsed;
            _gameTimer.Elapsed += _gameTimer_Elapsed;

            _gameTimer.Interval = 1000;
            _gameTimer.Start();
        }

        void _movementTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (_level)
            {
                case GameLevel.Horizontal:
                    _leftBar.Rotation = new Vector3D();
                    _rightBar.Rotation = new Vector3D();
                    break;
                case GameLevel.Vertical:
                    break;
                case GameLevel.TPattern:
                    break;
            }

            if (_level != GameLevel.TPattern)
            {
                _rightPosition -= new Vector3D(_separation, 0, 0);
                _leftPosition += new Vector3D(_separation, 0, 0);

                _rightBar.Position = _rightPosition;
                _leftBar.Position = _leftPosition;
            }
        }

        private void _gameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch (CurrentStage)
            {
                case GameStage.Countdown: UpdateCountdown(); break;
                case GameStage.Playing: UpdateGameState(); break;
                case GameStage.GameOver: GameOver(); break;
            }
        }

        private void GameOver()
        {
            _movementTimer.Stop();
            _gameTimer.Stop();
            PlayAudio(_buzzerSoundUri);
        }

        private void UpdateGameState()
        {
            if (_paused)
                return;

            _timerHUD.Value -= 1;

            if (_timerHUD.Value <= 0)
                AdvanceLevel();
        }

        private void AdvanceLevel()
        {
            _paused = true;
            PlayAudio(_buzzerSoundUri);
            _movementTimer.Stop();
            _lastActiveIndex = 0;
            _rightPosition = new Vector3D(-1, 0, 0);
            _leftPosition = new Vector3D(-5, 0, 0);

            if (_level == GameLevel.TPattern)
                CurrentStage = GameStage.GameOver;
            else
            {
                _level++;
                _timerHUD.Reset();
                _countdown.ItemType = HUDItemType.Numeric;
                _countdown.Reset();
                _countdown.Visible = true;
                CurrentStage = GameStage.Countdown;
            }

            if (CurrentStage != GameStage.GameOver)
            {
                switch (_level)
                {
                    case GameLevel.Vertical:
                        _rightBar.Position = _rightPosition;
                        _leftBar.Position = _leftPosition;
                        _leftBar.Rotation = new Vector3D(0, 0, 90);
                        _rightBar.Rotation = new Vector3D(0, 0, 90);
                        break;
                    case GameLevel.TPattern:
                        _leftBar.Position = _leftPosition + new Vector3D(-25, 230, 0);
                        _rightBar.Position = _rightPosition + new Vector3D(0, -90, 0);
                        _leftBar.Rotation = new Vector3D(0, 0, 90);
                        _rightBar.Rotation = new Vector3D(0, 0, 90);
                        ActivationOrder.Clear();
                        ActivationOrder = new List<GameObject>() { _centerBar, _rightBar, _centerBar, _leftBar };

                        GameObjects.Add(_centerBar);
                        break;
                }

                ActivationOrder.ForEach(x => x.Active = false);
                ActivationOrder[_lastActiveIndex].Active = true;
            }
        }

        private void UpdateCountdown()
        {
            if (_countdown.Value > 1)
            {
                _countdown.Value -= 1;
            }
            else if (_countdown.Value == 1)
            {
                _countdown.Value = 0;
                _countdown.ItemType = HUDItemType.Text;
                _countdown.Text = "GO!";
                _movementTimer.Start();
                ActivationOrder[0].Active = true;
                _paused = false;
            }
            else
            {
                _countdown.Visible = false;
                _countdown.Reset();
                _paused = false;
                CurrentStage = GameStage.Playing;
            }
        }

        public override void Collision(GameObject objectOne, GameObject objectTwo)
        {
            if (objectOne == null || objectTwo == null)
                return;

            if (objectOne.ObjectType != "Puck" || !objectTwo.Active || _paused)
                return;

            _lastActiveIndex++;
            if (_lastActiveIndex >= ActivationOrder.Count)
                _lastActiveIndex = 0;

            ActivationOrder[_lastActiveIndex].Active = true;
            objectTwo.Active = false;

            switch (_level)
            {
                case GameLevel.Horizontal:
                    _horizontalScore.Value += 1;
                    break;
                case GameLevel.Vertical:
                    _verticalScore.Value += 1;
                    break;
                case GameLevel.TPattern:
                    _movingScore.Value += 1;
                    break;
            }

            _totalScore.Value += 1;
        }

        public override int? Score
        {
            get
            {
                if (_totalScore.Value > 0)
                    return _totalScore.Value;

                return null;
            }
        }
    }
}
