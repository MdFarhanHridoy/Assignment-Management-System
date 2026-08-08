using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("UX_Users_Email");

        builder.HasIndex(e => e.Email)
            .HasDatabaseName("IX_Users_Email");
    }
}
