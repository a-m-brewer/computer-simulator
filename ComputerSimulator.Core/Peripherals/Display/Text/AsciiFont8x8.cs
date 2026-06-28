using ComputerSimulator.Core.Exceptions;

namespace ComputerSimulator.Core.Peripherals.Display.Text;

public static class AsciiFont8x8
{
    public const int GlyphWidth = 8;
    public const int GlyphHeight = 8;
    public const int AsciiCharacterCount = 128;
    public const int RomByteCount = AsciiCharacterCount * GlyphHeight;
    public const int FirstPrintable = 32;
    public const int LastPrintable = 126;

    private static readonly int[] Space = Glyph(
        "        ",
        "        ",
        "        ",
        "        ",
        "        ",
        "        ",
        "        ",
        "        ");

    private static readonly int[] Fallback = Glyph(
        " ####   ",
        "#    #  ",
        "     #  ",
        "   ##   ",
        "  #     ",
        "        ",
        "  #     ",
        "        ");

    private static readonly IReadOnlyDictionary<char, int[]> GeneratedGlyphs = Enumerable
        .Range(FirstPrintable, LastPrintable - FirstPrintable + 1)
        .ToDictionary(ascii => (char)ascii, ascii => GeneratedPrintableGlyph(ascii));

    private static readonly IReadOnlyDictionary<char, int[]> Glyphs = new Dictionary<char, int[]>
    {
        [' '] = Space,
        ['!'] = Glyph("  #     ", "  #     ", "  #     ", "  #     ", "  #     ", "        ", "  #     ", "        "),
        ['.'] = Glyph("        ", "        ", "        ", "        ", "        ", "        ", "  #     ", "        "),
        [','] = Glyph("        ", "        ", "        ", "        ", "        ", "  #     ", "  #     ", " #      "),
        [':'] = Glyph("        ", "        ", "  #     ", "        ", "        ", "  #     ", "        ", "        "),
        ['?'] = Fallback,
        ['0'] = Glyph(" ####   ", "#    #  ", "#   ##  ", "#  # #  ", "##   #  ", "#    #  ", " ####   ", "        "),
        ['1'] = Glyph("  #     ", " ##     ", "  #     ", "  #     ", "  #     ", "  #     ", " ###    ", "        "),
        ['2'] = Glyph(" ####   ", "#    #  ", "     #  ", "   ##   ", "  #     ", " #      ", "######  ", "        "),
        ['3'] = Glyph(" ####   ", "#    #  ", "     #  ", "  ###   ", "     #  ", "#    #  ", " ####   ", "        "),
        ['4'] = Glyph("   ##   ", "  # #   ", " #  #   ", "#   #   ", "######  ", "    #   ", "    #   ", "        "),
        ['5'] = Glyph("######  ", "#       ", "#       ", "#####   ", "     #  ", "#    #  ", " ####   ", "        "),
        ['6'] = Glyph(" ####   ", "#    #  ", "#       ", "#####   ", "#    #  ", "#    #  ", " ####   ", "        "),
        ['7'] = Glyph("######  ", "     #  ", "    #   ", "   #    ", "  #     ", "  #     ", "  #     ", "        "),
        ['8'] = Glyph(" ####   ", "#    #  ", "#    #  ", " ####   ", "#    #  ", "#    #  ", " ####   ", "        "),
        ['9'] = Glyph(" ####   ", "#    #  ", "#    #  ", " #####  ", "     #  ", "#    #  ", " ####   ", "        "),
        ['A'] = Glyph(" ####   ", "#    #  ", "#    #  ", "######  ", "#    #  ", "#    #  ", "#    #  ", "        "),
        ['B'] = Glyph("#####   ", "#    #  ", "#    #  ", "#####   ", "#    #  ", "#    #  ", "#####   ", "        "),
        ['C'] = Glyph(" ####   ", "#    #  ", "#       ", "#       ", "#       ", "#    #  ", " ####   ", "        "),
        ['D'] = Glyph("#####   ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", "#####   ", "        "),
        ['E'] = Glyph("######  ", "#       ", "#       ", "#####   ", "#       ", "#       ", "######  ", "        "),
        ['F'] = Glyph("######  ", "#       ", "#       ", "#####   ", "#       ", "#       ", "#       ", "        "),
        ['G'] = Glyph(" ####   ", "#    #  ", "#       ", "#  ###  ", "#    #  ", "#    #  ", " ####   ", "        "),
        ['H'] = Glyph("#    #  ", "#    #  ", "#    #  ", "######  ", "#    #  ", "#    #  ", "#    #  ", "        "),
        ['I'] = Glyph(" ###    ", "  #     ", "  #     ", "  #     ", "  #     ", "  #     ", " ###    ", "        "),
        ['J'] = Glyph("   ###  ", "    #   ", "    #   ", "    #   ", "    #   ", "#   #   ", " ###    ", "        "),
        ['K'] = Glyph("#    #  ", "#   #   ", "#  #    ", "###     ", "#  #    ", "#   #   ", "#    #  ", "        "),
        ['L'] = Glyph("#       ", "#       ", "#       ", "#       ", "#       ", "#       ", "######  ", "        "),
        ['M'] = Glyph("#    #  ", "##  ##  ", "# ## #  ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", "        "),
        ['N'] = Glyph("#    #  ", "##   #  ", "# #  #  ", "#  # #  ", "#   ##  ", "#    #  ", "#    #  ", "        "),
        ['O'] = Glyph(" ####   ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", " ####   ", "        "),
        ['P'] = Glyph("#####   ", "#    #  ", "#    #  ", "#####   ", "#       ", "#       ", "#       ", "        "),
        ['Q'] = Glyph(" ####   ", "#    #  ", "#    #  ", "#    #  ", "#  # #  ", "#   #   ", " ### #  ", "        "),
        ['R'] = Glyph("#####   ", "#    #  ", "#    #  ", "#####   ", "#  #    ", "#   #   ", "#    #  ", "        "),
        ['S'] = Glyph(" ####   ", "#    #  ", "#       ", " ####   ", "     #  ", "#    #  ", " ####   ", "        "),
        ['T'] = Glyph("#####   ", "  #     ", "  #     ", "  #     ", "  #     ", "  #     ", "  #     ", "        "),
        ['U'] = Glyph("#    #  ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", " ####   ", "        "),
        ['V'] = Glyph("#    #  ", "#    #  ", "#    #  ", "#    #  ", "#    #  ", " #  #   ", "  ##    ", "        "),
        ['W'] = Glyph("#    #  ", "#    #  ", "#    #  ", "# ## #  ", "# ## #  ", "##  ##  ", "#    #  ", "        "),
        ['X'] = Glyph("#    #  ", "#    #  ", " #  #   ", "  ##    ", " #  #   ", "#    #  ", "#    #  ", "        "),
        ['Y'] = Glyph("#   #   ", "#   #   ", " # #    ", "  #     ", "  #     ", "  #     ", "  #     ", "        "),
        ['Z'] = Glyph("######  ", "     #  ", "    #   ", "   #    ", "  #     ", " #      ", "######  ", "        ")
    };

    public static int GetByte(int address)
    {
        if (address < 0)
        {
            throw new ComputerSimulatorException("Font address cannot be negative");
        }

        var ascii = address / GlyphHeight;
        var row = address % GlyphHeight;

        return GetGlyphRow((char)ascii, row);
    }

    public static byte[] CreateRomImage()
    {
        var image = new byte[RomByteCount];
        for (var address = 0; address < image.Length; address++)
        {
            image[address] = (byte)GetByte(address);
        }

        return image;
    }

    public static int GetGlyphRow(char character, int row)
    {
        if (row is < 0 or >= GlyphHeight)
        {
            throw new ComputerSimulatorException($"Font row {row} is out of range");
        }

        return GetGlyphRows(character)[row];
    }

    public static IReadOnlyList<int> GetGlyphRows(char character)
    {
        if (character is < (char)FirstPrintable or > (char)LastPrintable)
        {
            return Fallback;
        }

        var normalized = char.IsLower(character) ? char.ToUpperInvariant(character) : character;
        if (Glyphs.TryGetValue(normalized, out var glyph))
        {
            return glyph;
        }

        return GeneratedGlyphs.TryGetValue(character, out var generated) ? generated : Fallback;
    }

    private static int[] GeneratedPrintableGlyph(int ascii)
    {
        // Keeps every printable ASCII slot deterministic and non-empty until a hand-tuned glyph is added.
        return
        [
            0b01111110,
            0b01000010 | ((ascii & 0b00000001) != 0 ? 0b00011000 : 0),
            0b01000010 | ((ascii & 0b00000010) != 0 ? 0b00011000 : 0),
            0b01000010 | ((ascii & 0b00000100) != 0 ? 0b00011000 : 0),
            0b01000010 | ((ascii & 0b00001000) != 0 ? 0b00011000 : 0),
            0b01000010 | ((ascii & 0b00010000) != 0 ? 0b00011000 : 0),
            0b01000010 | ((ascii & 0b00100000) != 0 ? 0b00011000 : 0),
            0b01111110
        ];
    }

    private static int[] Glyph(params string[] rows)
    {
        if (rows.Length != GlyphHeight)
        {
            throw new ComputerSimulatorException("A glyph must have exactly 8 rows");
        }

        var bytes = new int[GlyphHeight];
        for (var y = 0; y < rows.Length; y++)
        {
            if (rows[y].Length != GlyphWidth)
            {
                throw new ComputerSimulatorException("A glyph row must have exactly 8 columns");
            }

            for (var x = 0; x < rows[y].Length; x++)
            {
                if (rows[y][x] != ' ')
                {
                    bytes[y] |= 1 << x;
                }
            }
        }

        return bytes;
    }
}
