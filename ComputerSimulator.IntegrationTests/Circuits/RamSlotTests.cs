using ComputerSimulator.Core.Circuits;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class RamSlotTests : IntegrationTestBase
{
    [Test]
    public void RamSlot_CanTakeBusValue()
    {
        // Arrange

        var sut = GetRequiredService<IRamSlot>();
        sut.X = CreateTestWire("ram-slot-x", false);
        sut.Y = CreateTestWire("ram-slot-y", false);
        sut.Enable = CreateTestWire("ram-slot-enable", false);
        sut.Set = CreateTestWire("ram-slot-set", false);
        sut.Io = CreateTestBus("ram-io-bus", false);
        
        // Act
        sut.X.Value = true;
        sut.Y.Value = true;
        sut.Set.Value = true;

        foreach (var ioWire in sut.Io)
        {
            ioWire.Value = true;
        }
        
        // Assert
        var andOutputGroup = GetFirstGroupMatchingRegex<bool>(@"Register-.+-word-to-enabler");

        using (new AssertionScope())
        {
            foreach (var wire in andOutputGroup)
            {
                wire.Value.Should().Be(true);
            }
        }
    }
}