namespace PuckControl.Windows
{
    using NetSparkle;
    using PuckControl.Controls;
    using PuckControl.Domain;
    using PuckControl.Domain.Entities;
    using PuckControl.Domain.Interfaces;
    using PuckControl.Engine;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public partial class Main : Window, IDisposable
    {
        GameEngine _engine;

        private DebugWindow _debugWindow;
        private List<IGame> _games;

        private NewUser _newUserWindow;
        private HighScores _highScoresWindow;
        private Game _gameWindow;
        private Settings _settingsWindow;
        private Sparkle _sparkle;

        public Main()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            System.Drawing.Icon windowIcon;
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/PuckControl;component/Assets/pcIcon.ico")).Stream;
            windowIcon = new System.Drawing.Icon(iconStream);

            _sparkle = new Sparkle("http://www.headsup.technology/download/betaupdates", windowIcon); 
            _sparkle.StartLoop(true);

            _engine = new GameEngine();
            _debugWindow = new DebugWindow(_engine);
            _debugWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            _newUserWindow = new NewUser();
            _newUserWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            _gameWindow = new Game(_engine);
            _highScoresWindow = new HighScores(_engine);
            _highScoresWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            _settingsWindow = new Settings(_engine, _engine.SettingsRepository);
            _settingsWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            this.Closing += MainWindow_Closing;

            _games = FindGames();

            foreach (IGame gameType in _games)
            {
                IGame game = (IGame)Activator.CreateInstance(gameType.GetType());

                if (String.IsNullOrWhiteSpace(game.Name))
                    continue;

                TileButton tileButton = new TileButton();
                tileButton.BackgroundBrush = new SolidColorBrush(game.TileColor);
                tileButton.ButtonLabel = game.Name;
                tileButton.DataContext = tileButton;

                tileButton.Click += (s, e) =>
                {
                    _engine.LoadGame(gameType);
                    _gameWindow.Visibility = System.Windows.Visibility.Visible;
                    _gameWindow.Owner = this;
                    this.Visibility = System.Windows.Visibility.Hidden;
                    _engine.StartGame();
                };

                GameList.Children.Add(tileButton);
            }

            _engine.GameOver += _engine_GameOver;
            _engine.Users.CollectionChanged += (s, e) => { PopulateUserSelectionList(); };

            try
            {
                _engine.Init();
            }
            catch (NotSupportedException ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "An error occured", MessageBoxButton.OK);
            }

            this.KeyDown += MainWindow_KeyDown;
            this.ContentRendered += (s, e) =>
            {
                _newUserWindow.Owner = this;
                if (_engine.Users.Count() == 0)
                    ShowNewUserDialog();
                else
                    PopulateUserSelectionList();
            };

            this.Activated += Main_Activated;
        }

        void Main_Activated(object sender, EventArgs e)
        {
            try
            {
                _gameWindow.Visibility = System.Windows.Visibility.Hidden;
                _gameWindow.Owner = this;
                _highScoresWindow.Owner = this;
                _settingsWindow.Owner = this;
            }
            catch (InvalidOperationException)
            { }
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.F12:
                    _debugWindow.Visibility = _debugWindow.Visibility == System.Windows.Visibility.Collapsed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _engine.StopTracking();
            _debugWindow.Close();
        }

        private void PopulateUserSelectionList()
        {
            UserList.Children.Clear();

            foreach (var user in _engine.Users)
            {
                var userButton = new UserRadioButton();
                userButton.Margin = new Thickness(5, 0, 5, 0);
                userButton.DataContext = user;
                userButton.GroupName = "Users";
                userButton.Content = user.Name;

                userButton.Click += (s, e) =>
                {
                    _engine.CurrentUser = userButton.DataContext as User;
                    GameList.Visibility = System.Windows.Visibility.Visible;
                };

                UserList.Children.Add(userButton);
            }

        }

        private void NewUser(object sender, RoutedEventArgs args)
        {
            ShowNewUserDialog();
        }

        private void ShowNewUserDialog()
        {
            _newUserWindow.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            _newUserWindow.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            _newUserWindow.ShowDialog();

            if (_newUserWindow.Canceled)
                return;

            var newUser = new User();
            newUser.Name = _newUserWindow.txtUserName.Text;
            newUser.BirthYear = Int32.Parse(_newUserWindow.txtBirthYear.Text);
            newUser.UserType = (UserType)Enum.Parse(typeof(UserType), _newUserWindow.cmbUserType.Text);
            newUser.UserType = UserType.Local;

            _engine.Users.Add(newUser);
        }

        private void ShowSettings(object sender, RoutedEventArgs args)
        {
            _settingsWindow.ShowDialog();
        }

        private void _engine_GameOver(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                var delay = new System.Windows.Threading.DispatcherTimer();
                delay.Tick += (s, args) =>
                {
                    delay.Stop();
                    _highScoresWindow.UpdateHighScores();
                    _highScoresWindow.ShowDialog();
                };
                delay.Interval = new TimeSpan(0, 0, 2);
                delay.Start();
            }));

        }

        private static List<IGame> FindGames()
        {
            List<IGame> games = new List<IGame>();
            string folder = System.AppDomain.CurrentDomain.BaseDirectory;

            string[] files = Directory.GetFiles(folder, "*.dll");

            foreach (string file in files)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);

                    foreach (Type type in assembly.GetTypes())
                    {
                        Type iface = type.GetInterface("IGame");

                        if (iface != null && iface.Namespace.StartsWith("PuckControl.Domain"))
                        {
                            if (!type.IsAbstract)
                            {
                                IGame plugin = (IGame)Activator.CreateInstance(type);
                                games.Add(plugin);
                            }
                        }
                    }
                }
                catch (NotSupportedException)
                {
                    // Do nothing, we've tried to load an unsupported dll
                }
            }
            return games;
        }

        #region IDispose Implementation
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _engine.Dispose();
                _gameWindow.Dispose();
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion
    }
}
