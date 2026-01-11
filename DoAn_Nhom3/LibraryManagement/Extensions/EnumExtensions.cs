using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LibraryManagement.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum? value)
    {
        if (value == null) return string.Empty;

        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        if (member == null) return value.ToString();

        var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
        return displayAttr?.GetName() ?? value.ToString();
    }
}