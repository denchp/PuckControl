using System;

namespace PuckControl.Domain.Entities
{
    [Serializable]
    public class Score : AbstractEntity
    {
        public int FinalScore { get; set; }
        public string Game { get; set; }
        public int? Rank { get; set; }
        public int Monitor { get; set; }

        public User User { get; set; }
        public String DateString { get { return Created.ToShortDateString(); } }
    }
}
