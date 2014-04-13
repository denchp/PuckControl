using PuckControl.Domain.Entities;
using System.Windows;
using System.Windows.Controls;

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
