using DevLife.Models.MeetingExcuse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class MeetingExcuseConfiguration : IEntityTypeConfiguration<MeetingExcuse>
{
    public void Configure(EntityTypeBuilder<MeetingExcuse> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExcuseText).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Category).HasConversion<string>();
        builder.Property(e => e.Type).HasConversion<string>();
        builder.Property(e => e.BelievabilityScore).IsRequired();
        builder.Property(e => e.TagsJson).HasColumnType("nvarchar(1000)");
        builder.Property(e => e.UsageCount).HasDefaultValue(0);
        builder.Property(e => e.AverageRating).HasDefaultValue(0.0);
        builder.Property(e => e.RatingCount).HasDefaultValue(0);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => new { e.Category, e.Type, e.IsActive });
        builder.HasIndex(e => new { e.BelievabilityScore, e.IsActive });
        builder.HasIndex(e => new { e.AverageRating, e.RatingCount });
        builder.HasIndex(e => e.UsageCount);
    }
}
