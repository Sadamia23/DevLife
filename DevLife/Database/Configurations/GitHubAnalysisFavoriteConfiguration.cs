using DevLife.Models.GitHubAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class GitHubAnalysisFavoriteConfiguration : IEntityTypeConfiguration<GitHubAnalysisFavorite>
{
    public void Configure(EntityTypeBuilder<GitHubAnalysisFavorite> builder)
    {
        builder.HasKey(e => e.Id);

        // Use NoAction to avoid cascade conflict
        builder.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.GitHubAnalysisResult)
              .WithMany(r => r.Favorites)
              .HasForeignKey(e => e.GitHubAnalysisResultId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.CustomName).HasMaxLength(200);

        builder.HasIndex(e => new { e.UserId, e.GitHubAnalysisResultId }).IsUnique();
        builder.HasIndex(e => new { e.UserId, e.SavedAt });
    }
}
