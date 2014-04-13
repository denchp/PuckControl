using PuckControl.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuckControl.Domain.Interfaces
{
    public interface IDataService
    {
        IRepository<Score> ScoreRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<Setting> SettingRespository { get; }
        IRepository<SettingOption> OptionRepository { get; }
    }
}
