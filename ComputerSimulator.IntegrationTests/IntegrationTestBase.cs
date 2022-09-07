using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class IntegrationTestBase : HostTestBase
{
    private IWireFactory _wireFactory = null!;

    [SetUp]
    public void IntegrationSetUp()
    {
        ComputerSettings = GetRequiredService<ComputerSettings>();
        _wireFactory = GetRequiredService<IWireFactory>();
        ComponentFactory = GetRequiredService<IComponentFactory>();
    }

    [TearDown]
    public void IntergrationTearDown()
    {
        ComputerSettings = null!;
        _wireFactory = null!;
        ComponentFactory = null!;
    }

    protected ComputerSettings ComputerSettings { get; private set; } = null!;
    protected IComponentFactory ComponentFactory { get; private set; } = null!;
    
    protected IWire<T> CreateTestWire<T>(T initialValue)
    {
        return _wireFactory.CreateWire(initialValue);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(T initialValue)
    {
        return CreateTestWireGroup(initialValue, ComputerSettings.WordSize);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(T initialValue, int size)
    {
        return _wireFactory.CreateGroup(initialValue, size);
    }

    protected IBus CreateTestBus()
    {
        return _wireFactory.CreateBus();
    }

    protected IOp CreateTestOp()
    {
        return _wireFactory.CreateOp();
    }
}