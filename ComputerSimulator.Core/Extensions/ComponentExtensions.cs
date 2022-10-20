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
    
    public static void Update<T>(this Dictionary<int, T> components)
        where T : IComponent
    {
        foreach (var component in components.Values)
        {
            component.Update();
        }
    }
}