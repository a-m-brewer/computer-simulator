The Keyboard Adapter in Practice

The display gave the computer a way to speak to a person, but it did not give the person a way to answer. A program could fill display RAM with pixels, and the renderer could turn those pixels into a visible picture, but the machine was still closed from the user's side. If the program wanted a new value, that value had to already be in RAM, already be in the program image, or be produced by arithmetic. To make an interactive computer, the machine needs one more path. A real key press must become a byte that the CPU can read.

That sounds simple until we remember what kind of machine this simulator is. The CPU does not receive events. There is no hidden callback that interrupts the running program when a key is pressed. The CPU only fetches instructions, moves values across wires, and responds to the clocked signals that already exist. So the keyboard has to fit into the same I/O shape as the display. The host terminal may notice the physical key, but the simulated computer only sees an adapter attached to the [I/O bus](../../ComputerSimulator.Core/Parts/IoBus.cs).

The first job happens outside the CPU. When the terminal user presses a printable key, the host side maps it to a byte value. For ordinary printable characters this is the ASCII value, so `B` becomes `66`. Enter is mapped to `13`, and Backspace or Delete is mapped to `8`. Values that are not part of this small keyboard language are ignored instead of being forced into the simulated machine. The accepted bytes are placed into a queue, because the host can receive keys at moments when the CPU is busy doing something else.

The queue is important because simulated time and human time do not line up neatly. A person may press four keys before the slow, gate-level machine has finished one pass through its polling loop. If the input were only a single byte slot, later key presses could overwrite earlier ones. Instead, each mapped key code waits in order. The [keyboard adapter](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs) is the bridge between that host-side queue and the CPU's ordinary `IN DATA` instruction.

The CPU still begins by selecting the device. The keyboard's I/O address is `0x0F`, so a polling program first puts `0x0F` into a register and performs `OUT ADDR`. That address is not a RAM location. It is the name the keyboard adapter recognizes on the I/O bus. When the adapter sees that address while the address-selection control wires are active, it remembers that it is the selected device. Other adapters should ignore the later read because the selected address was not theirs.

After selection, the program performs `IN DATA` into one of the CPU registers. This is the moment when the direction of the bus reverses. With the display, the CPU was putting a byte onto the bus and the adapter was storing it. With the keyboard, the adapter puts a byte onto the bus and the CPU stores it in a register. If a key code is waiting, the adapter supplies the oldest queued byte. If the queue is empty, it supplies `0`.

Here is the concrete path for pressing `B`. First, the host terminal maps the key press to ASCII `66` and pushes that byte into the keyboard input queue. Later, the program selects the keyboard by sending `0x0F` with `OUT ADDR`. Then it performs `IN DATA, R1`. The adapter sees that the keyboard is selected, sees that this is a data read, removes `66` from the queue, and presents that value on the CPU bus long enough for the CPU register to capture it. The program now has `66` in `R1`, and can compare it, store it, or use it to choose a font glyph.

If the program immediately reads again and no other key has arrived, the result is different. It selects or remains pointed at the keyboard, performs another `IN DATA`, and the adapter asks the queue for the next byte. The queue is empty, so the adapter presents `0`. This is not the ASCII character `0`; it is the byte value zero, used as a simple answer to the question, "is there a key ready?" Keyboard programs usually compare the read value with zero and jump back to read again when the answer is no.

One subtle part of the adapter is easy to miss if we only think in instruction names. A single `IN DATA` instruction is not one instant inside the simulator. The control wires that enable the read can stay active across many adapter updates while the CPU is working through that instruction's clock steps. If the adapter removed a new key from the queue on every one of those internal updates, fast typing would lose characters. The CPU would only capture one value, but the adapter might have consumed several queued keys before the read window closed.

To avoid that, the adapter treats a read window as one opportunity to dequeue. At the start of a selected data read, it takes one key code if one is available, or chooses zero if not. It then holds that value for the whole window. Only after the read-enable signal drops does the adapter reset its temporary input and become ready for another read. This is why quickly typing `A` and `B` should produce `A` on the first read and keep `B` waiting for the next separate read, not silently discard `B` during the same instruction.

The behavior can be summarized without looking at implementation details.

| Situation | Byte returned by `IN DATA` |
| --- | --- |
| Keyboard selected and queue contains `B` | `66` |
| Keyboard selected and queue is empty | `0` |
| Keyboard not selected | no keyboard byte is driven for the CPU to read |
| Several keys queued before separate reads | one byte per read, in the original order |

This design is deliberately plain. The keyboard does not interrupt the CPU. It does not know what the program plans to do with a character. It does not draw anything, edit a line, or move a cursor. It only turns host key presses into queued bytes, and then turns selected `IN DATA` reads into one byte at a time on the bus. That small contract is enough for useful programs, because the CPU can already compare, jump, and write to the display.

The result is a computer that listens by repeatedly asking. That may seem inefficient, but it is a good match for the machine we have built. Polling keeps the hardware simple, keeps all device communication on the same I/O bus, and makes the program responsible for deciding what a key means. In the next step, the program will use this adapter and the display adapter together: read a byte from the keyboard, turn it into an eight-row glyph, write those rows into display RAM, and advance a cursor so typing begins to look like typing.

Further reading in the simulator

| Topic | Where to look |
| --- | --- |
| Keyboard input queue interface and buffer | [`ComputerSimulator.Core/Peripherals/Keyboard/IKeyboardInput.cs`](../../ComputerSimulator.Core/Peripherals/Keyboard/IKeyboardInput.cs) |
| Keyboard adapter bus logic | [`ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs`](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs) |
| Host terminal key-to-byte mapping | [`ComputerSimulator/Tui/TerminalKeyboardInput.cs`](../../ComputerSimulator/Tui/TerminalKeyboardInput.cs) |
| Keyboard I/O address shared by programs | [`programs/stdlib/io.asm`](../../programs/stdlib/io.asm) |
| Keyboard adapter integration tests | [`ComputerSimulator.IntegrationTests/Peripherals/Keyboard/KeyboardAdapterTests.cs`](../../ComputerSimulator.IntegrationTests/Peripherals/Keyboard/KeyboardAdapterTests.cs) |
| Buffered input behavior tests | [`ComputerSimulator.IntegrationTests/Peripherals/Keyboard/BufferedKeyboardInputTests.cs`](../../ComputerSimulator.IntegrationTests/Peripherals/Keyboard/BufferedKeyboardInputTests.cs) |
