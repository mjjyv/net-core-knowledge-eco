using Ecosystem.Domain.Common;
using Ecosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Data;

/// <summary>
/// Lớp trung tâm quản lý phiên làm việc với Cơ sở Dữ liệu cho The Ecosystem (EF Core DbContext).
/// Tích hợp Fluent API, Tự động Audit Timestamps và Bộ lọc toàn cục Soft-Delete.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Aspect> Aspects => Set<Aspect>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ClassroomSession> ClassroomSessions => Set<ClassroomSession>();
    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Tự động áp dụng tất cả các cấu hình IEntityTypeConfiguration<T> trong Assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // 2. Cấu hình Global Query Filter cho Soft Delete (bỏ qua bản ghi đã bị đánh dấu IsDeleted = true)
        modelBuilder.Entity<Topic>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Aspect>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Article>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ClassroomSession>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Tự động cập nhật thời gian CreatedAt khi thêm mới và UpdatedAt khi cập nhật.
    /// </summary>
    private void ApplyAuditTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
