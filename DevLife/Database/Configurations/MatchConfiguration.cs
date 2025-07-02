using DevLife.Models.DevDating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.User1Id, e.User2Id }).IsUnique();
        builder.HasOne(e => e.User1)
               .WithMany()
               .HasForeignKey(e => e.User1Id)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.User2)
               .WithMany()
               .HasForeignKey(e => e.User2Id)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
