The Display Adapter in Practice

The [display adapter](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs) gives the CPU a way to change pixels, but the CPU still never points at a pixel directly. It writes bytes. The adapter receives those bytes through the [I/O bus](../../ComputerSimulator.Core/Parts/IoBus.cs), keeps a small amount of state about what the CPU is trying to do, and stores the final pixel byte in [display RAM](../../ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs). That is the whole path from an instruction to a mark on the screen. There is no hidden drawing command that says "turn on this pixel." There is only the old habit of putting a value on the bus and pulsing the right control wires.

The first thing the program must do is select the display. In this simulator the display adapter listens for I/O address `0x07`. When the CPU sends `OUT ADDR, Rn` and the register contains `0x07`, the display adapter recognizes its own address and becomes the active adapter for later display writes. This address is not the top-left pixel, and it is not a place in RAM. It is only the name of the device on the I/O bus. After that selection has happened, another `OUT ADDR` has a different meaning. It no longer selects the display itself. It selects a byte address inside the display adapter's own display RAM.

This is why a display write is usually a three-step conversation. First, send the device address `0x07` as an I/O address. Second, send the display RAM byte address as another I/O address. Third, send the pixel byte as I/O data. The second step loads the display RAM address register used for writes. The third step stores the byte into the selected slot. If the selected display RAM address is `0`, the byte belongs to the first group of eight pixels on the top row. If the selected address is `1`, it belongs to the next group of eight pixels on that same row. The adapter does not need a special instruction for either case. The same `OUT ADDR` and `OUT DATA` instructions do all the work.

One display RAM byte represents eight horizontal pixels. Bit 0 is the leftmost pixel in that group, bit 1 is the next pixel, and so on through bit 7. So the byte `00000101` lights the first and third pixels in its group, because bits 0 and 2 are set. On the top-left byte of the screen, that means pixel `(0, 0)` is on, pixel `(1, 0)` is off, pixel `(2, 0)` is on, and the rest of that eight-pixel group is off. The byte is just a number to the CPU, but the adapter and renderer agree to read that number as eight on-or-off dots.

The display RAM address is linear, so a program counts across a row before it reaches the next row. The number of display bytes in one row is `width / 8`. The default display is 96 pixels wide, so it has 12 display RAM bytes per row. Address `0` is row 0, byte column 0. Address `1` is row 0, byte column 1. Address `11` is still row 0, byte column 11. Address `12` is row 1, byte column 0. In general, the row is the address divided by `bytesPerRow`, and the byte column is the remainder after that division.

| Display RAM address | Screen row | Byte column | Pixel x range |
| --- | --- | --- | --- |
| `0` | `0` | `0` | `0` through `7` |
| `1` | `0` | `1` | `8` through `15` |
| `11` | `0` | `11` | `88` through `95` |
| `12` | `1` | `0` | `0` through `7` |

Here is a concrete walk through the first two rows on the default display. Suppose the program wants the first eight pixels of the top row to be on, off, on, off, on, off, on, off. It selects the display by sending `0x07` with `OUT ADDR`. Then it sends display RAM address `0` with `OUT ADDR`. Then it sends the pixel byte `01010101` with `OUT DATA`. Because bit 0 is leftmost, the left edge of the screen is on, the next pixel is off, and the pattern alternates across the first byte.

Now suppose the program wants to place a different pattern directly below that first byte, on the next screen row. It does not use address `1`, because address `1` is still on the top row. It uses address `12`, because there are 12 bytes in a 96-pixel row. The program sends `12` with `OUT ADDR`, then sends the new pixel byte with `OUT DATA`. If that byte is `11111111`, the first eight pixels of the second row are all lit. The screen now contains two unrelated bytes at two different display RAM addresses. Their relationship as one-above-the-other is created by the address mapping, not by a two-dimensional drawing instruction.

Writing a byte also replaces the whole group of eight pixels. If address `0` currently contains `01010101` and the program later writes `00000001` to address `0`, the old alternating pattern is gone. Only the leftmost pixel in that group remains on. This can feel wasteful if a program only wanted to change one pixel, but it keeps the adapter small. A program that wants to alter a single pixel must know the byte that already belongs to that eight-pixel group, change one bit in that byte, and write the whole byte back.

This is the same tradeoff that appears in many small machines. Memory is arranged in the units that are easy for the hardware to move, not always in the units that are easiest for a human to imagine. A person thinks about one pixel. The display adapter thinks about one byte. Once the program accepts that rule, drawing becomes a matter of choosing the right byte address and the right bit pattern.

That mapping is important when a program draws larger shapes. A horizontal line can often be written by stepping from address `0` to address `1` to address `2`, because adjacent display RAM bytes are adjacent groups of pixels on the same row. A vertical line is different. To move down one row while staying in the same byte column, the program adds `bytesPerRow` to the display RAM address. On the default display, that means adding 12. On a wider display, the number changes. This is why display programs often take `BYTES_PER_ROW` or `BYTES_PER_FRAME` as assembly-time values instead of baking one screen size into every calculation.

The small [display-pattern](../../programs/display-pattern.asm) program demonstrates this byte view of the screen. It selects the display once, starts with display RAM address `0`, writes a byte, increments the address, and repeats until it has covered the frame. Since it writes each address value as the pixel byte, the picture is not a hand-drawn image. It is the display memory layout made visible. The first bytes affect the first row. Later bytes spill into later rows. The program is simple, but it proves that the adapter, the display RAM address latch, the pixel byte write, and the row mapping are all cooperating.

Inside the simulator, the display adapter is built the same way as the rest of the machine. The [I/O bus control logic](../../ComputerSimulator.Core/Peripherals/Display/IoBusControl.cs) watches for the display address and for the difference between address output and data output. Display RAM has a write-side address register and stores the incoming byte when the write signal is active. The screen side later reads those stored bytes to decide which pixels are bright. These parts are separated so the CPU can write at one moment and the display can be rendered at another moment, just as a real display has memory that survives between refreshes.

The result is a useful compromise for a small simulated computer. The CPU does not need to know about terminal windows, frame buffers, or drawing libraries. It only needs to know that the display has an I/O address, that the display remembers a byte address, and that each stored byte becomes eight pixels. Once that rule is in place, the same simple instructions can draw a checkerboard, a line, a glyph, or anything else that can be reduced to rows of bits.

Further reading in the simulator

| Topic | Where to look |
| --- | --- |
| Display adapter write and render paths | [`ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs`](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs) |
| Display RAM storage and dirty byte tracking | [`ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs`](../../ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs) |
| Display I/O address selection and write signals | [`ComputerSimulator.Core/Peripherals/Display/IoBusControl.cs`](../../ComputerSimulator.Core/Peripherals/Display/IoBusControl.cs) |
| Pixel scanning and byte-to-pixel reading | [`ComputerSimulator.Core/Peripherals/Display/ScreenControl.cs`](../../ComputerSimulator.Core/Peripherals/Display/ScreenControl.cs) |
| Display address constants | [`ComputerSimulator.Core/Peripherals/IoAddress.cs`](../../ComputerSimulator.Core/Peripherals/IoAddress.cs) |
| Assembly program that fills display RAM | [`programs/display-pattern.asm`](../../programs/display-pattern.asm) |
