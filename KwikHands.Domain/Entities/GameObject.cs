using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KwikHands.Domain.Entities
{
    public class GameObject
    {
        public bool ApplyPhysics = true;

        public Int32 Mass { get; set; }
        public Int32 MotionSmoothingSteps { get; set; }
        public Vector3D Position
        {
            get {
                Vector3D averagedPosition = new Vector3D(0,0,0);
                Int32 Steps = 0;
                if ((Steps = Positions.Count()) == 0)
                    return averagedPosition;

                var localPositions = new Vector3D[10];
                Positions.CopyTo(localPositions, 0);
                foreach (var vector in localPositions)
                {
                    averagedPosition += (vector * (Array.IndexOf(localPositions, vector) + 1));
                }

                if (Steps > 1)
                    averagedPosition = averagedPosition / (Steps * (Steps + 1) / 2);

                return averagedPosition;
            }
            set
            {
                while (Positions.Count() >= MotionSmoothingSteps)
                    Positions.RemoveAt(0);
                
                Positions.Add(value);
            }
        }
        private List<Vector3D> Positions { get; set; }
        public Vector3D Motion { get; set; }
        public Vector3D Rotation { get; set; }
        public ObjectType Type { get; set; }
        public String ID { get; set; }
        public bool Active { get; set; }
        public Rect3D Bounds { get; set; }

        public GameObject()
        {
            Positions = new List<Vector3D>();
            MotionSmoothingSteps = 1;
        }
    }
}
