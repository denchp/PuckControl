using KwikHands.Domain.Entities;
using KwikHands.Domain.EventArg;
using System;
using System.Windows.Media.Media3D;
namespace KwikHands.Domain
{
    public interface IGame
    {
        event EventHandler<ObjectEventArgs> NewObjectEvent;
        event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        event EventHandler<HudItemEventArgs> NewHudItemEvent;
        event EventHandler<HudItemEventArgs> UpdateHudItemEvent;

        event EventHandler<MediaEventArgs> MediaEvent;

        event EventHandler<GameStageEventArgs> GameStageChange;

        GameStages CurrentStage { get; set; }
        ControlTypeEnum ControlType { get; set; }

        bool Init();
        void StartGame();
        void PuckCollision(GameObject obj);
    }
}
