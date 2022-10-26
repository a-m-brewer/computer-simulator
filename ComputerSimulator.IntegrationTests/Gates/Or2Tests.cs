using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Gates;

public class Or2Tests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public void GateTest(bool inputA, bool inputB, bool expectedOutput)
    {
        // Arrange
        var sut = ComponentFactory.CreateOr2(CreateTestWire<bool>(), CreateTestWire<bool>(), CreateTestWire<bool>());

        // Act
        sut.InputA.Value = inputA;
        sut.InputB.Value = inputB;
        
        sut.Update();

        // Assert
        sut.Output.Value.Should().Be(expectedOutput);
    }
}