using System;
using System.Collections.Generic;

namespace PuckControl.Domain.Entities
{
    [Serializable]
    public class Setting : AbstractEntity
    {
        public string Module { get; set; }
        public string Section { get; set; }
        public string Key { get; set; }
        public virtual ICollection<SettingOption> Options { get; set; }
        public string Note { get; set; }

        public Setting()
        {
            Options = new List<SettingOption>();
        }
        
    }
}
