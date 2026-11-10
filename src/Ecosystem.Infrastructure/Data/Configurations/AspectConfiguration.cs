using Ecosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecosystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình Fluent API cho bảng Aspects (Khía cạnh chuyên sâu cấp 2).
/// </summary>
public class AspectConfiguration : IEntityTypeConfiguration<Aspect>
{
    public void Configure(EntityTypeBuilder<Aspect> builder)
    {
        builder.ToTable("Aspects");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(a => a.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Aspects_Slug");

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.OrderIndex)
            .HasDefaultValue(0);

        builder.HasIndex(a => a.TopicId)
            .HasDatabaseName("IX_Aspects_TopicId");

        // Quan hệ 1-N: Một Aspect thuộc về một Topic
        builder.HasOne(a => a.Topic)
            .WithMany(t => t.Aspects)
            .HasForeignKey(a => a.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ 1-N: Một Aspect có nhiều Categories
        builder.HasMany(a => a.Categories)
            .WithOne(c => c.Aspect)
            .HasForeignKey(c => c.AspectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
