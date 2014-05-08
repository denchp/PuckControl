using PuckControl.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace PuckControl.Data.CE
{
    internal class DataContext : DbContext
    {
        public DbSet<Score> HighScores { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<SettingOption> SettingOptions { get; set; }
        public DbSet<User> Users { get; set; }

        public DataContext() : base(@"Data Source=C:\ProgramData\PuckControl\PuckControl.sdf") { }
        public DataContext(string connectionString) : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Score>()
                .HasRequired<User>(x => x.User);

            modelBuilder.Entity<Setting>()
                .HasMany<SettingOption>(x => x.Options);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            ObjectContext context = ((IObjectContextAdapter)this).ObjectContext;

            //Find all Entities that are Added/Modified that inherit from my EntityBase
            IEnumerable<ObjectStateEntry> objectStateEntries =
                from e in context.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified)
                where
                    e.IsRelationship == false &&
                    e.Entity != null &&
                    typeof(AbstractEntity).IsAssignableFrom(e.Entity.GetType())
                select e;

            var currentTime = DateTime.Now;

            foreach (var entry in objectStateEntries)
            {
                var entityBase = entry.Entity as AbstractEntity;

                if (entry.State == EntityState.Added)
                {
                    entityBase.Created = currentTime;
                }

                entityBase.Modified = currentTime;
            }

            return base.SaveChanges();
        }
    }
}
