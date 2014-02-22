using System.Windows.Media.Media3D;

namespace KwikHands.Domain
{
    public interface IGameWindow
    {
        void AddObject(GameObject newObject);
        void AddHudItem(HudItem newItem);
        void UpdateHudItem(HudItem updatedItem);
    }
}
