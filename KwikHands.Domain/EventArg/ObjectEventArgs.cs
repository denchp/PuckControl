using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwikHands.Domain.EventArg
{
    public class ObjectEventArgs
    {
        public GameObject Obj = null;
        public ObjectType ObjType = ObjectType.None;

        public ObjectEventArgs(GameObject obj, ObjectType objType = ObjectType.None)
        {
            this.Obj = obj;
            this.ObjType = objType;
        }
    }
}
