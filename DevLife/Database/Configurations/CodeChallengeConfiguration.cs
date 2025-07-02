using DevLife.Models.CodeCasino;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class CodeChallengeConfiguration : IEntityTypeConfiguration<CodeChallenge>
{
    public void Configure(EntityTypeBuilder<CodeChallenge> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.CorrectCode).IsRequired();
        builder.Property(e => e.BuggyCode).IsRequired();
        builder.Property(e => e.Explanation).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.TechStack).HasConversion<string>();
        builder.Property(e => e.DifficultyLevel).HasConversion<string>();
    }
}
