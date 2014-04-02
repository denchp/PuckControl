using PuckControl.Domain.Entities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckControl.Controls
{
    public class ComboSettingControl : Control
    {
        public Setting Setting {get; set;}

        static ComboSettingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboSettingControl), new FrameworkPropertyMetadata(typeof(ComboSettingControl)));
        }
    }

    public class TextSettingControl : Control
    {
        public Setting Setting { get; set; }

        static TextSettingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextSettingControl), new FrameworkPropertyMetadata(typeof(TextSettingControl)));
        }
    }
}
