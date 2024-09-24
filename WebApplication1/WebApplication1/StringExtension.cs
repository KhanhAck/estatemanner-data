using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace WebApplication1;

public static class StringExtension
{
    public static string ToSlug(this string source)
    {
        return ToSlug(source, int.MaxValue);
    }

    public static string ToSlug(this string source, int maxLength)
    {
        source = !string.IsNullOrEmpty(source) ? source : "";
        string str = RemoveSign4VietnameseString(source.ToLower());

        // invalid chars, make into spaces
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        // convert multiple spaces/hyphens into one space       
        str = Regex.Replace(str, @"[\s-]+", " ").Trim();
        // cut and trim it
        str = str.Substring(0, str.Length <= maxLength ? str.Length : maxLength).Trim();
        // hyphens
        str = Regex.Replace(str, @"\s", "-");

        return str;
    }

    public static string RemoveSign4VietnameseString(this string source)
    {
        string[] vietnameseSigns =
        {
                "aAeEoOuUiIdDyY", "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ", "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ", "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ", "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ", "íìịỉĩ", "ÍÌỊỈĨ",
                "đ", "Đ", "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };

        //Tiến hành thay thế , lọc bỏ dấu cho chuỗi
        for (int i = 1; i < vietnameseSigns.Length; i++)
        {
            for (int j = 0; j < vietnameseSigns[i].Length; j++)
                source = source.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
        }

        return source;
    }

}