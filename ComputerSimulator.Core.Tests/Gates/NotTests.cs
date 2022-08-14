using System;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.TestUtilities;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ComputerSimulator.Core.Tests.Gates;

public class NotTests : MockBase<Not>
{
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void InputChanged_OutputIsReverseOfInput(bool input)
    {
        // Arrange
        var inputWire = Mock.Of<IWire2<bool>>(m => m.Value == input);
        
        var sut = new Not(inputWire, Mock.Of<IWire2<bool>>());

        // Act
        Mock.Get(inputWire)
            .Raise(m => m.ValueChanged += null, EventArgs.Empty);
        
        // Assert
        sut.Output
            .Value
            .Should()
            .Be(!input);
    }
}