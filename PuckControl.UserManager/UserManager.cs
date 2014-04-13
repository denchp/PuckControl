using PuckControl.Domain;
using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Collections.Generic;

[assembly: CLSCompliant(true)]
namespace PuckControl.Users
{
    public class UserManager : IUserManager
    {
        private List<UserType> _userTypes;
        private IDataService _dataService;

        public UserManager(IDataService dataService)
        {
            _userTypes = new List<UserType>();
            _userTypes.Add(UserType.Local);
            _dataService = dataService;
        }

        public IEnumerable<User> Users
        {
            get
            {
                var users = _dataService.UserRepository.All;
                return users;
            }
        }

        public void SaveUser(User user)
        {
            var users = new HashSet<User>(_dataService.UserRepository.All);
            users.Add(user);

            _dataService.UserRepository.Save(users);
        }

        public IEnumerable<UserType> UserTypes
        {
            get { return _userTypes; }
        }
    }
}
