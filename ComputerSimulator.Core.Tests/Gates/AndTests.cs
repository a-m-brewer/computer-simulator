using System;
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
        var wires = new List<IWire2<bool>>
        {
            Mock.Of<IWire2<bool>>(m => m.Value == a),
            Mock.Of<IWire2<bool>>(m => m.Value == b)
        };
        
        var wireGroup = GetMock<IWireGroup<bool>>();
        
        wireGroup.SetupListMock(wires);

        var output = Mock.Of<IWire2<bool>>();

        var sut = CreateSut();

        sut.Inputs = wireGroup.Object;
        sut.Output = output;
        
        // Act
        wireGroup.Raise(r => r.WireValuesChanged += null, null, 1);
        
        // Assert
        output.Value
            .Should()
            .Be(expected);
    }
}