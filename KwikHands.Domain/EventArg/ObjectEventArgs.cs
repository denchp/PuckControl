using KwikHands.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwikHands.Domain.EventArg
{
    public class ObjectEventArgs : EventArgs
    {
        public GameObject Obj = null;

        public ObjectEventArgs(GameObject obj = null)
        {
            this.Obj = obj;
        }
    }
}
