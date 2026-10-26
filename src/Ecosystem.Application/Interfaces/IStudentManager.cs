using Ecosystem.Domain.Entities;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Giao diện hợp đồng quản lý danh sách sinh viên theo chuẩn bài thực hành Buổi 1.
/// </summary>
public interface IStudentManager
{
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Student> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetTopStudentsAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> FilterByMajorAndGpaAsync(string major, double minGpa, CancellationToken cancellationToken = default);
    Task AddStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task RemoveStudentAsync(Guid id, CancellationToken cancellationToken = default);
}
