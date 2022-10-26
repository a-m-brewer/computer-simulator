using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class WordComparatorTests : IntegrationTestBase
{
    [Test]
    [TestCase(0xFFFF, 0xFFFF, false, true)]
    [TestCase(0xFFFE, 0xFFFF, false, false)]
    [TestCase(0xFFFF, 0xFFFE, true, false)]
    public void CanCompareWords(int a, int b, bool aLarger, bool equal)
    {
        // Arrange
        var inputABools = a.ToBinaryBools(ComputerSettings.WordSize);
        var inputBBools = b.ToBinaryBools(ComputerSettings.WordSize);
        
        var sut = ComponentFactory.CreateWordComparator(
            CreateTestWireGroup<bool>(),
            CreateTestWireGroup<bool>(),
            CreateTestWire<bool>(),
            CreateTestWire<bool>(),
            CreateTestWireGroup<bool>(),
            CreateTestWire<bool>(),
            CreateTestWire<bool>());
        
        // Act
        sut.AllBitsAboveEqual.Value = true;
        sut.AAboveLarger.Value = false;

        for (var i = 0; i < ComputerSettings.WordSize; i++)
        {
            sut.InputsA[i].Value = inputABools[i];
            sut.InputsB[i].Value = inputBBools[i];
        }
        
        sut.Update();
        
        // Assert
        sut.Equal.Value.Should().Be(equal);

        using (new AssertionScope())
        {
            for (var i = 0; i < ComputerSettings.WordSize; i++)
            {
                sut.UnEqual[i].Value.Should().Be(inputABools[i] != inputBBools[i]);
            }
        }

        sut.ALarger.Value.Should().Be(aLarger);
    }
}