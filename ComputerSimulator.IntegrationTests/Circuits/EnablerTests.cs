using System.Collections.Generic;
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
        var inputs = CreateTestWireGroup(false);
        var outputs = CreateTestWireGroup(false);
        var enableWire = CreateTestWire(false);

        var sut = ComponentFactory.CreateEnabler(enableWire, inputs, outputs);

        // Act
        sut.Enable.Value = enable;

        for (var i = 0; i < sut.Inputs.Count; i++)
        {
            sut.Inputs.SetValue(i, true);
        }
        
        // Assert
        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Outputs.Count; i++)
            {
                sut.Outputs.GetValue(i).Should().Be(enable);
            }
        }
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Enabler_EnableNotSet_DoesNotRaiseEventsForInputs(bool enable)
    {
        // Arrange
        var inputs = CreateTestWireGroup(false);

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
        outputs.SetupWireGroupMock(outputWires);
        
        var enableWire = CreateTestWire( false);
        
        var sut = ComponentFactory.CreateEnabler(enableWire, inputs, outputs.Object);

        // Act
        sut.Enable.Value = enable;

        inputs.SetValue(0, true);
        
        // Assert
        outputWire0.VerifySet(wire2 => wire2.Value = It.IsAny<bool>(), enable ? Times.AtLeastOnce : Times.Never);
    }
}