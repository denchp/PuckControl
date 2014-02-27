using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KwikHands.Domain
{
    public class GameObject
    {
        public bool ApplyPhysics = true;

        public Int32 Mass { get; set; }
        public ModelVisual3D Model { get; set; }
        public Vector3D Position { get; set; }
        public Vector3D Motion { get; set; }
        public Vector3D Rotation { get; set; }
        public ObjectType Type { get; set; }
        public String ID { get; set; }
        public bool Active { get; set; }
    }
}
