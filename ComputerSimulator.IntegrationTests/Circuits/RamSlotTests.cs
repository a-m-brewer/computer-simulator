using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Parts;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
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

        var sut = ComponentFactory.CreateRamSlot(
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestBus());

        // Act
        sut.X.Value = x;
        sut.Y.Value = y;
        sut.Set.Value = set;

        for (var i = 0; i < sut.Io.Count; i++)
        {
            sut.Io.SetValue(i, true);
        }

        var andOutputGroup = new Mock<IWireGroup<bool>>().Object;
        using (new AssertionScope())
        {
            for (var i = 0; i < andOutputGroup.Count; i++)
            {
                andOutputGroup.GetValue(i).Should().Be(expected);
            }
        }
    }

    [Test]
    public void RamSlot_CanLetValuesBackOntoTheBus()
    {
        // Arrange

        var sut = ComponentFactory.CreateRamSlot(
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestBus());

        // Act
        sut.X.Value = true;
        sut.Y.Value = true;
        sut.Set.Value = true;

        for (var i = 0; i < sut.Io.Count; i++)
        {
            sut.Io.SetValue(i, true);
        }

        sut.Set.Value = false;

        for (var i = 0; i < sut.Io.Count; i++)
        {
            sut.Io.SetValue(i, false);
        }

        var andOutputGroup = new Mock<IWireGroup<bool>>().Object;

        using (new AssertionScope())
        {
            for (var i = 0; i < andOutputGroup.Count; i++)
            {
                andOutputGroup.GetValue(i).Should().Be(true);
            }
        }

        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Io.Count; i++)
            {
                sut.Io.GetValue(i).Should().BeFalse();
            }
        }

        sut.Enable.Value = true;

        using (new AssertionScope())
        {
            for (var i = 0; i < sut.Io.Count; i++)
            {
                sut.Io.GetValue(i).Should().BeTrue();
            }
        }
    }
}