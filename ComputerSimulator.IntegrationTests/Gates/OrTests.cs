using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Gates;

public class OrTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public void GateTest(bool inputA, bool inputB, bool expectedOutput)
    {
        // Arrange
        var sut = ComponentFactory.CreateOr(CreateTestWireGroup<bool>(2), CreateTestWire<bool>());

        // Act
        sut.Inputs[0].Value = inputA;
        sut.Inputs[1].Value = inputB;
        
        sut.Update();

        // Assert
        sut.Output.Value.Should().Be(expectedOutput);
    }
}