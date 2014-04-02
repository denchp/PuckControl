using PuckControl.Data.Dat;
using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: CLSCompliant(true)]
namespace PuckControl.Scoring
{
    public class LocalScorekeeper : IScorekeeper
    {
        public string Name { get { return "Local"; } }
        private IRepository<Score> _repository;
        private IRepository<User> _userRepository;

        public LocalScorekeeper()
        {
            _repository = new DatRepository<Score>();
            _userRepository = new DatRepository<User>();
        }

        public IEnumerable<Score> GetScores(string game, int count, int offset, DateTime? since = null)
        {
            //var scores = new IEnumerable<Score>();
            var scores = _repository.All;

            int rank = 0;
            foreach (var score in scores)
            {
                score.Rank = ++rank;
            }

            if (!String.IsNullOrEmpty(game))
                scores = scores.Where(x => x.Game == game).ToList();

            if (since != null)
                scores = scores.Where(x => x.Created > since).ToList();

            scores = scores.Skip(offset).ToList();

            if (count > 0)
                scores = scores.Take(count).ToList();

            foreach (var score in scores)
            {
                score.UserName = _userRepository.Find(x => x.Id == score.UserId).First().Name;
            }

            return scores;
        }

        public void SaveScore(Score newScore)
        {
            var scoreList = new List<Score>() { newScore };
            _repository.Save(scoreList);
        }
    }
}
