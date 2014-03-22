using KwikHands.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwikHands.Domain.EventArg
{
    public class HudItemEventArgs : EventArgs
    {
        public HudItem Item { get; set; }
    }
}
