using ComputerSimulator.Core.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests.Circuits;

public class DecoderTests : IntegrationTestBase
{
    [Test]
    [TestCase(false, false, true, false, false, false)]
    [TestCase(false, true, false, true, false, false)]
    [TestCase(true, false, false, false, true, false)]
    [TestCase(true, true, false, false, false, true)]
    public void TruthTable2X4(bool i1, bool i0, bool o0, bool o1, bool o2, bool o3)
    {
        const int decoderInputSize = 2;
        var sut = ComponentFactory.CreateDecoder(CreateTestWireGroup(false, decoderInputSize));

        sut.Inputs.SetValue(0, i0);
        sut.Inputs.SetValue(1, i1);

        sut.Outputs.GetValue(0).Should().Be(o0);
        sut.Outputs.GetValue(1).Should().Be(o1);
        sut.Outputs.GetValue(2).Should().Be(o2);
        sut.Outputs.GetValue(3).Should().Be(o3);
    }
    
    [Test]
    public void Decoder8X256Tests()
    {
        for (var expected = 0; expected < 256; expected++)
        {
            // Arrange
            const int decoderInputSize = 8;

            var input = expected.ToBinaryBools(decoderInputSize);
            
            var sut = ComponentFactory.CreateDecoder(CreateTestWireGroup(false, decoderInputSize));

            // Act
            for (var i = 0; i < input.Length; i++)
            {
                sut.Inputs.SetValue(i, input[i]);
            }
            
            // Assert
            sut.EnabledIndex.Should().Be(expected);
        }
    }
}