using DevLife.Models.GitHubAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class GitHubAnalysisResultConfiguration : IEntityTypeConfiguration<GitHubAnalysisResult>
{
    public void Configure(EntityTypeBuilder<GitHubAnalysisResult> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.PersonalityType).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PersonalityDescription).IsRequired().HasMaxLength(500);
        builder.Property(e => e.StrengthsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.WeaknessesJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.CelebrityDevelopersJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.AnalysisDetailsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.GitHubUsername).IsRequired().HasMaxLength(100);

        builder.HasIndex(e => new { e.UserId, e.AnalyzedAt });
        builder.HasIndex(e => e.GitHubUsername);
        builder.HasIndex(e => e.OverallScore);
        builder.HasIndex(e => new { e.IsPublic, e.ShareCount });
    }
}
