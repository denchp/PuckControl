using PuckControl.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PuckControl.Domain.Interfaces
{
    public interface IRepository<T> where T : AbstractEntity
    {
        IEnumerable<T> All { get; }
        IEnumerable<T> Find(Func<T, bool> predicate, Expression<Func<T, object>> include = null);
        IEnumerable<T> Save(IEnumerable<T> entities);
        void Delete(IEnumerable<T> entities);
    }
}
