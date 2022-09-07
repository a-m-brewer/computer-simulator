using System.Collections.Generic;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.TestUtilities;
using FluentAssertions;
using NUnit.Framework;
using Moq;

namespace ComputerSimulator.Core.Tests.Gates;

public class AndTests : MockBase<And>
{
    [Test]
    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, true)]
    public void InputsChanged_SetsOutput(bool a, bool b, bool expected)
    {
        // Arrange
        var wires = new List<IWire<bool>>
        {
            Mock.Of<IWire<bool>>(m => m.Value == a),
            Mock.Of<IWire<bool>>(m => m.Value == b)
        };
        
        var wireGroup = GetMock<IWireGroup<bool>>();
        
        wireGroup.SetupListMock(wires);

        var output = Mock.Of<IWire<bool>>();

        var and = new And(wireGroup.Object, output);

        // Act
        and.Update();
        
        // Assert
        output.Value
            .Should()
            .Be(expected);
    }
}