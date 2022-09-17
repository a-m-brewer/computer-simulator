namespace ComputerSimulator.Core.Extensions;

public static class ComponentExtensions
{
    public static void Update(this IEnumerable<IComponent> components)
    {
        foreach (var component in components)
        {
            component.Update();
        }
    }
}