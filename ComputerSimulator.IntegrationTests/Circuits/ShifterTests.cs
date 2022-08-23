using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class ShifterTests : IntegrationTestBase
{
    public class RightShifterTests : ShifterTests
    {
        [Test]
        public void PerformsShift()
        {
            // Arrange
            const int input = 0x42;
            var inputBools = input.ToBinaryBools(ComputerSettings.WordSize);

            var sut = ComponentFactory.CreateRightShifter(
                CreateTestWire(false),
                CreateTestWire(false),
                CreateTestWireGroup(false),
                CreateTestWireGroup(false));
            
            // Act
            for (var i = 0; i < sut.Input.Count; i++)
            {
                sut.Input[i].Value = inputBools[i];
            }
            
            sut.Update();
            
            // Assert
            const int expected = 0x21;

            sut.Output
                .ToInt()
                .Should()
                .Be(expected);
        }
    }
}