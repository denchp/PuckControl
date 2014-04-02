using PuckControl.Controls;
using PuckControl.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PuckControl.Windows
{
    /// <summary>
    /// Interaction logic for HighScores.xaml
    /// </summary>
    public partial class HighScores : Window
    {
        private GameEngine _engine;
        private HashSet<HighScoreControl> _highScoreLists;

        public HighScores(GameEngine engine)
        {
            if (engine == null)
                throw new ArgumentException("HighScore list cannot be initialized with null engine");

            InitializeComponent();
            _engine = engine;

            foreach (string name in _engine.Scorekeepers)
            {
                TabButton highScoreKeeperButton = new TabButton() { ButtonLabel = name };

                highScoreKeeperButton.Click += (s, e) =>
                {
                    UpdateHighScores(name);
                };

                highScoreKeeperButton.DataContext = highScoreKeeperButton;
                ScoreKeeperButtonPanel.Children.Add(highScoreKeeperButton);
            }

            _highScoreLists = new HashSet<HighScoreControl>();
            UpdateHighScores(_engine.Scorekeepers.First());

            btnReplay.Click += btnReplay_Click;
            btnShowMenu.Click +=btnShowMenu_Click;
            this.Closing += HighScores_Closing;
        }

        void HighScores_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        public void UpdateHighScores()
        {
            UpdateHighScores(_highScoreLists.First().Title);
        }

        public void UpdateHighScores(string title)
        {
            var scores = _engine.GetScores(0, 10);

            _highScoreLists.Clear();
            foreach (var table in scores)
            {
                var newList = new HighScoreControl(table.Name, table.Scores);
                _highScoreLists.Add(newList);
            }

            HighScoreControl.DataContext = _highScoreLists.Where(x => x.Title == title).First();
        }

        private void btnReplay_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            _engine.StartGame();            
        }

        private void btnShowMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Owner.Visibility = System.Windows.Visibility.Visible;
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
