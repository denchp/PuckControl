using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckControl.Controls
{
    public class TabButton : Button
    {
        public string ButtonLabel { get; set; }
        public SolidColorBrush BackgroundBrush { get; set; }

        static TabButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabButton), new FrameworkPropertyMetadata(typeof(TabButton)));
        }
    }
}
