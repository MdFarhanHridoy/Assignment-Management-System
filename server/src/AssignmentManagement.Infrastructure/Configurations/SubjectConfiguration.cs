using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(e => new { e.ClassId, e.Name })
            .IsUnique()
            .HasDatabaseName("UX_Subjects_ClassId_Name");

        builder.HasIndex(e => e.ClassId)
            .HasDatabaseName("IX_Subjects_ClassId");

        builder.HasOne<Class>()
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
