using PuckControl.Domain.Entities;
using System;

namespace PuckControl.Domain.EventArg
{
    public class HUDItemEventArgs : EventArgs
    {
        public HUDItem Item { get; set; }
    }
}
