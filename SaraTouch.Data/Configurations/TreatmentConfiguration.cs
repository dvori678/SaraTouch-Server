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
    public class TreatmentConfiguration : IEntityTypeConfiguration<Treatment>
    {
        public void Configure(EntityTypeBuilder<Treatment> builder)
        {
            builder.ToTable("Treatments");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Description).HasMaxLength(500);
            builder.Property(t => t.Price).HasPrecision(10, 2);

            // many-to-many: טיפול <-> תגית, דרך טבלת הקשר TreatmentTags
            builder.HasMany(t => t.Tags)
                   .WithMany(g => g.Treatments)
                   .UsingEntity(j => j.ToTable("TreatmentTags"));
        }
    }
}
