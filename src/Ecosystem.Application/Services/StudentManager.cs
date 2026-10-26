using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Entities;

namespace Ecosystem.Application.Services;

/// <summary>
/// Hiện thực dịch vụ quản lý danh sách sinh viên, vận dụng LINQ, Collections, Generic và async/await.
/// </summary>
public class StudentManager : IStudentManager
{
    private readonly List<Student> _students = new();
    private readonly object _lock = new();

    public StudentManager()
    {
        // Khởi tạo sẵn danh sách sinh viên mẫu cho bài thực hành Buổi 1
        _students.AddRange(new[]
        {
            new Student("An Nguyễn", "CNTT", 3.6),
            new Student("Bình Trần", "KTPM", 2.8),
            new Student("Chi Lê", "CNTT", 3.9),
            new Student("Dũng Phạm", "HTTT", 3.1),
            new Student("Hương Đỗ", "CNTT", 3.4)
        });
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Mô phỏng thao tác I/O bất đồng bộ non-blocking
        await Task.Delay(10, cancellationToken);

        lock (_lock)
        {
            return _students.Where(s => !s.IsDeleted).ToList().AsReadOnly();
        }
    }

    public async Task<Student> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);

        lock (_lock)
        {
            var student = _students.FirstOrDefault(s => s.Id == id && !s.IsDeleted);
            if (student == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sinh viên có Id = '{id}'.");
            }
            return student;
        }
    }

    public async Task<IReadOnlyList<Student>> GetTopStudentsAsync(int count, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);

        if (count <= 0)
        {
            return Array.Empty<Student>();
        }

        lock (_lock)
        {
            return _students
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.GPA)
                .Take(count)
                .ToList()
                .AsReadOnly();
        }
    }

    public async Task<IReadOnlyList<Student>> FilterByMajorAndGpaAsync(string major, double minGpa, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);

        lock (_lock)
        {
            return _students
                .Where(s => !s.IsDeleted &&
                            s.Major.Equals(major, StringComparison.OrdinalIgnoreCase) &&
                            s.GPA >= minGpa)
                .OrderByDescending(s => s.GPA)
                .ToList()
                .AsReadOnly();
        }
    }

    public async Task AddStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);
        await Task.Delay(10, cancellationToken);

        lock (_lock)
        {
            _students.Add(student);
        }
    }

    public async Task RemoveStudentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);

        lock (_lock)
        {
            var student = _students.FirstOrDefault(s => s.Id == id && !s.IsDeleted);
            if (student == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sinh viên có Id = '{id}' để xóa.");
            }
            student.IsDeleted = true;
            student.UpdatedAt = DateTime.UtcNow;
        }
    }
}
