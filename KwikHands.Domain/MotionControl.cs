using KwikHands.Domain.EventArg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace KwikHands.Domain
{
    public class MotionControl
    {
        public EventHandler<ControlEventArgs> ControlChanged;

        public enum MotionType
        {
            Fixed, Continuous
        }

        public MotionControl(MotionType type = MotionType.Fixed) { this.ControlType = type; this.ControlVector = new Vector3D(0,0,0); }
        
        public MotionType ControlType { get; set; }
        
        private Vector3D _controlVector;
        public Vector3D ControlVector
        {
            get { return _controlVector; }
            set
            {
                this._controlVector = value;
                if (ControlChanged != null)
                {
                    ControlChanged(this, new ControlEventArgs());
                }
            }
        }
    }
}
