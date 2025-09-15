using System.Globalization;
using System.Text;

public static class StringHelper
{
    public static string NormalizeText(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Chuẩn hóa FormD (tách ký tự base + dấu)
        var normalized = input.Normalize(NormalizationForm.FormD);

        // Bỏ hết ký tự dấu (NonSpacingMark)
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }
}
