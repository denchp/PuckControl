using PuckControl.Domain.Entities;
using PuckControl.Domain.EventArg;
using System;
using System.Windows.Media;

[assembly: CLSCompliant(true)]
namespace PuckControl.Domain.Interfaces
{
    public interface IGame
    {
        event EventHandler<ObjectEventArgs> NewObjectEvent;
        event EventHandler<ObjectEventArgs> RemoveObjectEvent;
        event EventHandler<HUDItemEventArgs> NewHUDItemEvent;
        event EventHandler<HUDItemEventArgs> UpdateHUDItemEvent;
        event EventHandler<HUDItemEventArgs> RemoveHUDItemEvent;
        event EventHandler<MediaEventArgs> MediaEvent;
        event EventHandler<GameStageEventArgs> GameStageChange;

        GameStage CurrentStage { get; set; }
        ControlType ControlType { get; set; }
        Color TileColor { get; }
        string Name { get; }
        int? Score { get; }

        bool Init();
        void StartGame();
        void EndGame();
        void Reset();
        void PuckCollision(GameObject obj);
        void CleanUp();
    }
}
