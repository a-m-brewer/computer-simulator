using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ComputerSimulator.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCoreServices(
        this IServiceCollection services,
        ComputerSettings computerSettings)
    {
        services.Scan(s => s.FromAssemblyOf<IComponent>()
            .AddClasses(f => f.AssignableTo<IComponent>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        services.AddTransient<IComputer, Computer>();

        services.AddSingleton<IWireFactory, WireFactory>();
        services.AddTransient<IComponentFactory, ComponentFactory>();
        services.AddSingleton<RightShifterWireFactory>();
        services.AddSingleton<LeftShifterWireFactory>();
        
        services.AddSingleton(computerSettings);

        return services;
    }
}