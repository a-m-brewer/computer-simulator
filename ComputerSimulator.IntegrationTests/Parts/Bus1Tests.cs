using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

public class Bus1Tests : IntegrationTestBase
{
    [Test]
    public void BitNotSetPassesBitsThrough()
    {
        // Arrange
        var sut = ComponentFactory.CreateBus1(CreateTestWire(false), CreateTestWireGroup(false),
            CreateTestWireGroup(false));
        
        // Act
        sut.Bit.Value = false;

        foreach (var input in sut.Inputs)
        {
            input.Value = true;
        }
        
        sut.Update();
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var sutOutput in sut.Outputs)
            {
                sutOutput.Value.Should().BeTrue();
            }
        }
    }
    
    [Test]
    public void BitSetOutputShouldBe1()
    {
        // Arrange
        var sut = ComponentFactory.CreateBus1(CreateTestWire(false), CreateTestWireGroup(false),
            CreateTestWireGroup(false));
        
        // Act
        sut.Bit.Value = true;

        foreach (var input in sut.Inputs)
        {
            input.Value = true;
        }
        
        sut.Update();
        
        // Assert
        sut.Outputs
            .ToInt()
            .Should()
            .Be(1);
    }
}