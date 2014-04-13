using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PuckControl.Domain.Entities
{
    [Serializable]
    public class User : AbstractEntity
    {
        public string Name { get; set; }
        public int BirthYear { get; set; }

        public byte[] Avatar { get; set; }

        [NotMapped]
        public UserType UserType { get; set; }
    }
}
