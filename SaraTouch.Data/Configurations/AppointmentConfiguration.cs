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
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(a => a.CreatedAtUtc).IsRequired();

            // Restrict: אי אפשר למחוק משתמשת, טיפול או סלוט שיש עליהם תורים
            builder.HasOne(a => a.User)
                   .WithMany(u => u.Appointments)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Treatment)
                   .WithMany(t => t.Appointments)
                   .HasForeignKey(a => a.TreatmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.TimeSlot)
                   .WithMany(s => s.Appointments)
                   .HasForeignKey(a => a.TimeSlotId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
