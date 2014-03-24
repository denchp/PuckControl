using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwikHands.Domain
{
    public enum TransformType
    {
        Translate, Rotate, Scale
    }

    public enum GameStages
    {
        Countdown,
        Playing,
        GameOver,
        Menu
    }

    public enum ControlTypeEnum
    {
        Absolute, Relative
    }
}
