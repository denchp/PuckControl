using PuckControl.Domain.Entities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckControl.Controls
{
    public abstract class SettingControl : Control
    {
        public event EventHandler SettingChanged;
        public Setting Setting { get; set; }
        
        protected void OnSettingChanged()
        {
            if (SettingChanged != null)
                SettingChanged(this, new EventArgs());
        }
    }

    public class ComboSettingControl : SettingControl
    {
        static ComboSettingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboSettingControl), new FrameworkPropertyMetadata(typeof(ComboSettingControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ComboBox options = (ComboBox)this.GetTemplateChild("cmbOptions");

            if(options != null)
                options.SelectionChanged += options_SelectionChanged;
        }

        void options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSettingChanged();
        }
    }

    public class TextSettingControl : SettingControl
    {
        static TextSettingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextSettingControl), new FrameworkPropertyMetadata(typeof(TextSettingControl)));
        }
    }
}
