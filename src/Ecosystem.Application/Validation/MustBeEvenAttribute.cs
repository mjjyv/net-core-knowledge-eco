using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.Validation;

/// <summary>
/// Custom validation attribute kiểm tra giá trị số nguyên phải là số chẵn (Bài tập 4 - Buổi 4).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MustBeEvenAttribute : ValidationAttribute
{
    public MustBeEvenAttribute()
    {
        ErrorMessage = "{0} phải là số chẵn.";
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        return value switch
        {
            int intValue => intValue % 2 == 0,
            long longValue => longValue % 2 == 0,
            short shortValue => shortValue % 2 == 0,
            _ => false
        };
    }
}
