using Ecosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecosystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình Fluent API cho bảng Topics (Chủ đề lớn cấp 1).
/// </summary>
public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Topics_Slug");

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.OrderIndex)
            .HasDefaultValue(0);

        // Quan hệ 1-N: Một Topic có nhiều Aspects
        builder.HasMany(t => t.Aspects)
            .WithOne(a => a.Topic)
            .HasForeignKey(a => a.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
