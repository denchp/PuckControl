using System;

namespace PuckControl.Domain.EventArg
{
    public class GameStageEventArgs : EventArgs
    {
        public GameStage Stage { get; set; }
    }
}
