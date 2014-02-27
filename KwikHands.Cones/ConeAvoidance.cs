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

namespace KwikHands.Cones
{
    public class ConeAvoidance : IGame
    {
        GameObject _cone = new GameObject();
        GameObject _puck = new GameObject();
        GameObject _rink = new GameObject();

        public event EventHandler<ObjectEventArgs> NewObjectEvent;
        public event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        public event EventHandler<ObjectEventArgs> ObjectCollisionEvent;
        public event EventHandler<ObjectEventArgs> ObjectMotionEvent;
        public event EventHandler<HudItemEventArgs> NewHudItemEvent;
        public event EventHandler<HudItemEventArgs> UpdateHudItemEvent;

        private List<GameObject> _gameObjects = new List<GameObject>();
        private List<HudItem> _hudItems = new List<HudItem>();
        private HudItem _timerHud;
        private HudItem _scoreHud;
        private HudItem _countdownHud;
        private HudItem _livesHud;

        private Timer _gameTimer;

        public enum GameStages
        {
            Countdown,
            Playing,
            GameOver
        }

        public ConeAvoidance()
        {
            _gameTimer = new Timer();
            _countdownHud = new HudItem();
        }

        public GameStages CurrentStage { get; set; }

        public bool Init()
        {
            string AssemblyName = "KwikHands.Cones";
            BitmapImage TextureImage = new BitmapImage();
            
            var info = Application.GetResourceStream(new Uri("pack://application:,,,/" + AssemblyName + ";component/models/Cone.xaml"));
            _cone.Model = (ModelVisual3D)XamlReader.Load(info.Stream);
            _cone.Type = ObjectType.Cone;
            _cone.ID = "Cone_" + _gameObjects.Where(x => x.Type == ObjectType.Cone).Count();
            _cone.Active = true;

            info = Application.GetResourceStream(new Uri("pack://application:,,,/" + AssemblyName + ";component/models/Puck.xaml"));
            _puck.Model = (ModelVisual3D)XamlReader.Load(info.Stream);
            _puck.Position = new Vector3D(-6, -4, 0);
            _puck.Type = ObjectType.Puck;

            info = Application.GetResourceStream(new Uri("pack://application:,,,/" + AssemblyName + ";component/models/rink.xaml"));
            _rink.Model = (ModelVisual3D)XamlReader.Load(info.Stream);
            _rink.Type = ObjectType.Rink;

            _rink.ApplyPhysics = false;

            _gameObjects.Add(_cone);
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

            if (NewHudItemEvent != null)
            {
                var args = new HudItemEventArgs();
                args.Item = _timerHud;
                NewHudItemEvent(this, args);

                args.Item = _scoreHud;
                NewHudItemEvent(this, args);

                args.Item = _countdownHud;
                NewHudItemEvent(this, args);

                args.Item = _livesHud;
                NewHudItemEvent(this, args);
            }

            if (NewObjectEvent != null)
            {
                var args = new ObjectEventArgs(_rink);
                NewObjectEvent(this, args);

                args.Obj = _cone;
                NewObjectEvent(this, args);

                args.Obj = _puck;
                NewObjectEvent(this, args);
            }
            return true;
        }

        public void UpdateBall(Vector3D motionVector)
        {
            if (this.ObjectMotionEvent != null)
            {
                _puck.Position = motionVector / 2;
                
                var args = new ObjectEventArgs(_puck, ObjectType.Puck);
 
                ObjectMotionEvent(this, args);
            }
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
            switch(CurrentStage)
            {
                case GameStages.Countdown: UpdateCountdown(); break;
                case GameStages.Playing: UpdateInterface(); break;
                case GameStages.GameOver: GameOver(); break;
            }
        }

        private void GameOver()
        {
            throw new NotImplementedException();
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

            switch (obj.Type)
            {
                case ObjectType.Cone:
                    _livesHud.Value -= 1;
                    break;

                case ObjectType.Target:
                    _scoreHud.Value += 1;
                    break;
            }

            UpdateInterface();
        }
    }
}
