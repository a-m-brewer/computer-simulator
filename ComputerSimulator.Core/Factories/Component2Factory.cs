using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ComputerSimulator.Core.Factories;

public interface IComponentFactory2
{
    public T Create<T>() where T : IComponent2;

    public T[] CreateSet<T>() where T : IComponent2;
}

public class ComponentFactory2 : IComponentFactory2
{
    private readonly ComputerSettings _computerSettings;
    private readonly IServiceProvider _serviceProvider;

    public ComponentFactory2(
        ComputerSettings computerSettings,
        IServiceProvider serviceProvider)
    {
        _computerSettings = computerSettings;
        _serviceProvider = serviceProvider;
    }
    
    public T Create<T>() where T : IComponent2
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public T[] CreateSet<T>() where T : IComponent2
    {
        return _computerSettings
            .InitArray<T>()
            .Fill(Create<T>);
    }
}