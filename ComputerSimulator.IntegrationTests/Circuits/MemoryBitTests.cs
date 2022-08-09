using ComputerSimulator.Core.Circuits;
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
        var sut = GetRequiredService<IMemoryBit>();
        sut.Input = CreateTestWire("memory-bit-input", false);
        sut.Output = CreateTestWire("memory-bit-output", false);
        sut.Set = CreateTestWire("memory-bit-set", false);
        
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
        var sut = GetRequiredService<IMemoryBit>();
        sut.Input = CreateTestWire("memory-bit-input", false);
        sut.Output = CreateTestWire("memory-bit-output", false);
        sut.Set = CreateTestWire("memory-bit-set", false);
        
        // Act / Assert
        sut.Set.Value = true;
        sut.Input.Value = true;

        sut.Output.Value.Should().BeTrue();
        
        sut.Set.Value = false;
        sut.Input.Value = false;

        sut.Output.Value.Should().BeTrue();
    }
}