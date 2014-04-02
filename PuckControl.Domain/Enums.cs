
namespace PuckControl.Domain
{
    public enum TransformType
    {
        Translate, Rotate, Scale
    }

    public enum GameStage
    {
        Countdown,
        Playing,
        GameOver,
        Menu
    }

    public enum ControlType
    {
        Absolute, Relative
    }

    public enum HUDItemType
    {
        Numeric, Text, Timer
    }

    public enum TimerType
    {
        Up, Down
    }

    public enum HUDItemStyle
    {
        Circle, Progress, Text
    }

    public enum UserType
    {
        Local, Online
    }
}
