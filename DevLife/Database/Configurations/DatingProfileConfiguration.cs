using DevLife.Models.DevDating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class SwipeActionConfiguration : IEntityTypeConfiguration<SwipeAction>
{
    public void Configure(EntityTypeBuilder<SwipeAction> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.SwiperId, e.SwipedUserId }).IsUnique();
        builder.HasOne(e => e.Swiper)
               .WithMany()
               .HasForeignKey(e => e.SwiperId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.SwipedUser)
               .WithMany()
               .HasForeignKey(e => e.SwipedUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
