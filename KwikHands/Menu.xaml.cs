using KwikHands.Domain;
using KwikHands.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KwikHands
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        private KwikEngine _engine;

        public Menu(KwikEngine engine)
        {
            InitializeComponent();
            _engine = engine;
            btnConesAndTargets.Click += btnConesAndTargets_Click;
            btnSpeedChallenege.Click += btnSpeedChallenege_Click;
        }

        void btnSpeedChallenege_Click(object sender, RoutedEventArgs e)
        {

            _engine.LoadGame<KwikHands.Games.SpeedChallenge>();
            this.Visibility = System.Windows.Visibility.Collapsed;
            _engine.StartGame();
        }

        void btnConesAndTargets_Click(object sender, RoutedEventArgs e)
        {
            _engine.LoadGame<KwikHands.Games.ConesAndTargets>();
            this.Visibility = System.Windows.Visibility.Collapsed;
            _engine.StartGame();
        }
    }
}
