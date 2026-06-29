# Scott CPU Assembly

Assembler flow:

```bash
dotnet run --project ComputerSimulator.Assembler.Cli -- programs/display-pattern.asm -o display-pattern.bin -D BYTES_PER_FRAME=576
dotnet run --project ComputerSimulator -- run display-pattern.bin
```

The assembler emits raw `.bin` images. The simulator only loads those bytes; it does not parse assembly.

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
