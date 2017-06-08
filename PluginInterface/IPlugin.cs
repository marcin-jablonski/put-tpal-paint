using System;
using System.Windows;
using System.Windows.Controls;

namespace PluginInterface
{
    public delegate void PluginUndo(object sender, RoutedEventArgs routedEventArgs);

    public interface IPlugin
    {
        void SetCanvas(Canvas canvas);
        ToolBar GetPluginToolbar();
    }
}