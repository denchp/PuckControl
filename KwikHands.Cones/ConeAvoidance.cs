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

        private Dictionary<GameObject, ObjectType> _gameObjects = new Dictionary<GameObject, ObjectType>();

        public bool Init()
        {
            string AssemblyName = "KwikHands.Cones";
            BitmapImage TextureImage = new BitmapImage();

            var info = Application.GetResourceStream(new Uri("pack://application:,,,/" + AssemblyName + ";component/models/Cone.xaml"));
            _cone.Model = (ModelVisual3D)XamlReader.Load(info.Stream);

            info = Application.GetResourceStream(new Uri("pack://application:,,,/" + AssemblyName + ";component/models/Puck.xaml"));
            _puck.Model = (ModelVisual3D)XamlReader.Load(info.Stream);
            _puck.Position = new Vector3D(-6, -4, 0);

            info = Application.GetResourceStream(new Uri("pack://application:,,,/" + AssemblyName + ";component/models/rink.xaml"));
            _rink.Model = (ModelVisual3D)XamlReader.Load(info.Stream);

            _rink.ApplyPhysics = false;

            _gameObjects.Add(_cone, ObjectType.Cone);
            _gameObjects.Add(_rink, ObjectType.Rink);
            _gameObjects.Add(_puck, ObjectType.Puck);

            if (NewObjectEvent != null)
            {
                var args = new ObjectEventArgs(_cone);
                //NewObjectEvent(this, args);

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
                _puck.Position = motionVector / 5;
                
                var args = new ObjectEventArgs(_puck, ObjectType.Puck);
                ObjectMotionEvent(this, args);
            }
        }
    }
}
