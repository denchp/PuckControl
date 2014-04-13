using PuckControl.Data.CE;
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
        private IDataService _dataService;

        public LocalScorekeeper(IDataService dataService)
        {
            _dataService = dataService;
        }

        public IEnumerable<Score> GetScores(string game, int count, int offset, DateTime? since = null)
        {
            //var scores = new IEnumerable<Score>();
            var scores = _dataService.ScoreRepository.All;
            scores = scores.OrderByDescending(x => x.FinalScore);

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
                score.User = _dataService.UserRepository.Find(x => x.Id == score.User.Id).First(x => x.Id == score.User.Id);
            }

            return scores;
        }

        public void SaveScore(Score newScore)
        {
            var scoreList = new List<Score>() { newScore };
            _dataService.ScoreRepository.Save(scoreList);
        }
    }
}
