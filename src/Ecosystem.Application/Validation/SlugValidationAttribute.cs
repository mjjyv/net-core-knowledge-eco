using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ecosystem.Application.Validation;

/// <summary>
/// Custom validation attribute xác thực chuỗi theo định dạng chuẩn SEO kebab-case.
/// Ví dụ hợp lệ: "lap-trinh-csharp", "net-core-8", "microservices-architecture".
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class SlugValidationAttribute : ValidationAttribute
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public SlugValidationAttribute()
    {
        ErrorMessage = "{0} phải có định dạng kebab-case chuẩn (chữ thường, số, phân cách bởi dấu gạch ngang, ví dụ: 'chu-de-mau').";
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        var str = value.ToString();
        if (string.IsNullOrWhiteSpace(str))
            return true;

        return SlugRegex.IsMatch(str);
    }
}
