using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Exceptions;

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

    public static void SetRegisterValue(this IRegister register, IList<bool> values)
    {
        register.Set.Value = true;

        for (var i = 0; i < values.Count(); i++)
        {
            register.Inputs[i].Value = values[i];
        }
        
        register.Update();
        register.Set.Value = false;
    }
}