using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

public static class EnumHelper
{
    public static string GetDisplayName<TEnum>(TEnum value) where TEnum : Enum
    {
        if (value == null) return string.Empty;

        var type = typeof(TEnum);
        var member = type.GetMember(value.ToString()).FirstOrDefault();
        if (member == null) return value.ToString();

        var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
        return displayAttr?.Name ?? value.ToString();
    }
}
