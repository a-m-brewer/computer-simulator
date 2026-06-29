Peripherals and Adapters

We already have the computer. It is still the CPU and RAM. The CPU reads instructions, moves bytes around, does arithmetic and logic, and decides where the next instruction will come from. The RAM remembers the instructions and the data. That is enough to make a computer, but it is not enough to make a computer that a person can use very easily. If the computer only changes bytes in RAM, then the person sitting in front of it cannot see what changed. If the person cannot put new bytes into the machine, then the computer can only run the program it already has.

So the computer needs other parts around it. These parts are not the computer itself. They are things the computer talks to. A display is one of these things. A keyboard is another one. A disk would be another one. A printer, a sound device, a timer, or a network connection would also be in this group. They can all be very different objects, but to this computer they all have one important thing in common. They move bytes into or out of the computer.

These outside parts are called peripherals. The word is a useful one because it reminds us that the CPU and RAM are still the center of the machine. The peripherals are around the edge of it. They make the computer useful to a person, but they do not change what the CPU is. The CPU does not know how to draw a letter on glass. It does not know how a key switch works. It does not know how to spin a disk. The CPU only knows how to put bytes on a bus, take bytes from a bus, and turn a few control wires on and off.

That means every peripheral needs a little translator between itself and the computer. This translator is called an adapter. The adapter has two sides. On one side it understands the computer's [I/O bus](../../ComputerSimulator.Core/Parts/IoBus.cs). On the other side it understands the device. The [display adapter](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs) understands [display RAM](../../ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs) and pixels. The [keyboard adapter](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs) understands key codes waiting to be read. A disk adapter would understand blocks of stored bytes. The CPU does not have to know the private details of each device. It only has to know the common way to talk to adapters.

The common way is simple. First the computer says which device it wants to talk to. Then it says whether the byte on the bus is an address or data. Then it says whether the byte is going out of the CPU or coming into the CPU. Then a clock wire makes the transfer happen. Many different devices can be attached to the same I/O bus because each adapter ignores bus activity that is not meant for it.

Each adapter has an I/O address. This is not a RAM address. It is just the number that a device recognizes as its own name on the I/O bus. In this simulator the display uses address `0x07`, and the keyboard uses address `0x0F`. When the CPU puts `0x07` on the bus as an I/O address, the display adapter recognizes it. When the CPU puts `0x0F` on the bus as an I/O address, the keyboard adapter recognizes it. Every other adapter should see that the address is not its own, and do nothing.

The instruction used for all of this is the I/O instruction. It is still only one instruction code, but the bits inside it give four useful actions.

| Language | Meaning |
| --- | --- |
| `OUT ADDR, Rn` | Send the value in register `Rn` as an address |
| `OUT DATA, Rn` | Send the value in register `Rn` as data |
| `IN ADDR, Rn` | Read an address value into register `Rn` |
| `IN DATA, Rn` | Read a data value into register `Rn` |

Most of the time, our programs use three of these. They use `OUT ADDR` to select a device or to select an address inside a device. They use `OUT DATA` to send bytes to an output device. They use `IN DATA` to read bytes from an input device. This is enough to make the display and the keyboard work, even though the display and keyboard are very different from each other.

For example, suppose a program wants to write one byte into the display adapter's RAM. The program first puts the display's I/O address, `0x07`, into a register. Then it performs `OUT ADDR` with that register. This does not draw anything. It only tells the I/O bus that the display adapter is the device being selected. Next the program puts the display RAM address into a register, and performs `OUT ADDR` again. This time the display adapter is already active, so it treats the address as a location inside display RAM. Finally the program puts the pixel byte into a register and performs `OUT DATA`. The display adapter stores that byte in its own RAM. Later, when the screen is rendered, the eight bits in that byte become eight pixels on one row of the display.

The keyboard works in the opposite direction. Suppose a program wants to know whether the user has pressed a key. The program puts the keyboard's I/O address, `0x0F`, into a register, and performs `OUT ADDR`. The keyboard adapter recognizes that address. Then the program performs `IN DATA` into a register. If the user has pressed the `A` key, the adapter puts the ASCII code for `A` on the bus, and the CPU stores that value in the register named by the instruction. If no key has been pressed, the adapter puts `0` on the bus. This is why keyboard programs usually repeat the same little test over and over. They read a byte, compare it with zero, and if it is zero they go back and try again.

Now we can put those two examples together. Imagine that a person presses `A`, and the program is an echo program whose job is to put typed characters on the screen. The first part of the program selects the keyboard with `OUT ADDR`, reads a key code with `IN DATA`, and compares the result with zero. If the result is zero, the program jumps back and polls the keyboard again. If the result is not zero, the byte is an ASCII code, and the program keeps it. For `A`, that code is `65`.

The second part of the program turns that ASCII code into pixels. The display cannot show the number `65` directly as a letter. It needs the little eight-byte picture of `A` from the font. So the program uses the ASCII value to find the correct glyph in the font data. Since each glyph is eight bytes high, the picture for a character begins at that character's ASCII number multiplied by eight. The program copies those eight font bytes, one row at a time, to eight different places in display RAM.

To copy the first row of the `A`, the program selects the display adapter with `OUT ADDR`, sends the correct display RAM byte address with another `OUT ADDR`, and sends the first font byte with `OUT DATA`. Then it moves to the next row of the glyph. The next display RAM address is not usually the next byte, because the next row of pixels is one whole screen row lower. On a 96 pixel wide display, each screen row is 12 bytes wide, so the next row of the same character is 12 display bytes later. After this has happened eight times, the display RAM contains the eight rows of the letter's picture. The next time the display is rendered, the person sees an `A`.

This sounds like a lot of work for one key. It is a lot of work for one key. The CPU had to read from one peripheral, test the result, find a picture in memory, write to another peripheral, and repeat part of that process eight times. But every piece of the work was small. The CPU moved bytes, compared bytes, added numbers, and jumped. The adapters turned those byte movements into something that looked like typing on a screen.

There is one more detail that matters in this simulator. There is no event system that secretly wakes things up. Each part of the simulated machine has an [`Update()`](../../ComputerSimulator.Core/IComponent.cs) method. The computer calls these methods in order. During each CPU update, the connected I/O components are updated from the I/O bus. This means the adapters are not special exceptions outside the machine. They are simulated parts connected to simulated wires, and they respond when the bus wires are in the right state.

This is also why the adapter code looks like wiring. The display adapter has logic that recognizes the display address and turns an `OUT ADDR` into a display RAM address latch. It turns an `OUT DATA` into a display RAM write. The keyboard adapter has logic that recognizes the keyboard address and turns an `IN DATA` into a register enable. When that read begins, it asks the keyboard input queue for one key code. The queue exists because the simulated keyboard is partly outside the simulated hardware: the host terminal receives the real key press, and the adapter makes it available to the CPU as a byte.

So a peripheral is not magic. It is not another CPU. It is a device with a small amount of logic that knows how to answer the few I/O signals from the computer. The CPU keeps doing the same small things it has always done. It moves bytes. The adapters make those bytes mean pixels, key presses, or whatever other outside-world device we decide to attach next.

Further reading in the simulator

| Topic | Where to look |
| --- | --- |
| The shared I/O bus wires | [`ComputerSimulator.Core/Parts/IoBus.cs`](../../ComputerSimulator.Core/Parts/IoBus.cs) |
| Display and keyboard I/O addresses | [`ComputerSimulator.Core/Peripherals/IoAddress.cs`](../../ComputerSimulator.Core/Peripherals/IoAddress.cs) |
| Where adapters are attached to the machine | [`ComputerSimulator.Core/Computer.cs`](../../ComputerSimulator.Core/Computer.cs) |
| The display adapter | [`ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs`](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs) |
| Display I/O bus control logic | [`ComputerSimulator.Core/Peripherals/Display/IoBusControl.cs`](../../ComputerSimulator.Core/Peripherals/Display/IoBusControl.cs) |
| The keyboard adapter | [`ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs`](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs) |
| The echo program written in assembly | [`programs/echo.asm`](../../programs/echo.asm) |
