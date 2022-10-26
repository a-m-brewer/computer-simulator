using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class ShifterTests : IntegrationTestBase
{
    [Test]
    public void PerformsRightShift()
    {
        // Arrange
        const int input = 0x42;
        var inputBools = input.ToBinaryBools(ComputerSettings.WordSize);

        var sut = ComponentFactory.CreateRightShifter(
            CreateTestWire<bool>(),
            CreateTestWire<bool>(),
            CreateTestWireGroup<bool>(),
            CreateTestWireGroup<bool>());
            
        // Act
        for (var i = 0; i < sut.Input.Count; i++)
        {
            sut.Input[i].Value = inputBools[i];
        }
            
        sut.Update();
            
        // Assert
        const int expected = 0x21;

        sut.Output
            .ToInt()
            .Should()
            .Be(expected);
    }
    
    [Test]
    public void PerformsLeftShift()
    {
        // Arrange
        const int input = 0x42;
        var inputBools = input.ToBinaryBools(ComputerSettings.WordSize);

        var sut = ComponentFactory.CreateLeftShifter(
            CreateTestWire<bool>(),
            CreateTestWire<bool>(),
            CreateTestWireGroup<bool>(),
            CreateTestWireGroup<bool>());
            
        // Act
        for (var i = 0; i < sut.Input.Count; i++)
        {
            sut.Input[i].Value = inputBools[i];
        }
            
        sut.Update();
            
        // Assert
        const int expected = 0x84;

        sut.Output
            .ToInt()
            .Should()
            .Be(expected);
    }
}