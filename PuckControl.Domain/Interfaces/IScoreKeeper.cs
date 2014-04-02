using PuckControl.Domain.Entities;
using System;
using System.Collections.Generic;

namespace PuckControl.Domain.Interfaces
{
    public interface IScorekeeper
    {
        IEnumerable<Score> GetScores(string game, int count, int offset, DateTime? since = null);
        void SaveScore(Score newScore);
        string Name { get; }
    }
}
