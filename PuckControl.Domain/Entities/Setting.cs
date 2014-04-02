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
        public string SelectedOption { get; set; }
        public Dictionary<string, string> Options { get; private set; }
        public string Note { get; set; }

        public Setting()
        {
            Options = new Dictionary<string, string>();
        }
        
    }
}
