using ComputerSimulator.Core.Circuits;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class EnablerTests : IntegrationTestBase
{
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Enabler_OutputOnlyUpdatesIfEnableIsTrue(bool enable)
    {
        // Arrange
        var inputs = CreateTestWireGroup("enabler-inputs", false);
        var outputs = CreateTestWireGroup("enabler-outputs", false);
        var enableWire = CreateTestWire("enabler-wire", false);
        
        var sut = GetRequiredService<IEnabler>();
        sut.Inputs = inputs;
        sut.Outputs = outputs;
        sut.Enable = enableWire;

        // Act
        sut.Enable.Value = enable;

        foreach (var input in sut.Inputs)
        {
            input.Value = true;
        }
        
        // Assert
        using (new AssertionScope())
        {
            foreach (var output in sut.Outputs)
            {
                output.Value.Should().Be(enable);
            }
        }
    }
}