Useful Routines Without New Hardware

By this point the computer can do useful work, but a useful program can still be tiring to write. Drawing one character means finding the right eight bytes in the font, selecting the display adapter, sending eight display RAM addresses, and writing eight pixel rows. Reading a key means selecting the keyboard adapter, asking for data, checking whether the answer was zero, and trying again if nothing was waiting. Copying a few bytes from one place in RAM to another means building a small loop by hand. None of these jobs requires a new circuit, but each one asks the programmer to remember the same careful sequence again and again.

A standard library is a way to give those repeated sequences a name. It is not part of the CPU. It is not hidden inside the simulator. It is ordinary assembly source that can be included in another program, assembled into bytes, loaded into RAM, and executed by the same machine as any other program. The word library can make it sound more mysterious than it is. In this repository it means a set of small routines in [`programs/stdlib/`](../../programs/stdlib/) that agree on how arguments are passed, where temporary values may be stored, and how control returns to the caller.

The reason this needs an agreement is that the computer has very little built-in help for procedure calls. There is no stack. There is no instruction that automatically pushes a return address before a jump. There are only registers, RAM, jumps, and the discipline of the program. That does not prevent routines from being useful, but it makes the calling convention visible. The caller has to put input values in the expected registers, put a return address in `R3`, and jump to the routine. The routine has to leave its result where it promised, avoid depending on private state that the caller cannot know about, and return with `JMPR R3` unless its own documentation says otherwise.

This is the main convention used by the assembly standard library.

| Convention | Meaning |
| --- | --- |
| `R3` | Holds the return address for public routines |
| `R0`, `R1`, `R2` | Carry routine-specific inputs and outputs |
| `STDLIB_TMP0` through `STDLIB_TMP5` | Shared scratch RAM used by routines |
| Include files | Bring routine labels and definitions into the program being assembled |
| Documented clobbers | Tell the caller which registers, flags, or scratch bytes may be changed |

The scratch bytes matter because there are only four general-purpose registers. A routine often needs to remember something while it uses the registers for another job. For example, a string-printing routine needs to remember the address of the string while it uses registers to draw the current character. Since there is no stack frame where local variables can be created, the library uses a few fixed RAM locations for temporary storage. This is simple and honest, but it also means the routines are not re-entrant. One call cannot safely interrupt another call that is using the same scratch bytes, and a program should include each routine file once so the global labels stay unambiguous.

Imagine a program that wants to print a zero-terminated message on the display. The program includes the display routines near the beginning, behind an initial jump that skips over the routine bodies when execution starts. Later, when the program reaches its real entry point, it puts the address of the first byte of the message in `R0`. It puts the display RAM byte address where the first character should appear in `R2`. It puts the label it wants to resume at in `R3`. Then it jumps to `stdlib_print_string`.

At that moment nothing special happens in the hardware. The instruction address simply changes to the first byte of the routine. The routine reads the byte at the string address in `R0`. If the byte is zero, the string is finished, so the routine restores the saved return address and jumps back through `R3`. If the byte is not zero, the byte is an ASCII character. The routine arranges a temporary return address for the helper that draws one character, jumps to `stdlib_print_char`, and resumes after that helper has written the glyph.

The character helper does the same display work that earlier sections described by hand. It selects the display adapter with the display I/O address. It turns the ASCII code into a font address by multiplying the code by the glyph height, which is eight bytes. It reads the first glyph row from RAM and writes that byte to display RAM. Then it moves one screen row down by adding `BYTES_PER_ROW` to the display address and repeats the write for the next glyph row. After eight rows, it restores the original display address, advances it by one character cell, and returns to the string routine. The string routine advances the message pointer and repeats the loop for the next character.

This walkthrough is longer than the call itself, and that is the point. The caller does not need to re-describe every display transfer every time it wants to print a message. The routine still performs all the small actions. The display adapter is still selected with `OUT ADDR`. Pixel bytes still reach display RAM through `OUT DATA`. The font still lives in RAM as eight-byte glyphs. The standard library only packages the known sequence so a larger program can spend more of its attention on what it is trying to do.

The same pattern appears in the other library files. The keyboard routine repeatedly selects the keyboard adapter and reads data until it has collected a line or reached the caller's capacity. The memory copy routine walks a source address and a destination address forward one byte at a time. The multiplication routine adds the same number repeatedly, and the division routine subtracts the divisor until the remainder is smaller than the divisor. These are not fast algorithms by ordinary software standards, but they fit the machine we have. They use the instructions the CPU already understands, and they make the assembly language more pleasant without pretending the hardware has changed.

There is a useful lesson here about layers. Pseudo-instructions made the source language easier without adding new CPU instructions. Dogfood programs moved demonstrations out of C# and into assembly without changing the simulator's loader. The standard library takes one more step in the same direction. It lets assembly programs share behavior with other assembly programs. The machine still fetches bytes from RAM and moves values over buses, but now a programmer can build a program out of named routines instead of rebuilding every little loop from scratch.

This does not remove the limits of the computer. Short jumps still have one-byte targets unless the assembler expands a software long jump. The CPU still has no stack. Public routines still rely on `R3` for their return address, and nested routine calls have to save that address explicitly when they need to use `R3` again. Those limits are worth keeping visible. They show that a standard library is not magic. It is a carefully written program that other programs agree to call in a careful way.

Further reading in the simulator

| Topic | Where to look |
| --- | --- |
| Shared stdlib scratch definitions | [`programs/stdlib/common.asm`](../../programs/stdlib/common.asm) |
| I/O and font constants used by routines | [`programs/stdlib/io.asm`](../../programs/stdlib/io.asm) |
| Character and string printing routines | [`programs/stdlib/display.asm`](../../programs/stdlib/display.asm) |
| Keyboard line input routine | [`programs/stdlib/keyboard.asm`](../../programs/stdlib/keyboard.asm) |
| Multiplication and division routines | [`programs/stdlib/math.asm`](../../programs/stdlib/math.asm) |
| Memory copy routine | [`programs/stdlib/memory.asm`](../../programs/stdlib/memory.asm) |
| Assembly syntax and routine contracts | [`docs/ASSEMBLY.md`](../ASSEMBLY.md) |
| Integration tests for stdlib behavior | [`ComputerSimulator.IntegrationTests/Assembler/StdlibProgramTests.cs`](../../ComputerSimulator.IntegrationTests/Assembler/StdlibProgramTests.cs) |
