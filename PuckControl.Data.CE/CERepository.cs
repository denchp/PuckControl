using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

[assembly: CLSCompliant(true)]
namespace PuckControl.Data.CE
{
    class CERepository<T> : IRepository<T> where T : AbstractEntity
    {
        DataContext _context;

        public CERepository(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<T> All
        {
            get {
                return _context.Set<T>().ToList();
            }
        }

        public IEnumerable<T> Find(Func<T, bool> predicate, Expression<Func<T, object>> include = null)
        {
            IQueryable<T> entities;
            entities = _context.Set<T>().AsQueryable();
            
            if (include != null)
                entities.Include(include);

            return entities.Where(predicate).ToList();
        }

        public IEnumerable<T> Save(IEnumerable<T> entities)
        {
            HashSet<T> savedEntities = new HashSet<T>();

            foreach (var entity in entities)
            {
                if (entity.Id > 0)
                {
                    var dataEntity = _context.Set<T>().First(x => x.Id == entity.Id);
                    dataEntity = entity;
                    savedEntities.Add(entity);
                }
                else
                {
                    _context.Set<T>().Add(entity);
                    savedEntities.Add(entity);
                }
            }

            if (_context.SaveChanges() > 0)
                return savedEntities;

            return new HashSet<T>();
        }

        public void Delete(IEnumerable<T> entities)
        {
            var deleteList = entities.ToList();
            foreach (var entity in deleteList)
            {
                _context.Set<T>().Remove(entity);
            }

            _context.SaveChanges();
        }
    }
}
