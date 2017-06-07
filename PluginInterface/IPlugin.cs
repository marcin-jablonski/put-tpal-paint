using System.Windows.Controls;

namespace PluginInterface
{
    public interface IPlugin
    {
        void SetToolbar(ToolBarTray toolBarTray);
        void SetCanvas(Canvas canvas);
        void AddPluginControls();
    }
}