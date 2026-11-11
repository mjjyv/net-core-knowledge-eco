using Ecosystem.Application.DTOs;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Hợp đồng dịch vụ nghiệp vụ quản lý Sinh viên (Lab Buổi 6 &amp; Buổi 7 N-Tier).
/// </summary>
public interface IStudentService
{
    Task<IEnumerable<StudentResponseDto>> GetAllAsync(string? name = null, CancellationToken cancellationToken = default);
    Task<StudentResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentResponseDto> CreateAsync(StudentCreateDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, StudentUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
