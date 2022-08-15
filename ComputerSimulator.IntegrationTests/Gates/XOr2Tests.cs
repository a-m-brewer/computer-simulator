using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Gates;

public class XOr2Tests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    public void GateTest(bool inputA, bool inputB, bool expectedOutput)
    {
        // Arrange
        var sut = ComponentFactory.CreateXOr2(CreateTestWire(false), CreateTestWire(false), CreateTestWire(false));

        // Act
        sut.InputA.Value = inputA;
        sut.InputB.Value = inputB;
        
        sut.Update();

        // Assert
        sut.Output.Value.Should().Be(expectedOutput);
    }
}