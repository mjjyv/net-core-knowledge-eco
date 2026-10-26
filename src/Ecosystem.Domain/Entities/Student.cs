using Ecosystem.Domain.Common;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Thực thể Sinh viên phục vụ bài lab thực hành và ôn tập C# (OOP, LINQ, async/await).
/// </summary>
public class Student : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public double GPA { get; set; } = 0.0;

    public Student()
    {
    }

    public Student(string name, string major, double gpa)
    {
        Name = name;
        Major = major;
        GPA = gpa;
    }
}
