using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(e => e.DeadlineUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.MaxMarks)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(AssignmentStatus.Draft);

        builder.Property(e => e.AllowResubmission)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(e => e.ClassId)
            .HasDatabaseName("IX_Assignments_ClassId");

        builder.HasIndex(e => e.SubjectId)
            .HasDatabaseName("IX_Assignments_SubjectId");

        builder.HasIndex(e => e.TeacherId)
            .HasDatabaseName("IX_Assignments_TeacherId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Assignments_Status");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Class>()
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_assignments_max_marks_positive", "\"max_marks\" > 0"));
    }
}
