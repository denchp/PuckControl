using PuckControl.Domain.Entities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckControl.Controls
{
    public class UserRadioButton : RadioButton
    {
        public User User { get; set; }

        static UserRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserRadioButton), new FrameworkPropertyMetadata(typeof(UserRadioButton)));
        }
    }
}
