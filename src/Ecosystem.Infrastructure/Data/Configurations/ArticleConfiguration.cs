using Ecosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecosystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình Fluent API cho bảng Articles (Bài viết tri thức Markdown cấp 4).
/// </summary>
public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(280);

        builder.HasIndex(a => a.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Articles_Slug");

        builder.Property(a => a.Summary)
            .HasMaxLength(1000);

        builder.Property(a => a.MarkdownContent)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.ViewCount)
            .HasDefaultValue(0);

        builder.Property(a => a.ReadingTimeMinutes)
            .HasDefaultValue(0);

        builder.Property(a => a.TagsJson)
            .IsRequired()
            .HasMaxLength(2000)
            .HasDefaultValue("[]");

        // Indexes hỗ trợ truy vấn, tìm kiếm và phân trang
        builder.HasIndex(a => a.CategoryId)
            .HasDatabaseName("IX_Articles_CategoryId");

        builder.HasIndex(a => a.AuthorId)
            .HasDatabaseName("IX_Articles_AuthorId");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_Articles_Status");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_Articles_CreatedAt");

        // Quan hệ 1-N với Category
        builder.HasOne(a => a.Category)
            .WithMany(c => c.Articles)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quan hệ 1-N với Author (User)
        builder.HasOne(a => a.Author)
            .WithMany(u => u.Articles)
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
