using DevLife.Models.CodeRoast;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class CodeRoastTaskConfiguration : IEntityTypeConfiguration<CodeRoastTask>
{
    public void Configure(EntityTypeBuilder<CodeRoastTask> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Requirements)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.TechStack)
            .HasConversion<string>();

        builder.Property(e => e.DifficultyLevel)
            .HasConversion<string>();

        builder.Property(e => e.StarterCode)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.TestCasesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ExamplesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Topic)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => new { e.TechStack, e.DifficultyLevel, e.IsActive });
        builder.HasIndex(e => e.CreatedAt);
    }
}
