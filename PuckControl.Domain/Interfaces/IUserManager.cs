using PuckControl.Domain.Entities;
using System.Collections.Generic;

namespace PuckControl.Domain.Interfaces
{
    public interface IUserManager
    {
        IEnumerable<User> Users { get; }
        void SaveUser(User user);
        IEnumerable<UserType> UserTypes { get; }
    }
}
