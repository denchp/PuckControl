using PuckControl.Domain.Entities;

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
