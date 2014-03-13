using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KwikHands.Domain.EventArg
{
    public class GameStageEventArgs : EventArgs
    {
        public GameStages Stage { get; set; }
    }
}
