using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class RamSlotTests : IntegrationTestBase
{
    [Test]
    [TestCase(0, false)]
    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(3, false)]
    [TestCase(4, false)]
    [TestCase(5, false)]
    [TestCase(6, false)]
    [TestCase(7, true)]
    public void RamSlot_CanTakeBusValue(int decimalTruthTable, bool expected)
    {
        // Arrange
        var truthTable = decimalTruthTable.ToBinaryBools(3);
        var x = truthTable[0];
        var y = truthTable[1];
        var set = truthTable[2];

        var sut = GetRequiredService<IRamSlot>();
        sut.X = CreateTestWire("ram-slot-x", false);
        sut.Y = CreateTestWire("ram-slot-y", false);
        sut.Enable = CreateTestWire("ram-slot-enable", false);
        sut.Set = CreateTestWire("ram-slot-set", false);
        sut.Io = CreateTestBus("ram-io-bus", false);

        // Act
        sut.X.Value = x;
        sut.Y.Value = y;
        sut.Set.Value = set;

        foreach (var ioWire in sut.Io)
        {
            ioWire.Value = true;
        }

        var andOutputGroup = GetFirstGroupMatchingRegex<bool>(@"Register-.+-word-to-enabler");
        using (new AssertionScope())
        {
            foreach (var wire in andOutputGroup)
            {
                wire.Value.Should().Be(expected);
            }
        }
    }

    [Test]
    public void RamSlot_CanLetValuesBackOntoTheBus()
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

        sut.Set.Value = false;

        foreach (var ioWire in sut.Io)
        {
            ioWire.Value = false;
        }

        var andOutputGroup = GetFirstGroupMatchingRegex<bool>(@"Register-.+-word-to-enabler");

        using (new AssertionScope())
        {
            foreach (var wire in andOutputGroup)
            {
                wire.Value.Should().Be(true);
            }
        }

        using (new AssertionScope())
        {
            foreach (var ioWire in sut.Io)
            {
                ioWire.Value.Should().BeFalse();
            }
        }

        sut.Enable.Value = true;

        using (new AssertionScope())
        {
            foreach (var ioWire in sut.Io)
            {
                ioWire.Value.Should().BeTrue();
            }
        }
    }
}