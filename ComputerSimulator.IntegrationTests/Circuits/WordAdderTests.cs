using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class WordAdderTests : IntegrationTestBase
{
    [Test]
    public void AddTwoNumbersCorrectly()
    {
        // Arrange
        var aNumber = 50.ToBinaryBools(ComputerSettings.WordSize);
        var bNumber = 150.ToBinaryBools(ComputerSettings.WordSize);

        var sut = ComponentFactory.CreateWordAdder(
            CreateTestWireGroup(false),
            CreateTestWireGroup(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWireGroup(false));
        
        // Act
        for (var i = 0; i < aNumber.Length; i++)
        {
            sut.InputsA[i].Value = aNumber[i];
        }
        
        for (var i = 0; i < bNumber.Length; i++)
        {
            sut.InputsB[i].Value = bNumber[i];
        }
        
        sut.Update();
        
        // Assert
        sut.Sum.ToInt().Should().Be(200);
    }
    
    [Test]
    public void InputLargerThanOneWordOutputCarried()
    {
        // Arrange
        var sut = ComponentFactory.CreateWordAdder(
            CreateTestWireGroup(false),
            CreateTestWireGroup(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWireGroup(false));
        
        // Act
        sut.CarryIn.Value = true;
        
        foreach (var inputA in sut.InputsA)
        {
            inputA.Value = true;
        }

        foreach (var inputB in sut.InputsB)
        {
            inputB.Value = true;
        }
        
        sut.Update();
        
        // Assert
        sut.CarryOut.Value.Should().BeTrue();
        
        using (new AssertionScope())
        {
            foreach (var sumWire in sut.Sum)
            {
                sumWire.Value.Should().BeTrue();
            }
        }
    }
}