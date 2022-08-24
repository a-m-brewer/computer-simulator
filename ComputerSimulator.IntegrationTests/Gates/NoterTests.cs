using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Gates;

public class NoterTests : IntegrationTestBase
{
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void GateTest(bool input)
    {
        // Arrange
        var sut = ComponentFactory.CreateNoter(CreateTestWireGroup(false), CreateTestWireGroup(false));
        
        // Act
        foreach (var wire in sut.Inputs)
        {
            wire.Value = input;
        }
        
        sut.Update();
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var outputWire in sut.Outputs)
            {
                outputWire.Value
                    .Should()
                    .Be(!input);
            }
        }
    }
}