using PuckControl.Domain.Entities;
using PuckControl.Engine;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using PuckControl.Controls;
using PuckControl.Domain.Interfaces;

namespace PuckControl.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private GameEngine _engine;
        private IRepository<Setting> _settingRepository;

        public Settings(GameEngine engine, IRepository<Setting> settingRepository)
        {
            InitializeComponent();
            _engine = engine;
            _settingRepository = settingRepository;

            SettingsList.SelectionChanged += Settings_SelectionChanged;
            _engine.Settings.CollectionChanged += Settings_CollectionChanged;
            this.Closing += Settings_Closing;

            SaveSettings.Click += SaveSettings_Click;
            Cancel.Click += Cancel_Click;
            ApplySettings.Click += ApplySettings_Click;
        }

        void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            _settingRepository.Save(_engine.Settings);
            _engine.ReloadSettings();
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingRepository.Save(_engine.Settings);
            _engine.ReloadSettings();
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        void Settings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // prevent closing as this window is re-used.
            e.Cancel = true;
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        void Settings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var sections = _engine.Settings.Select(x => x.Section).Distinct().ToList();
            SettingsList.DataContext = sections;

            if (SettingsList.SelectedValue != null)
                Settings_SelectionChanged(SettingsList, null);
        }

        void Settings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string sectionName = ((ListBox)sender).SelectedValue.ToString();
            var settings = _engine.Settings.Where(x => x.Section == sectionName);
            SettingsStack.Children.Clear();

            foreach (var setting in settings)
            {
                if (setting.Options.Count > 0)
                {
                    ComboSettingControl newControl = new ComboSettingControl();
                    newControl.Setting = setting;
                    newControl.DataContext = newControl;
                    newControl.SettingChanged += (s, args) => { ApplySettings_Click(this, new RoutedEventArgs()); };
                    SettingsStack.Children.Add(newControl);
                }
                else
                {
                    TextSettingControl newControl = new TextSettingControl();
                    newControl.Setting = setting;
                    newControl.DataContext = newControl;
                    SettingsStack.Children.Add(newControl);
                }
            }
        }
    }
}
