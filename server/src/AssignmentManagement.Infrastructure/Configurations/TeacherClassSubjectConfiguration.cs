using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Configurations;

public class TeacherClassSubjectConfiguration : IEntityTypeConfiguration<TeacherClassSubject>
{
    public void Configure(EntityTypeBuilder<TeacherClassSubject> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(e => new { e.TeacherId, e.ClassId, e.SubjectId })
            .IsUnique()
            .HasDatabaseName("UX_TeacherClassSubjects_TeacherId_ClassId_SubjectId");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Class>()
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
