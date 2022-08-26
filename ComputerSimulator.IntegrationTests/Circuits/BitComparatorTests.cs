using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class BitComparatorTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false, false, true)]
    [TestCase(false, true, true, false, false)]
    [TestCase(true, false, true, true, false)]
    [TestCase(true, true, false, false, true)]
    public void CanCompareTwoBits(bool a, bool b, bool unEqual, bool aLarger, bool equal)
    {
        // Arrange
        var sut = ComponentFactory.CreateBitComparator(
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false));
        
        // Act
        sut.InputA.Value = a;
        sut.InputB.Value = b;
        sut.AllBitsAboveEqual.Value = true;
        sut.AAboveLarger.Value = false;
        
        sut.Update();

        // Assert
        sut.Equal.Value.Should().Be(equal);
        sut.ALarger.Value.Should().Be(aLarger);
        sut.UnEqual.Value.Should().Be(unEqual);
    }

    [Test]
    public void WillReportALargerIfItIsPassedIn()
    {
        // Arrange
        var sut = ComponentFactory.CreateBitComparator(
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false),
            CreateTestWire(false));
        
        // Act
        sut.InputA.Value = false;
        sut.InputB.Value = false;
        sut.AllBitsAboveEqual.Value = false;
        sut.AAboveLarger.Value = true;
        
        sut.Update();
        
        // Assert
        sut.UnEqual.Value.Should().BeFalse();
        sut.ALarger.Value.Should().BeTrue();
        sut.Equal.Value.Should().BeFalse();
    }
}