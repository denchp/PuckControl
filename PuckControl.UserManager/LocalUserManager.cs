using PuckControl.Data.Dat;
using PuckControl.Domain;
using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Collections.Generic;

[assembly: CLSCompliant(true)]
namespace PuckControl.UserManager
{
    public class LocalUserManager : IUserManager
    {
        private List<UserType> _userTypes;
        private IRepository<User> _repository;

        public LocalUserManager()
        {
            _userTypes = new List<UserType>();
            _userTypes.Add(UserType.Local);
            _repository = new DatRepository<User>();
        }

        public IEnumerable<User> Users
        {
            get
            {
                var users = _repository.All;
                return users;
            }
        }

        public void SaveUser(User user)
        {
            var users = (HashSet<User>)_repository.All;
            users.Add(user);

            _repository.Save(users);
        }

        public IEnumerable<UserType> UserTypes
        {
            get { return _userTypes; }
        }
    }
}
