using System;

namespace PuckControl.Domain.Entities
{
    [Serializable]
    public abstract class AbstractEntity
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
