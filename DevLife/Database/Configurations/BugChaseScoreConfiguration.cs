using DevLife.Models.BugChase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class BugChaseScoreConfiguration : IEntityTypeConfiguration<BugChaseScore>
{
    public void Configure(EntityTypeBuilder<BugChaseScore> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.SurvivalTime)
              .HasConversion(
                  timespan => timespan.Ticks,
                  ticks => new TimeSpan(ticks));

        builder.HasIndex(e => new { e.Score, e.Distance, e.PlayedAt });
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.PlayedAt);
    }
}
