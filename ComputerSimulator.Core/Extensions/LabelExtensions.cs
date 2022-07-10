namespace ComputerSimulator.Core.Extensions;

public static class LabelExtensions
{
    public static string GenerateLabel(this ILabel obj, string value)
    {
        return $"{obj.Label}-{value}";
    }
}