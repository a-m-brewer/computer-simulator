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
        var inputs = CreateTestWireGroup(false);
        var outputs = CreateTestWireGroup(false);
        var setWire = CreateTestWire(false);

        var sut = ComponentFactory.CreateWord(inputs, outputs, setWire);

        // Act
        sut.Set.Value = set;

        for (var i = 0; i < sut.Inputs.Count; i++)
        {
            sut.Inputs.SetValue(i, true);
        }
        
        // Assert
        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Outputs.Count; i++)
            {
                sut.Outputs.GetValue(i).Should().Be(set);
            }
        }
    }
}