using DevLife.Models.BugChase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class BugChaseStatsConfiguration : IEntityTypeConfiguration<BugChaseStats>
{
    public void Configure(EntityTypeBuilder<BugChaseStats> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.TotalSurvivalTime)
              .HasConversion(
                  timespan => timespan.Ticks,
                  ticks => new TimeSpan(ticks));
    }
}
