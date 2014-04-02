using System;
using System.IO;

namespace PuckControl.Domain.Entities
{
    [Serializable]
    public class User : AbstractEntity
    {
        public string Name { get; set; }
        public int BirthYear { get; set; }
        public Stream Avatar { get; set; }
        public UserType UserType { get; set; }
    }
}
