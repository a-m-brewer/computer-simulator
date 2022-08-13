using ComputerSimulator.Core.Circuits;
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
        var sut = GetRequiredService<IDecoder>();
        sut.Initialize(decoderInputSize);
        sut.Inputs = CreateTestWireGroup("decoder-input", false, decoderInputSize);
        sut.Outputs = CreateTestWireGroup("decoder-output", false, sut.OutputSize);

        sut.Inputs[0].Value = i0;
        sut.Inputs[1].Value = i1;

        sut.Outputs[0].Value.Should().Be(o0);
        sut.Outputs[1].Value.Should().Be(o1);
        sut.Outputs[2].Value.Should().Be(o2);
        sut.Outputs[3].Value.Should().Be(o3);
    }
    
    [Test]
    public void Decoder8X256Tests()
    {
        for (var expected = 0; expected < 256; expected++)
        {
            // Arrange
            const int decoderInputSize = 8;

            var input = expected.ToBinaryBools(decoderInputSize);
            
            var sut = GetRequiredService<IDecoder>();
            sut.Initialize(decoderInputSize);
            sut.Inputs = CreateTestWireGroup("decoder-input", false, decoderInputSize);
            sut.Outputs = CreateTestWireGroup("decoder-output", false, sut.OutputSize);

            // Act
            for (var i = 0; i < input.Length; i++)
            {
                sut.Inputs[i].Value = input[i];
            }
            
            // Assert
            sut.EnabledIndex.Should().Be(expected);
        }
    }
}