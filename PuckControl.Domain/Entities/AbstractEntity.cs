using System;
using System.ComponentModel.DataAnnotations;

namespace PuckControl.Domain.Entities
{
    [Serializable]
    public abstract class AbstractEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
