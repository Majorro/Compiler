namespace Compiler.Utils;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).FirstOrDefault();
        return attribute is null ? value.ToString() : ((System.ComponentModel.DescriptionAttribute)attribute).Description;
    }
}