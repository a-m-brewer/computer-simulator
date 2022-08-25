using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Gates;

public class OrerTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public void GateTest(bool a, bool b, bool expected)
    {
        // Arrange
        var sut = ComponentFactory.CreateOrer(
            CreateTestWireGroup(false),
            CreateTestWireGroup(false),
            CreateTestWireGroup(false));
        
        // Act
        foreach (var aInput in sut.InputsA)
        {
            aInput.Value = a;
        }
        
        foreach (var bInput in sut.InputsB)
        {
            bInput.Value = b;
        }
        
        sut.Update();
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var output in sut.Outputs)
            {
                output.Value.Should().Be(expected);
            }
        }
    }
}