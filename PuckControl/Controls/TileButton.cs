using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckControl.Controls
{
    public class TileButton : Button
    {
        public String ButtonLabel { get; set; }
        public SolidColorBrush BackgroundBrush { get; set; }

        static TileButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TileButton), new FrameworkPropertyMetadata(typeof(TileButton)));
        }
    }
}
