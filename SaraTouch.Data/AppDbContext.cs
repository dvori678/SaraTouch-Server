using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Treatment> Treatments => Set<Treatment>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // טוען אוטומטית את כל קובצי ה-Configuration שבתיקייה Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // לכל סלוט שהשתנה מוגרלת גרסה חדשה.
            // EF ישים את הגרסה הישנה ב-WHERE של ה-UPDATE. אם מישהו אחר כבר שמר,
            // הגרסה בבסיס הנתונים כבר לא תואמת, ונזרקת DbUpdateConcurrencyException.
            foreach (var entry in ChangeTracker.Entries<TimeSlot>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.Version = Guid.NewGuid();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
