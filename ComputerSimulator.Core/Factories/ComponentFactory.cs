using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ComputerSimulator.Core.Factories;

public interface IComponentFactory
{
    public T Create<T>() where T : IComponent;

    public T[] CreateSet<T>() where T : IComponent;
}

public class ComponentFactory : IComponentFactory
{
    private readonly ComputerSettings _computerSettings;
    private readonly IServiceProvider _serviceProvider;

    public ComponentFactory(
        ComputerSettings computerSettings,
        IServiceProvider serviceProvider)
    {
        _computerSettings = computerSettings;
        _serviceProvider = serviceProvider;
    }
    
    public T Create<T>() where T : IComponent
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public T[] CreateSet<T>() where T : IComponent
    {
        return _computerSettings
            .InitArray<T>()
            .Fill(Create<T>);
    }
}