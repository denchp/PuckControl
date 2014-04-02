using PuckControl.Domain.Entities;
using System;
using System.Collections.Generic;

namespace PuckControl.Domain.Interfaces
{
    public interface ISettingsModule
    {
        IEnumerable<Setting> Settings { get; }
        void ReloadSettings();
    }
}
