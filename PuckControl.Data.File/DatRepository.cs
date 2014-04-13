using PuckControl.Domain.Entities;
using PuckControl.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

[assembly: CLSCompliant(true)]
namespace PuckControl.Data.Dat
{
    public class DatRepository<T> : IRepository<T> where T : AbstractEntity
    {
        private HashSet<T> cache;
        private string _dir;
        public DatRepository()
        {
            _dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\PuckControl\";
        }

        public IEnumerable<T> All
        {
            get
            {
                return Find(null);
            }
        }

        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            if (cache != null)
            {
                if (predicate != null)
                {
                    List<T> result = cache.Where(predicate).ToList();
                    return result;
                }
                else
                    return cache;
            }

            HashSet<T> entities = new HashSet<T>();
            string fileName = _dir + typeof(T).Name + "s.dat";

            if (!File.Exists(fileName))
            {
                cache = entities;
                return entities;
            }
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter deserializer = new BinaryFormatter();

                while (file.Position != file.Length)
                {
                    var newEntity = (T)deserializer.Deserialize(file);
                    
                    entities.Add(newEntity);
                }
            }

            cache = entities;

            if (predicate == null)
                return entities;

            return entities.Where(predicate).ToList();
        }

        public IEnumerable<T> Save(IEnumerable<T> entities)
        {
            string file = _dir + typeof(T).Name + "s.dat";

            if (cache == null)
                cache = (HashSet<T>)this.All;

            foreach (AbstractEntity entity in entities.OfType<AbstractEntity>().ToList())
            {
                if (cache.Select(x => x.Id).Contains(entity.Id))
                {
                    cache.Remove(cache.First(x => x.Id == entity.Id));
                }

                cache.Add((T)entity);

                if (entity.Id <= 0)
                {
                    entity.Id = cache.Count();
                }
            }

            if (File.Exists(file))
                File.Delete(file);

            foreach (AbstractEntity entity in entities.OfType<AbstractEntity>().ToList())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                using (FileStream fs = new FileStream(file, FileMode.Append))
                {
                    formatter.Serialize(fs, entity);
                }
            }

            return entities;
        }


        public IEnumerable<T> Find(Func<T, bool> predicate, System.Linq.Expressions.Expression<Func<T, object>> include = null)
        {
            throw new NotImplementedException();
        }


        public void Delete(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }
    }
}
