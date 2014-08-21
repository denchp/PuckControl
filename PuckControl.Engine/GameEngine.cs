using PuckControl.Data.CE;
using PuckControl.Domain;
using PuckControl.Domain.Entities;
using PuckControl.Domain.EventArg;
using PuckControl.Domain.Interfaces;
using PuckControl.Scoring;
using PuckControl.Tracking;
using PuckControl.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Media.Media3D;

[assembly: CLSCompliant(true)]
namespace PuckControl.Engine
{
    public class GameEngine : IDisposable
    {
        public event EventHandler<ImageEventArgs> NewCameraImage;
        public event EventHandler<ImageEventArgs> NewTrackingImage;
        public event EventHandler<ObjectEventArgs> NewObject;
        public event EventHandler<ObjectEventArgs> ObjectMotion;
        public event EventHandler<HUDItemEventArgs> NewHUDItem;
        public event EventHandler<HUDItemEventArgs> UpdateHUDItem;
        public event EventHandler<MediaEventArgs> PlayMedia;
        public event EventHandler<ObjectEventArgs> RemoveObject;
        public event EventHandler<HUDItemEventArgs> RemoveHUDItem;
        public event EventHandler TrackingUpdateReceived;
        public event EventHandler GameOver;
        public event EventHandler LostBall;
        public event EventHandler FoundBall;

        public bool Tracking { get; set; }
        public double MaxSpeed { get; set; }
        public double ControlDeadZone { get; set; }
        public Score LastGameScore { get; set; }
        public ObservableCollection<User> Users { get; private set; }
        public User CurrentUser { get; set; }
        public ObservableCollection<Setting> Settings { get; private set; }
        public IRepository<Setting> SettingsRepository { get { return _dataService.SettingRepository; } }
        private IDataService _dataService;
        private IGame _game;
        private IBallTracker _tracker;
        private Thread _trackingThread;
        private HashSet<GameObject> _objects;
        private HashSet<IScorekeeper> _scorekeepers;
        private HashSet<IUserManager> _userManagers;
        private HashSet<ISettingsModule> _settingModules;

        public GameEngine()
        {
            Settings = new ObservableCollection<Setting>();
            Users = new ObservableCollection<User>();
             string folderpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
             _dataService = new CEDataService("DataSource=|DataDirectory|" + folderpath + @"\PuckControl\PuckControl.sdf");
            _objects = new HashSet<GameObject>();
            _scorekeepers = new HashSet<IScorekeeper>();
            _userManagers = new HashSet<IUserManager>();
            _settingModules = new HashSet<ISettingsModule>();

            _scorekeepers.Add(new Scorekeeper(_dataService));
            _userManagers.Add(new UserManager(_dataService));
                
            MaxSpeed = 4;
            ControlDeadZone = .5;
                
            GetUsers(null).ToList().ForEach(x => Users.Add(x));

            Users.CollectionChanged += Users_CollectionChanged;
        }

        public void Init()
        {
            try
            {
                _tracker = new BlobBallTracker(_dataService);
                _tracker.LostBall += _tracker_LostBall;
                RegisterSettings(_tracker);
                _tracker.NewCameraImage += _tracker_NewCameraImages;
            }
            catch (NotSupportedException)
            {
                if (_tracker != null)
                    _tracker.Dispose();

                throw;
            }                
        }
        
        public void StartGame()
        {
            _game.Reset();
            _game.StartGame();
        }

        public void LoadGame<T>(T game) where T : IGame
        {
            if (_game != null)
            {
                _game.CleanUp();
            }

            if (_game == null || _game.GetType() != typeof(T))
            {
                _game = (IGame)Activator.CreateInstance(game.GetType());
                _game.NewObjectEvent += _game_NewObjectEvent;
                _game.RemoveObjectEvent += _game_RemoveObjectEvent;
                _game.NewHUDItemEvent += _game_NewHUDItemEvent;
                _game.RemoveHUDItemEvent += _game_RemoveHUDItemEvent;
                _game.UpdateHUDItemEvent += _game_UpdateHUDItemEvent;
                _game.MediaEvent += _game_MediaEvent;
                _game.GameStageChange += _game_GameStageChange;
            }

            _game.Init();
        }

        public void Collision(GameObject objectOne, GameObject objectTwo)
        {
            _game.Collision(objectOne, objectTwo);
        }

        public void StartTracking()
        {
            if (_tracker == null)
                return;

            Tracking = true;
            _trackingThread = new Thread(delegate() { _tracker.StartTracking(); });
            _trackingThread.Start();
            
            _tracker.BallUpdate -= _tracker_BallUpdate;
            _tracker.BallUpdate += _tracker_BallUpdate;
        }

        public void StopTracking()
        {
            Tracking = false;
            
            if (_tracker == null)
                return;

            _tracker.BallUpdate -= _tracker_BallUpdate;
            _tracker.StopTracking();
        }

        public void ToggleBoxes()
        {
            _tracker.DrawBoxes = !_tracker.DrawBoxes;
        }

        public void EndGame()
        {
            if (_game != null && _game.CurrentStage != GameStage.GameOver)
                _game.EndGame();
        }

        public IEnumerable<ScoreTable> GetScores(int start = 0, int count = 0)
        {
            var scores = new List<ScoreTable>();

            foreach (var scoreKeeper in _scorekeepers)
            {
                var scoreTable = new ScoreTable();
                string gameName = _game == null ? String.Empty : _game.Name;

                scoreTable.Name = scoreKeeper.Name;
                scoreTable.Scores = scoreKeeper.GetScores(gameName, count, start);

                scores.Add(scoreTable);
            }

            return scores;
        }

        public IEnumerable<ScoreTable> GetScoresSince(DateTime startDate, int start = 0, int count = 0)
        {
            var scores = new List<ScoreTable>();

            foreach (var scoreKeeper in _scorekeepers)
            {
                var scoreTable = new ScoreTable();
                scoreTable.Name = scoreKeeper.Name;
                string gameName = _game == null ? String.Empty : _game.Name;

                var resultScores = scoreKeeper.GetScores(gameName, count, start, startDate);
                scoreTable.Scores = resultScores;
                scores.Add(scoreTable);
            }

            return scores;
        }
        
        public IEnumerable<string> Scorekeepers
        {
            get
            {
                return _scorekeepers.Select(x => x.Name).ToList();
            }
        }

        void _tracker_LostBall(object sender, EventArgs e)
        {
            if (LostBall != null)
                LostBall(this, new EventArgs());
        }
        
        private void RegisterSettings(ISettingsModule module)
        {
            _settingModules.Add(module);

            var settings = ((ISettingsModule)module).Settings;
            var keyList = Settings.Select(x => x.Key).ToList();

            foreach (var setting in settings)
            {
                string key = setting.Key;
                if (keyList.Contains(key))
                    this.Settings.Remove(this.Settings.First(x => x.Key == key));

                Settings.Add(setting);
            }
        }

        private void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (User user in e.NewItems)
                    SaveUser(user, null);
            }
        }

        private void SaveUser(User newUser, UserType? userType)
        {
            var managers = new HashSet<IUserManager>();

            if (userType.HasValue)
                managers = (HashSet<IUserManager>)_userManagers.Where(x => (x.UserTypes).Contains(userType.Value));
            else
                managers = (HashSet<IUserManager>)_userManagers;

            foreach (var userManager in managers)
            {
                userManager.SaveUser(newUser);
            }
        }

        private IEnumerable<User> GetUsers(UserType? userType)
        {
            var managers = new HashSet<IUserManager>();
            if (userType.HasValue)
                managers = (HashSet<IUserManager>)_userManagers.Where(x => ((List<UserType>)x.UserTypes).Contains(userType.Value));
            else
                managers = (HashSet<IUserManager>)_userManagers;

            var users = new HashSet<User>();
            foreach (var manager in managers)
            {
                var result = manager.Users;
                foreach (var user in result)
                    users.Add(user);
            }
            
            return users;
        }

        private void _tracker_NewCameraImages(object sender, EventArgs e)
        {
            if (NewCameraImage != null)
            {
                NewCameraImage(this, new ImageEventArgs() { Image = (Bitmap)_tracker.CameraImage.Clone() });
            }

            if (NewTrackingImage != null)
            {
                NewTrackingImage(this, new ImageEventArgs() { Image = (Bitmap)_tracker.TrackingImage.Clone() });
            }
        }

        private void _game_RemoveHUDItemEvent(object sender, HUDItemEventArgs e)
        {
            if (RemoveHUDItem != null)
                RemoveHUDItem(this, e);
        }

        private void _game_GameStageChange(object sender, GameStageEventArgs e)
        {
            switch (e.Stage)
            {
                case GameStage.Playing:
                case GameStage.Countdown:
                    this.StartTracking();
                    break;
                case GameStage.GameOver:
                    this.StopTracking();

                    if (_game.Score.HasValue)
                    {

                        LastGameScore = new Score() { Created = DateTime.Now, FinalScore = _game.Score.Value, Game = _game.Name, User = CurrentUser };

                        foreach (var scoreKeeper in _scorekeepers)
                            scoreKeeper.SaveScore(LastGameScore);
                    }

                    if (this.GameOver != null)
                        GameOver(this, new EventArgs());

                    break;
            }
        }

        private void _game_MediaEvent(object sender, MediaEventArgs e)
        {
            if (PlayMedia != null)
                PlayMedia(this, e);
        }

        private void _game_UpdateHUDItemEvent(object sender, HUDItemEventArgs e)
        {
            if (this.UpdateHUDItem != null)
            {
                UpdateHUDItem(this, e);
            }
        }

        private void _game_NewHUDItemEvent(object sender, HUDItemEventArgs e)
        {
            if (this.NewHUDItem != null)
                NewHUDItem(this, e);
        }

        private void _tracker_BallUpdate(object sender, BallUpdateEventArgs e)
        {
            if (FoundBall != null)
                FoundBall(this, new EventArgs());

            if (ObjectMotion == null)
                return;
            try
            {
                foreach (GameObject obj in _objects.Where(x => x.ControlledObject == true))
                {
                    Vector3D motionVector = e.PositionVector - obj.Position;
                    if (motionVector.Length < this.ControlDeadZone)
                        continue;

                    switch (_game.ControlType)
                    {
                        case ControlType.Relative:
                            if (motionVector.Length > MaxSpeed)
                                motionVector = motionVector * (MaxSpeed / motionVector.Length);

                            obj.Position = obj.Position + motionVector;
                            ObjectMotion(this, new ObjectEventArgs(obj));
                            break;

                        default:
                            obj.Position = obj.Position + motionVector;
                            ObjectMotion(this, new ObjectEventArgs(obj));
                            break;
                    }
                }

                if (TrackingUpdateReceived != null)
                    TrackingUpdateReceived(this, new EventArgs());
            }
            catch (Exception ex)
            {

            }
        }

        private void _game_RemoveObjectEvent(object sender, ObjectEventArgs e)
        {
            _objects.Remove(e.Obj);

            if (RemoveObject != null)
                RemoveObject(this, e);
        }

        private void _game_NewObjectEvent(object sender, ObjectEventArgs e)
        {
            _objects.Add(e.Obj);
            e.Obj.ObjectMotion += Obj_ObjectMotion;

            if (NewObject != null)
                NewObject(this, e);
        }

        void Obj_ObjectMotion(object sender, EventArgs e)
        {
            if (ObjectMotion != null)
            {
                ObjectMotion(this, new ObjectEventArgs() { Obj = (GameObject)sender });
            }
        }
        
        public void ReloadSettings()
        {
            foreach (var module in _settingModules)
            {
                module.ReloadSettings();
                RegisterSettings(module);
            }
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
                _tracker.Dispose();
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion
    }
}
