using System.Collections.Generic;
using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.TestUtilities;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
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

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Enabler_EnableNotSet_DoesNotRaiseEventsForInputs(bool enable)
    {
        // Arrange
        var inputs = CreateTestWireGroup("enabler-inputs", false);

        var outputWire0 = new Mock<IWire2<bool>>(); 
        
        var outputWires = new List<IWire2<bool>>
        {
            outputWire0.Object
        };
        
        for (var i = 0; i < ComputerSettings.WordSize - 1; i++)
        {
            outputWires.Add(Mock.Of<IWire2<bool>>());
        }

        var outputs = new Mock<IWireGroup<bool>>();
        outputs.SetupListMock(outputWires);
        
        var enableWire = CreateTestWire("enabler-wire", false);
        
        var sut = GetRequiredService<IEnabler>();
        sut.Inputs = inputs;
        sut.Outputs = outputs.Object;
        sut.Enable = enableWire;

        // Act
        sut.Enable.Value = enable;

        inputs[0].Value = true;
        
        // Assert
        outputWire0.VerifySet(wire2 => wire2.Value = It.IsAny<bool>(), enable ? Times.AtLeastOnce : Times.Never);
    }
}