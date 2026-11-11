using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ quản lý Sinh viên tuân thủ N-Tier Architecture (Lab Buổi 7).
/// Controller gọi StudentService, Service gọi IUnitOfWork.Students và ánh xạ bằng AutoMapper.
/// </summary>
public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<StudentService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger ?? NullLogger<StudentService>.Instance;
    }

    public async Task<IEnumerable<StudentResponseDto>> GetAllAsync(string? name = null, CancellationToken cancellationToken = default)
    {
        var students = await _unitOfWork.Students.SearchByNameAsync(name, cancellationToken);
        return _mapper.Map<IEnumerable<StudentResponseDto>>(students);
    }

    public async Task<StudentResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(id, cancellationToken);
        return student == null ? null : _mapper.Map<StudentResponseDto>(student);
    }

    public async Task<StudentResponseDto> CreateAsync(StudentCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Thêm mới sinh viên {StudentName}, chuyên ngành {Major}", dto.Name, dto.Major);

        var student = new Student(dto.Name.Trim(), dto.Major.Trim(), dto.GPA)
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Students.AddAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StudentResponseDto>(student);
    }

    public async Task<bool> UpdateAsync(Guid id, StudentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(id, cancellationToken);
        if (student == null)
        {
            _logger.LogWarning("Không tìm thấy sinh viên {StudentId} để cập nhật", id);
            return false;
        }

        student.Name = dto.Name.Trim();
        student.Major = dto.Major.Trim();
        student.GPA = dto.GPA;

        _unitOfWork.Students.Update(student);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã cập nhật sinh viên {StudentId}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(id, cancellationToken);
        if (student == null)
        {
            return false;
        }

        _unitOfWork.Students.Remove(student);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã xóa sinh viên {StudentId}", id);
        return true;
    }
}
