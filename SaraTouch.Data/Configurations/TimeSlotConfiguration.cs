using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data.Configurations
{
    public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
    {
        public void Configure(EntityTypeBuilder<TimeSlot> builder)
        {
            builder.ToTable("TimeSlots");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.StartUtc).IsRequired();
            builder.Property(s => s.EndUtc).IsRequired();
            builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

            // Optimistic concurrency: הגרסה נכנסת לתנאי ה-WHERE בכל UPDATE
            builder.Property(s => s.Version).IsConcurrencyToken();

            builder.HasIndex(s => s.StartUtc);
        }
    }
}
