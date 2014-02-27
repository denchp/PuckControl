using KwikHands.Domain.EventArg;
using System;
using System.Windows.Media.Media3D;
namespace KwikHands.Domain
{
    public interface IGame
    {
        bool Init();
        void StartGame();

        event EventHandler<ObjectEventArgs> NewObjectEvent;
        event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        event EventHandler<ObjectEventArgs> ObjectCollisionEvent;
        event EventHandler<ObjectEventArgs> ObjectMotionEvent;

        event EventHandler<HudItemEventArgs> NewHudItemEvent;
        event EventHandler<HudItemEventArgs> UpdateHudItemEvent;

        void UpdateBall(Vector3D motionVector);
        void PuckCollision(GameObject obj);
    }
}
