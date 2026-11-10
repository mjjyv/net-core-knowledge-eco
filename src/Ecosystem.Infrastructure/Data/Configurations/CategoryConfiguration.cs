using Ecosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecosystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình Fluent API cho bảng Categories (Chuyên mục cụ thể cấp 3).
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Categories_Slug");

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.OrderIndex)
            .HasDefaultValue(0);

        builder.HasIndex(c => c.AspectId)
            .HasDatabaseName("IX_Categories_AspectId");

        // Quan hệ 1-N: Một Category thuộc về một Aspect
        builder.HasOne(c => c.Aspect)
            .WithMany(a => a.Categories)
            .HasForeignKey(c => c.AspectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ 1-N: Một Category có nhiều Articles (Restrict Delete để bảo vệ nội dung bài viết)
        builder.HasMany(c => c.Articles)
            .WithOne(a => a.Category)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
