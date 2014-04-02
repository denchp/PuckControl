using System.Collections.Generic;

namespace PuckControl.Domain.Entities
{
    public class ScoreTable
    {
        public string Name { get; set; }
        public IEnumerable<Score> Scores { get; set; }
    }
}
