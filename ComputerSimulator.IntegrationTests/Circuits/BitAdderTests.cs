using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class BitAdderTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, false, false, false)]
    [TestCase(false, false, true, false, true)]
    [TestCase(false, true, false, false, true)]
    [TestCase(false, true, true, true, false)]
    [TestCase(true, false, false, false, true)]
    [TestCase(true, false, true, true, false)]
    [TestCase(true, true, false, true, false)]
    [TestCase(true, true, true, true, true)]
    public void CanAddNumbers(bool carryIn, bool a, bool b, bool carryOut, bool sum)
    {
        // Arrange
        var sut = ComponentFactory.CreateBitAdder(
            CreateTestWire<bool>(),
            CreateTestWire<bool>(),
            CreateTestWire<bool>(),
            CreateTestWire<bool>(),
            CreateTestWire<bool>());
        
        // Act
        sut.InputA.Value = a;
        sut.InputB.Value = b;
        sut.CarryIn.Value = carryIn;

        sut.Update();

        sut.Sum.Value.Should().Be(sum);
        sut.CarryOut.Value.Should().Be(carryOut);
    }
}