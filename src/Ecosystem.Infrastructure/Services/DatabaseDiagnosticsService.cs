using Ecosystem.Application.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Services;

/// <summary>
/// Dịch vụ tầng Infrastructure thực hiện kiểm tra kết nối và đếm số lượng bản ghi các bảng thông qua EF Core.
/// </summary>
public class DatabaseDiagnosticsService : IDatabaseDiagnosticsService
{
    private readonly ApplicationDbContext _context;

    public DatabaseDiagnosticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DatabaseStatusReport> GetDatabaseStatusAsync(CancellationToken cancellationToken = default)
    {
        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return new DatabaseStatusReport
            {
                CanConnect = false,
                Provider = _context.Database.ProviderName
            };
        }

        var provider = _context.Database.ProviderName;
        var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);

        var counts = new DatabaseRecordCounts
        {
            Topics = await _context.Topics.CountAsync(cancellationToken),
            Aspects = await _context.Aspects.CountAsync(cancellationToken),
            Categories = await _context.Categories.CountAsync(cancellationToken),
            Articles = await _context.Articles.CountAsync(cancellationToken),
            Users = await _context.Users.CountAsync(cancellationToken),
            Students = await _context.Students.CountAsync(cancellationToken),
            ClassroomSessions = await _context.ClassroomSessions.CountAsync(cancellationToken)
        };

        return new DatabaseStatusReport
        {
            CanConnect = true,
            Provider = provider,
            AppliedMigrations = appliedMigrations,
            RecordCounts = counts
        };
    }
}
