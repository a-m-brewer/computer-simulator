Pictures of Letters

The display can show pixels now, but a person usually wants more than loose pixels. A program that says hello needs letters. The display adapter does not know what a letter is. It only knows display RAM bytes, and each byte still means eight horizontal pixels. So a letter has to become a small picture made from bytes before the display can show it. That small picture is called a glyph.

This simulator uses an 8 by 8 bitmap font. Each character is eight pixels wide and eight pixels tall. Since one display RAM byte is already eight horizontal pixels, one row of a glyph fits exactly into one byte. A full character therefore needs eight bytes: one byte for the top row, one byte for the next row, and so on down to the eighth row. The byte for a glyph row follows the same display rule as any other pixel byte. Bit 0 is the leftmost pixel of the row, and bit 7 is the rightmost pixel.

ASCII gives the program a number for each character. `H` is 72, `A` is 65, and a space is 32. The font data is arranged so that every ASCII slot owns eight bytes. That makes the address calculation regular. To find the first font byte for a character, multiply the ASCII value by 8 and add the font base address. To find a particular row, add the row number from 0 through 7. In the assembly programs, the font image is loaded into RAM at `FONT_BASE`, so the row for a character is found at `FONT_BASE + ascii * 8 + row`.

Walk through drawing an `H`. The ASCII value is 72. The first byte of the `H` glyph is at `FONT_BASE + 72 * 8`. The second row is one byte after that, and the eighth row is seven bytes after that. The program reads the first glyph byte from ordinary RAM, selects the display adapter if it has not already done so, sends the destination display RAM byte address with `OUT ADDR`, and sends the glyph byte with `OUT DATA`. One row of the letter is now stored in display RAM.

| Glyph row | Font byte address | Display byte address when drawing at top-left |
| --- | --- | --- |
| `0` | `FONT_BASE + 72 * 8 + 0` | `0` |
| `1` | `FONT_BASE + 72 * 8 + 1` | `bytesPerRow` |
| `2` | `FONT_BASE + 72 * 8 + 2` | `bytesPerRow * 2` |
| `7` | `FONT_BASE + 72 * 8 + 7` | `bytesPerRow * 7` |

The next row of the same `H` does not go to the next display RAM byte unless the display is only one character wide. It goes one screen row lower, in the same character column. Since each display row contains `bytesPerRow` display bytes, the destination address for the second glyph row is the first destination address plus `bytesPerRow`. The third glyph row is another `bytesPerRow` after that. On the default 96-pixel-wide display, `bytesPerRow` is 12. So a character drawn at display byte address `0` uses display RAM addresses `0`, `12`, `24`, `36`, `48`, `60`, `72`, and `84` for its eight rows.

That spacing is the most important difference between font memory and display memory. The font stores the eight rows of a glyph next to each other, because the font is just a compact table of character pictures. The display stores rows of the whole screen next to each other, because it has to represent a two-dimensional image. Drawing text is the act of copying from one layout to the other. The program reads consecutive font bytes, but it writes display bytes separated by the width of a screen row.

A space is handled the same way as a visible letter. Its ASCII value chooses a glyph, and that glyph happens to contain empty rows. The drawing routine still copies eight bytes to display RAM. This is useful because it keeps the text path regular. The program does not need a special instruction for blank space, and the display adapter does not need to understand word spacing. A space is only another eight-row picture.

Newlines and wrapping are also address choices rather than display features. To move to the next text row, a program chooses a display byte address that is eight pixel rows lower and usually returns to byte column zero. If the screen is 96 pixels wide, one text row is eight display rows, and each display row is 12 bytes, so the next text row starts 96 display bytes later. The display adapter does not know that this is a line break. It only sees the next eight glyph bytes arrive at their chosen display RAM addresses.

Adjacent letters use the same rule with a different starting address. If the first character starts at display byte address `0`, the next character in the same text row starts at display byte address `1`, because one 8-pixel-wide character is one display byte wide. Drawing `HI` at the top-left means drawing `H` into addresses `0`, `12`, `24`, and so on, then drawing `I` into addresses `1`, `13`, `25`, and so on. The two glyphs are interleaved by display row when they land on the screen, even though each glyph is stored contiguously in the font table.

The `HELLO WORLD` program is built from this repeated copy. It stores the message as bytes in RAM, ending with a zero byte. The print-string routine reads one character byte at a time. If the byte is zero, the string is finished. Otherwise, the print-character routine turns the ASCII value into a font address, copies eight font rows to display RAM, and returns the next display byte address so the following character can appear one cell to the right. The CPU still only loads, stores, adds, shifts, compares, jumps, and performs I/O. The appearance of text comes from arranging those small actions in the right order.

The font image itself is also just bytes. The simulator has [C# font data](../../ComputerSimulator.Core/Peripherals/Display/Text/AsciiFont8x8.cs) so tests and helper rendering can ask for glyph rows directly, but assembly programs do not call that C# code. They include a RAM-loadable binary font image. The assembler places [`programs/assets/font8x8.bin`](../../programs/assets/font8x8.bin) at `FONT_BASE` with `.incbin`, and the program reads it like any other data in RAM. That keeps the boundary clear. The simulator runs bytes. The assembler prepares bytes. The display shows bytes as pixels.

There are honest limits in this simple text system. The font is monochrome, so a pixel is either on or off. A character cell is fixed at eight by eight pixels. Lowercase letters are normalized by the [built-in font helper](../../ComputerSimulator.Core/Peripherals/Display/Text/AsciiFont8x8.cs), and printable characters without hand-drawn glyphs use deterministic fallback-style generated shapes. The assembly routines also follow the no-stack convention used by the small [standard library](../../programs/stdlib/), so they pass return addresses and scratch values explicitly instead of pushing calls onto a stack. These limits keep the machine understandable while still making real text possible.

Once letters are pictures, the keyboard and display can meet. A key press can become an ASCII byte. That byte can select eight glyph bytes. Those glyph bytes can be copied into display RAM. The person at the terminal sees a letter, but the machine only moved numbers through registers, RAM, and an adapter.

Further reading in the simulator

| Topic | Where to look |
| --- | --- |
| Built-in 8 by 8 ASCII font data | [`ComputerSimulator.Core/Peripherals/Display/Text/AsciiFont8x8.cs`](../../ComputerSimulator.Core/Peripherals/Display/Text/AsciiFont8x8.cs) |
| Helper that draws glyphs through the display adapter | [`ComputerSimulator.Core/Peripherals/Display/Text/GlyphRenderer.cs`](../../ComputerSimulator.Core/Peripherals/Display/Text/GlyphRenderer.cs) |
| RAM-loadable font image used by assembly programs | [`programs/assets/font8x8.bin`](../../programs/assets/font8x8.bin) |
| Display and text assembly routines | [`programs/stdlib/display.asm`](../../programs/stdlib/display.asm) |
| I/O and font address constants | [`programs/stdlib/io.asm`](../../programs/stdlib/io.asm) |
| Hello-world assembly program | [`programs/hello-world.asm`](../../programs/hello-world.asm) |
| Text program integration tests | [`ComputerSimulator.IntegrationTests/Peripherals/Display/TextProgramTests.cs`](../../ComputerSimulator.IntegrationTests/Peripherals/Display/TextProgramTests.cs) |
