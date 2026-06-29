# Scott CPU Assembly

Assembler flow:

```bash
dotnet run --project ComputerSimulator.Assembler.Cli -- programs/display-pattern.asm -o display-pattern.bin -D BYTES_PER_FRAME=576
dotnet run --project ComputerSimulator -- run display-pattern.bin
```

The assembler emits raw `.bin` images. The simulator only loads those bytes; it does not parse assembly.

Default built-in binaries are checked in under `programs/bin/` for the default `96x48` display. For custom dimensions,
assemble the source with explicit `-D` values and run the generated binary.

```bash
dotnet run --project ComputerSimulator.Assembler.Cli -- programs/display-pattern.asm -o display-pattern.bin -D BYTES_PER_FRAME=576
dotnet run --project ComputerSimulator.Assembler.Cli -- programs/hello-world.asm -o hello-world.bin -D BYTES_PER_ROW=12
dotnet run --project ComputerSimulator.Assembler.Cli -- programs/echo.asm -o echo.bin -D SCREEN_WIDTH=96 -D BYTES_PER_ROW=12
```

## Syntax

- Comments start with `;`.
- Labels are case-sensitive: `Loop:` and `loop:` are different.
- Mnemonics and registers are case-insensitive.
- Numbers may be decimal, hex (`0x2A`), binary (`0b101010`), or character literals (`'A'`).

## Operand Order

Assembly uses conventional destination-first syntax for mutating operations:

```asm
ADD R3, R0 ; R3 = R3 + R0
MOV R1, R0 ; R1 = R0
```

The book's shorthand writes ALU operands as `RA,RB`, where `RB` receives the result. The assembler maps the
destination-first source back to the existing hardware encoding internally.

`CMP` is different because it has no destination register, so it preserves written order:

```asm
CMP R3, R2 ; compare A=R3 with B=R2
```

## Instructions

```asm
DATA R0, 42

LD R1, [R0]
ST [R0], R1

ADD R3, R0
SHL R3
SHL R3, R0
SHR R3
NOT R3
AND R3, R0
OR R3, R0
XOR R3, R0
CMP R3, R2
CLF

JMP Loop
JMPR R0
JC CarryLabel
JA AboveLabel
JE EqualLabel
JZ ZeroLabel
JCAEZ AnyFlagLabel

IN DATA, R1
OUT ADDR, R1
OUT ADDRESS, R1
```

`JMP` and conditional jumps are real short jumps and require a target address that fits in one byte.

## Pseudo-Instructions

```asm
LDI R0, 0x7F
LDI R0, 0x2000, R3
MOV R1, R0
JMP16 FarLabel, R0, R3
HALT
HALT R0, R3
```

`LDI` is optimized. Values up to `0xFF` emit one `DATA` instruction. Larger values require an explicit scratch
register and expand to `DATA low`, `DATA high`, eight shifts, and `ADD`.

`JMP16` is a software long jump: it loads the 16-bit label address into the explicit address register, using the
explicit scratch register, then emits `JMPR`.

## Directives

```asm
.equ DISPLAY, 0x07
.org 0x2000
.byte 0x20, 'A'
.word 0x1234
.ascii "HELLO"
.asciz "HELLO"
.include "stdlib/io.asm"
.incbin "assets/font8x8.bin"
```

`.org` zero-fills gaps in the raw output image. `.incbin` makes custom fonts or other binary blobs possible without
changing simulator code.

## Dogfood Programs

Main programs live in `programs/`:

- `display-pattern.asm` writes each display RAM byte's address as its pixel byte. Requires `BYTES_PER_FRAME`.
- `hello-world.asm` draws `HELLO WORLD` using the RAM-loadable font at `0x2000`. Requires `BYTES_PER_ROW`.
- `echo.asm` polls the keyboard and draws typed characters. Requires `SCREEN_WIDTH` and `BYTES_PER_ROW`.

The shared font asset is `programs/assets/font8x8.bin`. Text programs place it with `.org FONT_BASE` and `.incbin`.
`programs/stdlib/io.asm` defines common I/O addresses and font constants.

The built-in simulator demos load checked-in default binaries from `programs/bin/`. Tests verify those binaries match
fresh assembly output, so update the `.bin` files whenever the corresponding `.asm` changes.

## Standard Library

Reusable assembly routines live in `programs/stdlib/`:

- `common.asm`: shared scratch RAM constants `STDLIB_TMP0` through `STDLIB_TMP5`.
- `io.asm`: I/O device addresses plus `FONT_BASE` and glyph constants.
- `math.asm`: `stdlib_mul`, `stdlib_div`.
- `memory.asm`: `stdlib_memcpy`.
- `display.asm`: `stdlib_print_char`, `stdlib_print_string`.
- `keyboard.asm`: `stdlib_read_line`.

Stdlib routines are written for the current no-stack CPU. Include routines near the start of a program behind an entry
jump, and reserve zero-page scratch bytes before including them:

```asm
JMP16 Start, R0, R1
.org 0x20
.include "stdlib/display.asm"

Start:
LDI R0, Message, R2
LDI R2, 0
LDI R3, Done, R1
JMP stdlib_print_string

Done:
HALT R0, R1
```

General calling convention:

- `R3` holds the return address for public routines.
- Routines return with `JMPR R3` unless documented otherwise.
- Routines are not re-entrant and may use `STDLIB_TMP0`-`STDLIB_TMP5`.
- Include files use global labels. Include each routine file once per program.

Routine contracts:

- `stdlib_mul`: input `R0` multiplicand, `R1` multiplier; output `R0` product low 16 bits; clobbers `R1`, `R2`.
- `stdlib_div`: input `R0` dividend, `R1` divisor; output `R0` quotient, `R1` remainder; divisor zero returns quotient `0` and remainder `dividend`; clobbers `R2`.
- `stdlib_memcpy`: input `R0` destination, `R1` source, `STDLIB_MEMCPY_LENGTH` byte count; clobbers `R0`-`R2`.
- `stdlib_print_char`: input `R1` ASCII character, `R2` display byte address; output `R2` next display byte address; requires `BYTES_PER_ROW` and font data at `FONT_BASE`.
- `stdlib_print_string`: input `R0` zero-terminated string address, `R2` display byte address; output `R2` next display byte address; requires `BYTES_PER_ROW` and font data at `FONT_BASE`.
- `stdlib_read_line`: input `R0` destination buffer, `R1` max characters before terminator; output `R1` character count and a zero-terminated buffer. It reads until Enter (`13`) or capacity.

## Testing Expectations

Dogfood integration tests should use the full path: `.asm` source, `ScottAssembler`, emitted bytes,
`ProgramLoader.Load`, simulated CPU/peripherals, then assertions against display, keyboard, or RAM behavior. Do not
reintroduce C# `InstructionSet` program builders for runtime demos.
