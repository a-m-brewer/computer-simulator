using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Peripherals.Display.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.Core.Tests.Peripherals.Display.Text;

public class AsciiFont8x8Tests
{
    [Test]
    public void FontByteAddressMapsToAsciiGlyphRow()
    {
        AsciiFont8x8.GetByte(('A' * AsciiFont8x8.GlyphHeight) + 3)
            .Should()
            .Be(AsciiFont8x8.GetGlyphRow('A', 3));
    }

    [Test]
    public void PrintableAsciiGlyphsAreAddressableAsEightRowsEach()
    {
        for (var ascii = AsciiFont8x8.FirstPrintable; ascii <= AsciiFont8x8.LastPrintable; ascii++)
        {
            for (var row = 0; row < AsciiFont8x8.GlyphHeight; row++)
            {
                AsciiFont8x8.GetByte((ascii * AsciiFont8x8.GlyphHeight) + row)
                    .Should()
                    .BeInRange(0, 0xFF);
            }
        }
    }

    [Test]
    public void SpaceGlyphIsBlank()
    {
        AsciiFont8x8.GetGlyphRows(' ')
            .Should()
            .OnlyContain(row => row == 0);
    }

    [Test]
    public void LowercaseCharactersUseUppercaseGlyphs()
    {
        AsciiFont8x8.GetGlyphRows('h')
            .Should()
            .Equal(AsciiFont8x8.GetGlyphRows('H'));
    }

    [Test]
    public void RejectsInvalidRows()
    {
        FluentActions.Invoking(() => AsciiFont8x8.GetGlyphRow('A', AsciiFont8x8.GlyphHeight))
            .Should()
            .Throw<ComputerSimulatorException>();
    }
}
