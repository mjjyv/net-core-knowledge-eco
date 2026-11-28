using Ecosystem.Domain.Entities;

namespace Ecosystem.Domain.Interfaces;

/// <summary>
/// Repository chuyên biệt cho thực thể Sinh viên (Student - Lab thực hành Buổi 6 &amp; 7).
/// </summary>
public interface IStudentRepository : IRepository<Student>
{
    Task<IEnumerable<Student>> SearchByNameAsync(string? name = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetTopStudentsByGpaAsync(double minGpa, CancellationToken cancellationToken = default);
}
