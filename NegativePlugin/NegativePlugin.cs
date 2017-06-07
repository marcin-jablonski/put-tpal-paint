using System.Windows;
using System.Windows.Controls;
using PluginInterface;

namespace NegativePlugin
{
    public class NegativePlugin : IPlugin
    {
        private ToolBarTray _toolBarTray;
        private Canvas _canvas;
        public void SetToolbar(ToolBarTray toolBarTray)
        {
            _toolBarTray = toolBarTray;
        }

        public void SetCanvas(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void AddPluginControls()
        {
            _toolBarTray.ToolBars.Add(GetPluginToolbar());   
        }

        private ToolBar GetPluginToolbar()
        {
            ToolBar toolBar = new ToolBar();
            toolBar.Items.Add(GetNegativeButton());
            return toolBar;
        }

        private Button GetNegativeButton()
        {
            var button = new Button {Content = "Negative"};
            button.Click += button_Click;
            return button;
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
