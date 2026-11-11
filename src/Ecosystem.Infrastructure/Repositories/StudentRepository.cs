using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Ecosystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem.Infrastructure.Repositories;

/// <summary>
/// Repository chuyên biệt cho thực thể Sinh viên (Student).
/// </summary>
public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Student>> SearchByNameAsync(string? name = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term));
        }

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetTopStudentsByGpaAsync(double minGpa, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.GPA >= minGpa)
            .OrderByDescending(s => s.GPA)
            .ToListAsync(cancellationToken);
    }
}
