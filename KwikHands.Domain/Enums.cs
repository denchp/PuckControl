using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwikHands.Domain
{
    public enum TransformType
    {
        TranslateX,
        TranslateY,
        TranslateZ,
        RotateX,
        RotateY, 
        RotateZ
    }

    public enum ObjectType
    {
        None, Rink, Puck, Cone, Target, Player, Board, Goalie
    }
}
