using System;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class IntegrationTestBase : HostTestBase
{
    private IWire2Factory _wireFactory = null!;
    private IComponentFactory2 _componentFactory = null!;
    private ComputerSettings _computerSettings = null!;

    [OneTimeSetUp]
    public void IntegrationOneTimeSetUp()
    {
        _computerSettings = GetRequiredService<ComputerSettings>();
        _wireFactory = GetRequiredService<IWire2Factory>();
        _componentFactory = GetRequiredService<IComponentFactory2>();
    }
    
    protected IWire2<T> CreateTestWire<T>(string label, T initialValue)
    {
        return _wireFactory.Create($"{Guid.NewGuid()}-{label}", initialValue);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(string label, T initialValue)
    {
        return CreateTestWireGroup(label, initialValue, _computerSettings.WordSize);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(string label, T initialValue, int size)
    {
        return _wireFactory.CreateGroup($"{Guid.NewGuid()}-{label}", initialValue, size);
    }
}