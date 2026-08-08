using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EnrolledAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(e => new { e.ClassId, e.StudentId })
            .IsUnique()
            .HasDatabaseName("UX_Enrollments_ClassId_StudentId");

        builder.HasOne<Class>()
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
