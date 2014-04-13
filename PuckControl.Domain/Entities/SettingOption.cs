using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuckControl.Domain.Entities
{
    public class SettingOption : AbstractEntity
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Setting_Id { get; set; }
        public bool IsSelected { get; set; }
    }
}
