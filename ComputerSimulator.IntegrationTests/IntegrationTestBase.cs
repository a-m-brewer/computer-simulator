using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class IntegrationTestBase : HostTestBase
{
    [SetUp]
    public void IntegrationSetUp()
    {
        ComputerSettings = GetRequiredService<ComputerSettings>();
        WireFactory = GetRequiredService<IWireFactory>();
        ComponentFactory = GetRequiredService<IComponentFactory>();
    }

    [TearDown]
    public void IntergrationTearDown()
    {
        ComputerSettings = null!;
        WireFactory = null!;
        ComponentFactory = null!;
    }

    protected ComputerSettings ComputerSettings { get; private set; } = null!;
    protected IComponentFactory ComponentFactory { get; private set; } = null!;
    protected IWireFactory WireFactory { get; private set; } = null!;
    
    protected IWire<T> CreateTestWire<T>(T initialValue, string? label = null) where T : new()
    {
        return WireFactory.CreateWire(initialValue, label);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(T initialValue, string? label = null) where T : new()
    {
        return CreateTestWireGroup(initialValue, ComputerSettings.WordSize, label);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(T initialValue, int size, string? label = null) where T : new()
    {
        return WireFactory.CreateGroup(initialValue, size, label);
    }

    protected IBus CreateTestBus(string? label = null)
    {
        return WireFactory.CreateBus(label);
    }

    protected IOp CreateTestOp(string? label = null)
    {
        return WireFactory.CreateOp(label);
    }
}