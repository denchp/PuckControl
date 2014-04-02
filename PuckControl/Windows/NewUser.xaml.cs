using System.Windows;

namespace PuckControl.Windows
{
    /// <summary>
    /// Interaction logic for NewUser.xaml
    /// </summary>
    public partial class NewUser : Window
    {
        public bool Canceled { get; set; }
        public NewUser()
        {
            InitializeComponent();

            btnCancel.Click += btnCancel_Click;
            btnOk.Click += btnOk_Click;
        }

        void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Canceled = false;
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Canceled = true;
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
