using PuckControl.Domain.Entities;
using System;

namespace PuckControl.Domain.EventArg
{
    public class ObjectEventArgs : EventArgs
    {
        public GameObject Obj { get; set; }

        public ObjectEventArgs(GameObject obj = null)
        {
            this.Obj = obj;
        }
    }
}
