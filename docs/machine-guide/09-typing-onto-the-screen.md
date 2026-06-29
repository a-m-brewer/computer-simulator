Typing Onto the Screen

The machine can now do two separate useful things. It can read a key code from the [keyboard adapter](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs), and it can write pixel bytes into [display RAM](../../ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs). Those are still only separate powers. Reading `65` from the keyboard does not by itself show an `A`, and drawing an `A` does not by itself know when the user wants one. The first genuinely interactive program is the one that joins the two halves: wait for a key, decide what kind of key it is, draw printable characters, and keep enough cursor state to know where the next character belongs.

This program is often called an echo program because it echoes typed input back to the screen. It is not a terminal emulator, a text editor, or an operating system. It is much smaller than that. Its job is to poll the keyboard over and over, ignore zero reads, draw ordinary printable characters with the bitmap font, move to a new line when Enter is read, and erase the previous character position when Backspace is read. That is enough to make the computer feel different. The user can now affect what appears on the display while the machine is running.

The program keeps a few tiny pieces of state in RAM. One value is the current display byte address where the next glyph should begin. Another is the current text column, which is used mostly to decide whether Backspace is allowed. A third is the base address of the current text line. These values are ordinary RAM bytes and words managed by the program. The [display adapter](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs) does not know about cursors or lines. It only knows about display RAM addresses and pixel bytes.

At startup, the program selects the display and the keyboard so both adapters have been addressed at least once, then it enters the polling loop. The loop selects the keyboard address `0x0F` and performs `IN DATA`. If the result is `0`, the program jumps back to the beginning of the loop. Nothing is drawn, and no cursor value changes. This is the quiet state of the program: the CPU is doing work, but the screen is unchanged because no key is ready.

When a printable key arrives, the path becomes longer. Suppose the user types `A`. The host has queued ASCII `65`, the keyboard adapter returns `65` during the selected `IN DATA` read, and the program compares the value with the special keys it knows about. It is not zero, not Enter, and not Backspace, so the program treats it as printable. Now the byte from the keyboard becomes an index into the font data.

Each font glyph is eight bytes high. To find the picture for `A`, the program multiplies the ASCII value by eight by shifting it left three times, then adds the font base address. The result is the RAM address of the first row of the `A` glyph. The program also loads the current cursor address, which is the display RAM byte where the top row of the glyph should be written. From there it repeats the same small action eight times: load one glyph row byte from RAM, send the display RAM address with `OUT ADDR`, send the glyph row byte with `OUT DATA`, move to the next glyph row, and move the display address down by one screen row.

On the default display, the screen is 96 pixels wide, so each row is 12 display bytes wide. If the top row of the `A` goes to display byte address `0`, the next row of the same `A` goes to address `12`, then `24`, and so on for eight rows. The bytes are not adjacent because adjacent display bytes are side by side on the same pixel row. The rows of a character are stacked vertically, so the program adds `BYTES_PER_ROW` between glyph rows.

After the eighth row has been written, the character is complete in display RAM. The program then advances the cursor address by one display byte and advances the column by one. That one-byte movement is one character cell to the right because a glyph is eight pixels wide and one display byte also holds eight horizontal pixels. The next printable character will reuse the same process, but its first display byte will be one byte farther along the row.

Enter changes the cursor without drawing a glyph. In this program, Enter is the byte value `13`. When the polling loop reads `13`, the program takes the saved base address for the current text line and adds `SCREEN_WIDTH` to it. This can look surprising until we translate it through the display layout. Moving down one text line means moving down eight pixel rows. Each pixel row is `SCREEN_WIDTH / 8` display bytes wide. Eight of those rows are therefore `SCREEN_WIDTH` display bytes. The program stores that new line base, sets the cursor address to the same value, resets the column to zero, and returns to polling.

Backspace is the byte value `8`, and it has one guard. If the current column is already zero, the program ignores Backspace because there is no previous character on this line to erase. Otherwise it subtracts one from the column and one from the cursor address. Then it draws a space character at that previous cursor position. The erase is not a special display operation. It is just another printable glyph write, using the font's picture for a blank space. In the current program, that erase reuses the same drawing path as any other printable character, so the visible result is the important part to notice: the previous character cell is overwritten with blank pixels.

The polling loop therefore gives a small set of meanings to bytes from the keyboard.

| Read byte | Program meaning | Action |
| --- | --- | --- |
| `0` | no key ready | jump back to polling |
| `13` | Enter | move to the next text line |
| `8` | Backspace or Delete | blank the previous cell when possible |
| printable ASCII | ordinary character | draw the matching font glyph |

Here is a concrete run with a blank screen and the cursor at the top-left cell. The user types `A`, so the program reads `65`, finds the `A` glyph, writes eight display RAM bytes starting at byte `0` and stepping by `BYTES_PER_ROW`, then advances the cursor address to `1` and the column to `1`. The user presses Enter, so the program reads `13`, moves the line base down by one text row, sets the cursor address to that new base, and resets the column to `0`. The user types `B`, so the program reads `66`, finds the `B` glyph, writes it at the first cell of the second text line, and advances the cursor to the next cell on that line.

The important thing is that no single part is doing anything mysterious. The keyboard adapter only returns bytes. The program gives meaning to zero, Enter, Backspace, and printable ASCII. The font data only stores pictures as eight-byte glyphs. The display adapter only stores pixel bytes at selected addresses. The appearance of text input comes from the program arranging these small pieces in the right order.

This echo program is still modest. It does not scroll the screen, it does not wrap long lines in a careful way, and it does not have a stack for nested subroutine calls. The [reusable assembly routines](../../programs/stdlib/) in the repository follow a no-stack convention, with explicit return registers and scratch RAM, because that is the machine available right now. Even with those limits, the program crosses an important line. The computer is no longer only running a prepared picture. It is reacting to the person at the keyboard and changing the screen as a result.

Further reading in the simulator

| Topic | Where to look |
| --- | --- |
| Echo program source | [`programs/echo.asm`](../../programs/echo.asm) |
| Shared I/O constants and font base | [`programs/stdlib/io.asm`](../../programs/stdlib/io.asm) |
| Keyboard polling routine in the stdlib | [`programs/stdlib/keyboard.asm`](../../programs/stdlib/keyboard.asm) |
| Character and string drawing routines | [`programs/stdlib/display.asm`](../../programs/stdlib/display.asm) |
| Font image loaded by text programs | [`programs/assets/font8x8.bin`](../../programs/assets/font8x8.bin) |
| Echo program integration tests | [`ComputerSimulator.IntegrationTests/Peripherals/Keyboard/EchoProgramTests.cs`](../../ComputerSimulator.IntegrationTests/Peripherals/Keyboard/EchoProgramTests.cs) |
