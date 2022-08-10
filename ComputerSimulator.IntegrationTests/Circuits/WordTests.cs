using ComputerSimulator.Core.Circuits;
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
        var inputs = CreateTestWireGroup("word-inputs", false);
        var outputs = CreateTestWireGroup("word-outputs", false);
        var setWire = CreateTestWire("word-set", false);
        
        var sut = GetRequiredService<IWord>();
        sut.Inputs = inputs;
        sut.Outputs = outputs;
        sut.Set = setWire;

        // Act
        sut.Set.Value = set;

        foreach (var input in sut.Inputs)
        {
            input.Value = true;
        }
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var output in sut.Outputs)
            {
                output.Value.Should().Be(set);
            }
        }
    }
}