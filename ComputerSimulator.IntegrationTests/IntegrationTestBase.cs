using System;
using System.Linq;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Repositories;
using ComputerSimulator.Core.Services;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class IntegrationTestBase : HostTestBase
{
    private IWireService _wireService = null!;
    private ComputerSettings _computerSettings = null!;
    private IWireRepository _wireRepository = null!;

    [OneTimeSetUp]
    public void IntegrationOneTimeSetUp()
    {
        _computerSettings = GetRequiredService<ComputerSettings>();
        _wireService = GetRequiredService<IWireService>();
        _wireRepository = GetRequiredService<IWireRepository>();
        GetRequiredService<IComponentFactory2>();
    }
    
    protected IWire2<T> CreateTestWire<T>(string label, T initialValue)
    {
        return _wireService.Create($"{Guid.NewGuid()}-{label}", initialValue);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(string label, T initialValue)
    {
        return CreateTestWireGroup(label, initialValue, _computerSettings.WordSize);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(string label, T initialValue, int size)
    {
        return _wireService.CreateGroup($"{Guid.NewGuid()}-{label}", initialValue, size);
    }

    protected IWire2<T> GetWireById<T>(string label)
    {
        return _wireRepository.Wires
            .OfType<IWire2<T>>()
            .First(f => f.Label == label);
    }

    protected static string GetInternalWireLabel(IComponent2 component, string subLabel)
    {
        return $"{component.GetType().Name}-{component.Id}-{subLabel}";
    }
}