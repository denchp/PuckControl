using PuckControl.Domain.Entities;
using System;
using System.Collections.Generic;

namespace PuckControl.Domain.Interfaces
{
    public interface IRepository<T> where T : AbstractEntity
    {
        IEnumerable<T> All { get; }
        IEnumerable<T> Find(Func<T, bool> predicate);
        IEnumerable<T> Save(IEnumerable<T> entities);
    }
}
