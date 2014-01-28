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

        public int Mass { get; set; }
        public ModelVisual3D Model { get; set; }
        public Vector3D Position { get; set; }
        public Vector3D Motion { get; set; }
        public Vector3D Rotation { get; set; }
    }
}
