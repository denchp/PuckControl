namespace KwikHands.Cones
{
    using KwikHands.Domain;
    using KwikHands.Domain.EventArg;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;

    public class Targets : IGame
    {
        GameObject _target = new GameObject();
        GameObject _puck = new GameObject();
        GameObject _rink = new GameObject();

        public event EventHandler<ObjectEventArgs> NewObjectEvent;
        public event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        public event EventHandler<ObjectEventArgs> ObjectCollisionEvent;
        public event EventHandler<ObjectEventArgs> ObjectMotionEvent;
        public event EventHandler<MediaEventArgs> MediaEvent;

        private Dictionary<GameObject, ObjectType> _gameObjects = new Dictionary<GameObject, ObjectType>();

        public bool Init()
        {
            BitmapImage TextureImage = new BitmapImage();
            
            _rink.ApplyPhysics = false;

            _gameObjects.Add(_target, ObjectType.Target);
            _gameObjects.Add(_rink, ObjectType.Rink);
            _gameObjects.Add(_puck, ObjectType.Puck);

            if (NewObjectEvent != null)
            {
                var args = new ObjectEventArgs(_target);
                NewObjectEvent(this, args);

                args.Obj = _target;
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
                _puck.Position = motionVector / 5;

                var args = new ObjectEventArgs(_puck, ObjectType.Puck);
                ObjectMotionEvent(this, args);
            }
        }


        public void StartGame()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<HudItemEventArgs> NewHudItemEvent;

        public event EventHandler<HudItemEventArgs> UpdateHudItemEvent;


        public void PuckCollision(GameObject obj)
        {
            throw new NotImplementedException();
        }


        public GameStages CurrentStage
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ControlTypeEnum ControlType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int ControlDeadZone
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public event EventHandler<GameStages> GameStageChange;
    }
}
