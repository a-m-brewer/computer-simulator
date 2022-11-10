using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

[TestFixture]
public class DisplayRamTests : IntegrationTestBase
{
    private IDisplayRam _sut;
    private bool[] _max;
    private int _maxInt;

    [SetUp]
    public void SetUp()
    {
        var computerSettings = GetRequiredService<ComputerSettings>();

        _max = computerSettings.WordSize.InitArray<bool>().Fill(true);
        _maxInt = _max.ToInt();

        _sut = ComponentFactory.CreateDisplayRam(
            WireFactory.CreateWire<bool>("set-mar-set"),
            WireFactory.CreateWire<bool>("enable-mar-set"),
            WireFactory.CreateGroup<bool>("set-mar-input-bus"),
            WireFactory.CreateGroup<bool>("enable-mar-input-bus"),
            WireFactory.CreateWire<bool>("set"),
            WireFactory.CreateWire<bool>("enable"),
            WireFactory.CreateGroup<bool>("input-bus"),
            WireFactory.CreateGroup<bool>("output-bus")
        );
    }

    [Test]
    public void CanStoreAValueFromTheInputBus()
    {
        // Arrange
        const int ramAddress = 1;

        _sut.SetMar.SetRegisterValue(ramAddress.ToBinaryBools());
        
        _sut.InputBus.SetValue(_max);

        _sut.Set.Value = true;
        
        // Act
        _sut.Update();
        
        // Assert
        _sut.GetSlot(1, 0)
            .Memory
            .StoredValue
            .ToInt()
            .Should()
            .Be(_maxInt);
    }
    
    [Test]
    public void CanOutputAValueFromRam()
    {
        // Arrange
        const int ramAddress = 2;

        _sut.GetSlot(ramAddress, 0)
            .Memory
            .SetRegisterValue(_max);
        
        _sut.EnableMar.SetRegisterValue(ramAddress.ToBinaryBools(2));

        _sut.Enable.Value = true;
        
        // Act
        _sut.Update();
        
        // Assert
        _sut.OutputBus
            .ToInt()
            .Should()
            .Be(_maxInt);
    }
}