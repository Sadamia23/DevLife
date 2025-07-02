using DevLife.Models.CodeRoast;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevLife.Database.Configurations;

public class CodeRoastSubmissionConfiguration : IEntityTypeConfiguration<CodeRoastSubmission>
{
    public void Configure(EntityTypeBuilder<CodeRoastSubmission> builder)
    {
        builder.HasKey(e => e.Id);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Task)
            .WithMany(t => t.Submissions)
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Properties
        builder.Property(e => e.SubmittedCode)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RoastMessage)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.TechnicalFeedback)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.PositivePointsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ImprovementPointsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RedFlagsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.DetectedPatternsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.CodeSmellsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RoastSeverity)
            .HasConversion<string>();

        // Indexes
        builder.HasIndex(e => new { e.UserId, e.SubmittedAt });
        builder.HasIndex(e => e.OverallScore);
        builder.HasIndex(e => new { e.TaskId, e.OverallScore });
    }
}
