using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class NAndTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    public void NAnd_TruthTableTest(bool a, bool b, bool expected)
    {
        // Arrange
        var inputs = CreateTestWireGroup(false, 2);
        var output = CreateTestWire(false);

        var sut = ComponentFactory.CreateNAnd(inputs, output);

        // Act
        inputs[0].Value = a;
        inputs[1].Value = b;
        
        sut.Update();
        
        // Assert
        sut.Output.Value.Should().Be(expected);
    }
}