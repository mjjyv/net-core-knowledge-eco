using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ecosystem.Application.Common;

/// <summary>
/// Tiện ích hỗ trợ chuẩn hóa và tự động tạo Slug URL chuẩn SEO (kebab-case) từ tiêu đề tiếng Việt.
/// </summary>
public static class SlugHelper
{
    /// <summary>
    /// Chuyển đổi chuỗi tiếng Việt hoặc bất kỳ tiêu đề nào sang định dạng slug URL (kebab-case).
    /// </summary>
    /// <param name="text">Tiêu đề cần chuyển đổi</param>
    /// <returns>Chuỗi slug kebab-case viết thường, không dấu, ngăn cách bằng gạch nối</returns>
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Bỏ dấu tiếng Việt qua Unicode Normalization FormD
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var clean = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        // Xử lý ký tự đặc thù tiếng Việt
        clean = clean.Replace("đ", "d");

        // Thay ký tự không phải chữ/số bằng gạch ngang
        clean = Regex.Replace(clean, @"[^a-z0-9\s-]", "");
        clean = Regex.Replace(clean, @"\s+", "-").Trim('-');

        return clean;
    }
}
