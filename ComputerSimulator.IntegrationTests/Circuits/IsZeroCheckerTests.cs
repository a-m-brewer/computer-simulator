using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class IsZeroCheckerTests : IntegrationTestBase
{
    [Test]
    [TestCase(0, true)]
    [TestCase(1, false)]
    public void CheckIsZero(int value, bool expected)
    {
        // Arrange
        var inputBools = value.ToBinaryBools(ComputerSettings.WordSize);
        
        var sut = ComponentFactory.CreateIsZeroChecker(CreateTestWireGroup<bool>(), CreateTestWire<bool>());
        
        // Act
        for (var i = 0; i < sut.Inputs.Count; i++)
        {
            sut.Inputs[i].Value = inputBools[i];
        }

        sut.Update();
        
        // Assert
        sut.IsZero.Value.Should().Be(expected);
    }
}