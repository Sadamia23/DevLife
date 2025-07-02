using DevLife.Models.DevDating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Match)
               .WithMany(m => m.ChatMessages)
               .HasForeignKey(e => e.MatchId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Sender)
               .WithMany()
               .HasForeignKey(e => e.SenderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
