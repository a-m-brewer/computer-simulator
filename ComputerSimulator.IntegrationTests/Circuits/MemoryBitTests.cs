using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class MemoryBitTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, true)]
    public void MemoryGate(bool i, bool s, bool o)
    {
        // Arrange
        var sut = ComponentFactory.CreateMemoryBit(CreateTestWire(false), CreateTestWire(false), CreateTestWire(false));

        // Act
        sut.Input.Value = i;
        sut.Set.Value = s;

        // Assert
        sut.Output.Value.Should().Be(o);
    }
    
    [Test]
    public void ShouldKeepStateIfSetIfFalse()
    {
        // Arrange
        var sut = ComponentFactory.CreateMemoryBit(CreateTestWire(false), CreateTestWire(false), CreateTestWire(false));
        
        // Act / Assert
        sut.Set.Value = true;
        sut.Input.Value = true;

        sut.Output.Value.Should().BeTrue();
        
        sut.Set.Value = false;
        sut.Input.Value = false;

        sut.Output.Value.Should().BeTrue();
    }
}