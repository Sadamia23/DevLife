using DevLife.Models.CodeRoast;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class CodeRoastStatsConfiguration : IEntityTypeConfiguration<CodeRoastStats>
{
    public void Configure(EntityTypeBuilder<CodeRoastStats> builder)
    {
        builder.HasKey(e => e.Id);

        // Unique index on UserId
        builder.HasIndex(e => e.UserId)
            .IsUnique();

        // Relationship
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Properties
        builder.Property(e => e.RecentScoresJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.UnlockedAchievementsJson)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(e => e.AverageScore);
        builder.HasIndex(e => e.TotalSubmissions);
        builder.HasIndex(e => e.LastSubmission);
    }
}
