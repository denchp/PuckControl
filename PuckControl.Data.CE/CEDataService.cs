using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Data.Entity;

namespace PuckControl.Data.CE
{
    public class CEDataService : IDataService, IDisposable
    {
        DataContext _context;

        private IRepository<Score> _scoreRepository;
        private IRepository<User> _userRepository;
        private IRepository<Setting> _settingRepository;
        private IRepository<SettingOption> _settingOptionRepository;
        private bool _disposed;

        public CEDataService(string connectionString)
        {
            if (!Database.Exists(connectionString))
                Database.SetInitializer<DataContext>(new DropCreateDatabaseAlways<DataContext>());
            else
                Database.SetInitializer<DataContext>(new DropCreateDatabaseIfModelChanges<DataContext>());

            _context = new DataContext(connectionString);
            
            Database.Exists(connectionString);
            
            
        }

        public IRepository<Score> ScoreRepository
        {
            get { return _scoreRepository ?? (_scoreRepository = new CERepository<Score>(_context)); }
        }

        public IRepository<User> UserRepository
        {
            get { return _userRepository ?? (_userRepository = new CERepository<User>(_context)); }
        }

        public IRepository<Setting> SettingRepository
        {
            get { return _settingRepository ?? (_settingRepository = new CERepository<Setting>(_context)); }
        }

        public IRepository<SettingOption> OptionRepository
        {
            get { return _settingOptionRepository ?? (_settingOptionRepository = new CERepository<SettingOption>(_context)); }
        }

        #region IDispose Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_context != null)
                    _context.Dispose();
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion
    }
}
