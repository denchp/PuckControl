using PuckControl.Domain.Entities;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckControl.Controls
{
    public class HighScoreControl : Control
    {
        public string Title { get; set; }
        public IEnumerable<Score> Scores { get; set; }

        private Color BackgroundColor;
        public SolidColorBrush BackgroundBrush { get { return new SolidColorBrush(BackgroundColor); } }

        static HighScoreControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HighScoreControl), new FrameworkPropertyMetadata(typeof(HighScoreControl)));
        }

        public HighScoreControl() { }

        public HighScoreControl(string title, IEnumerable<Score> scores)
        {
            Scores = scores;
            Title = title;
            BackgroundColor = Color.FromRgb(255, 255, 255);
        }
    }
}
