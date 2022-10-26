using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class WordTests : IntegrationTestBase
{
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Word_OutputOnlyUpdatesIfSetIsTrue(bool set)
    {
        // Arrange
        var inputs = CreateTestWireGroup<bool>();
        var outputs = CreateTestWireGroup<bool>();
        var setWire = CreateTestWire<bool>();

        var sut = ComponentFactory.CreateWord(inputs, outputs, setWire);

        // Act
        sut.Set.Value = set;

        for (var i = 0; i < sut.Inputs.Count; i++)
        {
            sut.Inputs[i].Value = true;
        }
        
        sut.Update();
        
        // Assert
        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Outputs.Count; i++)
            {
                sut.Outputs[i].Value.Should().Be(set);
            }
        }
    }
}