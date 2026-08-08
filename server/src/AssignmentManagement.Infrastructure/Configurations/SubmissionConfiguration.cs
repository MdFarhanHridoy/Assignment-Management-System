using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AnswerText)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(e => e.SubmittedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(SubmissionStatus.Submitted);

        builder.Property(e => e.Feedback)
            .HasColumnType("text");

        builder.Property(e => e.ReviewedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(e => new { e.AssignmentId, e.StudentId })
            .IsUnique()
            .HasDatabaseName("UX_Submissions_AssignmentId_StudentId");

        builder.HasIndex(e => e.StudentId)
            .HasDatabaseName("IX_Submissions_StudentId");

        builder.HasIndex(e => e.AssignmentId)
            .HasDatabaseName("IX_Submissions_AssignmentId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Submissions_Status");

        builder.HasOne<Assignment>()
            .WithMany()
            .HasForeignKey(e => e.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.ReviewedByTeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t => t.HasCheckConstraint("ck_submissions_marks_nonneg", "\"marks\" IS NULL OR \"marks\" >= 0"));
    }
}
