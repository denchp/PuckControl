using System;

namespace PuckControl.Domain.Entities
{
    [Serializable]
    public class Score : AbstractEntity
    {
        public int UserId { get; set; }
        private int _finalScore;
        public int FinalScore
        {
            get
            {
                return _finalScore;
            }
            set
            {
                _finalScore = value;
            }
        }
        public String Game { get; set; }
        public int? Rank { get; set; }
        private int _monitor;
        public int Monitor { get { return _monitor; } set { _monitor = value; } }

        [NonSerialized]
        private string _userName;
        public String UserName { get { return _userName; } set { _userName = value; } }
        
        public String DateString { get { return Created.ToShortDateString(); } }
    }
}
