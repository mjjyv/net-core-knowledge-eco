namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Hợp đồng dịch vụ chẩn đoán kết nối và thống kê cơ sở dữ liệu, giúp Controller không phụ thuộc trực tiếp DbContext.
/// </summary>
public interface IDatabaseDiagnosticsService
{
    Task<DatabaseStatusReport> GetDatabaseStatusAsync(CancellationToken cancellationToken = default);
}

public class DatabaseStatusReport
{
    public bool CanConnect { get; set; }
    public string? Provider { get; set; }
    public IEnumerable<string> AppliedMigrations { get; set; } = Array.Empty<string>();
    public DatabaseRecordCounts RecordCounts { get; set; } = new();
}

public class DatabaseRecordCounts
{
    public int Topics { get; set; }
    public int Aspects { get; set; }
    public int Categories { get; set; }
    public int Articles { get; set; }
    public int Users { get; set; }
    public int Students { get; set; }
    public int ClassroomSessions { get; set; }
}
