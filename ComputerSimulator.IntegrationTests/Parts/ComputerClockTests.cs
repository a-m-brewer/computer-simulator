using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Parts;

public class ComputerClockTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false, 0)]
    [TestCase(true, true, false, 1)]
    [TestCase(true, true, true, 2)]
    [TestCase(false, true, false, 3)]
    [TestCase(false, false, false, 4)]
    public void CanCompleteCycles(bool expectedClk, bool expectedClkE, bool expectedClkS, int cycles)
    {
        // Arrange
        var sut = ComponentFactory.CreateComputerClock(CreateTestWire(false), CreateTestWire(false), CreateTestWire(false));
        
        // Act
        for (var i = 0; i < cycles; i++)
        {
            sut.Update();
        }
        
        // Assert
        sut.Clk.Value.Should().Be(expectedClk);
        sut.ClkE.Value.Should().Be(expectedClkE);
        sut.ClkS.Value.Should().Be(expectedClkS);
    }
}