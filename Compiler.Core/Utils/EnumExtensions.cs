using System.ComponentModel;

namespace Compiler.Core.Utils;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
        return attribute is null ? value.ToString() : ((DescriptionAttribute)attribute).Description;
    }
}