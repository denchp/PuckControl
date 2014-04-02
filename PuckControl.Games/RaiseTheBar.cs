using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;

namespace PuckControl.Games
{
    public class RaiseTheBar : AbstractGame, IGame
    {
        public override bool Init()
        {
            throw new NotImplementedException();
        }

        public override void StartGame()
        {
            throw new NotImplementedException();
        }

        public override void PuckCollision(Domain.Entities.GameObject obj)
        {
            throw new NotImplementedException();
        }

        public override int? Score
        {
            get
            {
                return null;
            }
        }
    }
}
