using Ecosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecosystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình Fluent API cho bảng ClassroomSessions.
/// </summary>
public class ClassroomSessionConfiguration : IEntityTypeConfiguration<ClassroomSession>
{
    public void Configure(EntityTypeBuilder<ClassroomSession> builder)
    {
        builder.ToTable("ClassroomSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.RoomCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.RoomCode)
            .IsUnique()
            .HasDatabaseName("IX_ClassroomSessions_RoomCode");

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Status)
            .IsRequired();

        builder.HasIndex(s => s.HostId)
            .HasDatabaseName("IX_ClassroomSessions_HostId");

        builder.HasOne(s => s.Host)
            .WithMany(u => u.HostedSessions)
            .HasForeignKey(s => s.HostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
