# Machine Guide Writing Guide

This guide is for agents drafting `docs/machine-guide/*.md`. The sections should explain the completed simulator work in a beginner-friendly, bottom-up way while referencing the actual repository where useful.

Treat the existing sample, [`04-peripherals-and-adapters.md`](04-peripherals-and-adapters.md), as the style calibration point.

## Style Target

- Write original prose only.
- Use a plain title as the first line, with no Markdown `#` heading in section files.
- Use longer, flowing explanatory paragraphs rather than many isolated short sentences.
- Keep the tone patient, concrete, and beginner-friendly.
- Introduce one new idea at a time.
- Prefer ordinary explanations before naming the technical thing.
- Use instruction names and component names in prose, but avoid turning sections into API documentation.
- Link simulator class, file, and directory references with Markdown links when there is a clear target, especially on first mention and in the `Further reading in the simulator` table. From section files, paths to repo-root files usually start with `../../`, and paths to docs beside this directory usually start with `../`.
- Keep registers, instruction mnemonics, numeric values, formulas, and example filenames as inline code unless they are specifically being used as repository references. For example, keep `R0`, `OUT DATA`, `0x07`, and `bytesPerRow = width / 8` as code, but write [`ProgramLoader`](../../ComputerSimulator.Core/ProgramLoader.cs) when referring to the simulator class.
- Avoid code blocks in section prose unless absolutely necessary. Prefer small Markdown tables and abstract step-through explanations.
- Include concrete examples. Good examples say things like: first this instruction selects this adapter, then this component sees the address, then this byte is stored or read.
- Put heavier repository references in a `Further reading in the simulator` section at the end of each section.
- Keep the roadmap milestone labels hidden in the section prose. These are reader-facing guide sections, not project management notes.
- Keep limitations honest. For example, the CPU has no stack, color is deferred, short jumps are one-byte, and stdlib routines use a no-stack convention.

## Section Format

Each section should generally have:

1. A plain title line.
2. Several explanatory paragraphs that begin from a problem the current machine has.
3. One or more concrete examples or walkthroughs.
4. A short closing paragraph that connects the idea to the next machine capability.
5. A `Further reading in the simulator` section with a Markdown table of relevant files.

The sections should be substantial enough to teach the idea, but not exhaustive manuals. A typical section can be 40-90 lines of Markdown.

## Existing Sample

Use [`04-peripherals-and-adapters.md`](04-peripherals-and-adapters.md) as the style reference.

Important traits in that sample:

- It uses paragraphs rather than many standalone sentences.
- It explains CPU/RAM first, then why peripherals are needed.
- It includes a concrete sequence for pressing `A`, reading it from the keyboard, finding the font glyph, and writing display RAM.
- It references repository classes sparingly in the prose and lists more references at the end.

## Factual Anchors

General architecture:

- The project simulates a computer from gates upward: [gates](../../ComputerSimulator.Core/Gates/), [circuits](../../ComputerSimulator.Core/Circuits/), [parts](../../ComputerSimulator.Core/Parts/), [CPU](../../ComputerSimulator.Core/Parts/CentralProcessingUnit.cs)/[RAM](../../ComputerSimulator.Core/Parts/Ram.cs)/[bus](../../ComputerSimulator.Core/Parts/EventBus.cs), [peripherals](../../ComputerSimulator.Core/Peripherals/), [host](../../ComputerSimulator/), [assembler](../../ComputerSimulator.Assembler/).
- Components are updated explicitly with [`Update()`](../../ComputerSimulator.Core/IComponent.cs). There is no event system.
- The CPU has four general-purpose registers, `R0` through `R3`.
- Words are 16-bit, but `DATA` immediate values are 8-bit.
- The simulator is intentionally faithful and therefore slow compared with normal software.

I/O:

- The I/O bus is represented by [`ComputerSimulator.Core/Parts/IoBus.cs`](../../ComputerSimulator.Core/Parts/IoBus.cs).
- Peripherals attach through [`IoBus.ConnectedComponents`](../../ComputerSimulator.Core/Parts/IoBus.cs).
- Display I/O address is `0x07`; keyboard I/O address is `0x0F`.
- `OUT ADDR, Rn` sends an address. `OUT DATA, Rn` sends data. `IN DATA, Rn` reads data.
- Display writes select the [display](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs), latch a [display-RAM](../../ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs) byte address, then write one pixel byte.
- Keyboard reads select the [keyboard](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs), then `IN DATA` returns an ASCII keycode or `0` if no key is available.

Display:

- One display RAM byte represents eight horizontal pixels.
- Bit 0 is the leftmost pixel in the byte.
- `bytesPerRow = width / 8`.
- A linear byte address maps to row `address / bytesPerRow` and byte-column `address % bytesPerRow`.
- Gate-level rendering uses [`ScreenControl`](../../ComputerSimulator.Core/Peripherals/Display/ScreenControl.cs) to scan pixels in a faithful way.
- Scan-buffer rendering walks [display RAM](../../ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs) bytes directly and only re-presents when needed.
- Terminal rendering supports block and braille pixel modes in [`TerminalFrameRenderer`](../../ComputerSimulator/Tui/TerminalFrameRenderer.cs).

Text/font:

- The font is an 8x8 bitmap font.
- Each glyph is eight bytes.
- A glyph row is found at `fontBase + ascii * 8 + row`.
- Text programs place a RAM-loadable font image from [`programs/assets/font8x8.bin`](../../programs/assets/font8x8.bin).
- To draw one character, copy eight glyph bytes to display RAM rows separated by `bytesPerRow`.

Keyboard/echo:

- Host terminal input is queued through [`IKeyboardInput`](../../ComputerSimulator.Core/Peripherals/Keyboard/IKeyboardInput.cs) / [`BufferedKeyboardInput`](../../ComputerSimulator.Core/Peripherals/Keyboard/IKeyboardInput.cs).
- The [keyboard adapter](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs) consumes queued keycodes during selected `IN DATA` reads.
- The adapter returns `0` when no key is available.
- Echo programs poll the keyboard, draw non-zero characters, handle Enter as newline, and handle Backspace/Delete as erase previous character.

Loader and assembler:

- [`ProgramLoader`](../../ComputerSimulator.Core/ProgramLoader.cs) loads raw `.bin` images into RAM starting at address 0.
- The simulator does not parse assembly. It only loads binary bytes.
- The [assembler](../../ComputerSimulator.Assembler/) parses `.asm` and emits raw `.bin`.
- The assembler uses the shared [`InstructionSet`](../../ComputerSimulator.Core/Instructions/InstructionSet.cs); it should not duplicate opcode constants.
- Labels are resolved by the assembler, not by the CPU.
- `JMP` and conditional jumps use one-byte targets. `JMP16` is a software long jump.
- `LDI`, `MOV`, `HALT`, and `JMP16` are pseudo-instructions. They make assembly easier but do not add CPU hardware.
- Dogfood programs live in [`programs/*.asm`](../../programs/); checked-in default binaries live in [`programs/bin/*.bin`](../../programs/bin/).
- Stdlib routines live in [`programs/stdlib/`](../../programs/stdlib/) and use a no-stack convention. `R3` holds public routine return addresses. Scratch RAM constants are `STDLIB_TMP0` through `STDLIB_TMP5`.

## Planned Sections

### 01. The Computer We Have Built

Goal: Introduce the guide and summarize the working simulator: CPU, RAM, buses, I/O, display, keyboard, binary programs, assembler.

Concrete example: Describe a complete but simple program lifecycle at a high level: bytes in RAM, CPU fetches instructions, writes display RAM, renderer shows pixels.

Further reading candidates: [`ComputerSimulator.Core/Computer.cs`](../../ComputerSimulator.Core/Computer.cs), [`ComputerSimulator.Core/Parts/ComputerPart.cs`](../../ComputerSimulator.Core/Parts/ComputerPart.cs), [`docs/ROADMAP.md`](../ROADMAP.md), [`docs/ASSEMBLY.md`](../ASSEMBLY.md).

### 02. The Same Language in One Place

Goal: Explain why instruction encodings must be centralized, and how a shared instruction set keeps the simulator, assembler, and tests speaking the same machine language.

Concrete example: Explain how a mnemonic like `ADD R3, R0` becomes an instruction byte using the shared instruction table, and why duplicated constants could make the assembler and CPU disagree.

Further reading candidates: [`ComputerSimulator.Core/Instructions/InstructionSet.cs`](../../ComputerSimulator.Core/Instructions/InstructionSet.cs), [`ComputerSimulator.Core/Instructions/JumpCondition.cs`](../../ComputerSimulator.Core/Instructions/JumpCondition.cs), [`ComputerSimulator.Core/Instructions/IoInstruction.cs`](../../ComputerSimulator.Core/Instructions/IoInstruction.cs), [relevant instruction tests](../../ComputerSimulator.Core.Tests/Instructions/InstructionSetTests.cs).

### 03. Putting a Program in From the Outside

Goal: Explain raw `.bin` program loading. A file of bytes is loaded into RAM at address 0, and the CPU starts executing those bytes.

Concrete example: Walk through `program.bin`: first byte goes to RAM address 0, second byte to RAM address 1, then the instruction address register begins at the start.

Further reading candidates: [`ComputerSimulator.Core/ProgramLoader.cs`](../../ComputerSimulator.Core/ProgramLoader.cs), [`ComputerSimulator.Core/Computer.cs`](../../ComputerSimulator.Core/Computer.cs), [`ComputerSimulator/Startup.cs`](../../ComputerSimulator/Startup.cs), [`ComputerSimulator.Core/Programs/BuiltInProgramImages.cs`](../../ComputerSimulator.Core/Programs/BuiltInProgramImages.cs).

### 04. Peripherals and Adapters

Already drafted and calibrated. Do not rewrite unless specifically requested.

### 05. The Display Adapter in Practice

Goal: Go deeper on display writes: selecting display address `0x07`, latching display RAM byte addresses, writing pixel bytes, and mapping bytes to pixels.

Concrete example: Walk through making the first eight pixels of the top row show a pattern, then writing the next row. Explain `bytesPerRow` and bit 0 as leftmost.

Further reading candidates: [`ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs`](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs), [`DisplayRam.cs`](../../ComputerSimulator.Core/Peripherals/Display/DisplayRam.cs), [`IoBusControl.cs`](../../ComputerSimulator.Core/Peripherals/Display/IoBusControl.cs), [`ScreenControl.cs`](../../ComputerSimulator.Core/Peripherals/Display/ScreenControl.cs), [`programs/display-pattern.asm`](../../programs/display-pattern.asm).

### 06. A Screen That Can Keep Up

Goal: Explain why faithful gate-level scanning is expensive and why scan-buffer rendering exists. Distinguish scan mode from renderer.

Concrete example: Compare scanning every pixel one at a time versus walking changed display RAM bytes and sending a completed frame to terminal output.

Further reading candidates: [`DisplayAdapter.RenderFrame`](../../ComputerSimulator.Core/Peripherals/Display/DisplayAdapter.cs), [`DisplayScanMode.cs`](../../ComputerSimulator.Core/Peripherals/Display/DisplayScanMode.cs), [`ScreenControl.cs`](../../ComputerSimulator.Core/Peripherals/Display/ScreenControl.cs), [`ComputerSettings.cs`](../../ComputerSimulator.Core/Models/ComputerSettings.cs), [`ComputerSimulator/Tui/TerminalFrameRenderer.cs`](../../ComputerSimulator/Tui/TerminalFrameRenderer.cs), [`docs/PERFORMANCE.md`](../PERFORMANCE.md).

### 07. Pictures of Letters

Goal: Explain font data and drawing text: ASCII code to glyph bytes to display RAM rows.

Concrete example: Walk through drawing `H` or `A`: ASCII value, glyph offset, eight rows, display byte addresses separated by `bytesPerRow`. Mention `HELLO WORLD`.

Further reading candidates: [`AsciiFont8x8.cs`](../../ComputerSimulator.Core/Peripherals/Display/Text/AsciiFont8x8.cs), [`GlyphRenderer.cs`](../../ComputerSimulator.Core/Peripherals/Display/Text/GlyphRenderer.cs), [`programs/assets/font8x8.bin`](../../programs/assets/font8x8.bin), [`programs/hello-world.asm`](../../programs/hello-world.asm), [`programs/stdlib/display.asm`](../../programs/stdlib/display.asm).

### 08. The Keyboard Adapter in Practice

Goal: Go deeper on keyboard input, host input buffering, selected `IN DATA`, one keycode per read window, and `0` for no key.

Concrete example: Press `B`, host queues ASCII `66`, CPU polls, adapter presents `66`, next read returns `0` unless another key was queued.

Further reading candidates: [`IKeyboardInput.cs`](../../ComputerSimulator.Core/Peripherals/Keyboard/IKeyboardInput.cs), [`KeyboardAdapter.cs`](../../ComputerSimulator.Core/Peripherals/Keyboard/KeyboardAdapter.cs), [`ComputerSimulator/Tui/TerminalKeyboardInput.cs`](../../ComputerSimulator/Tui/TerminalKeyboardInput.cs), [keyboard integration tests](../../ComputerSimulator.IntegrationTests/Peripherals/Keyboard/).

### 09. Typing Onto the Screen

Goal: Explain the echo program as the first useful interactive program: poll keyboard, branch on zero, draw glyph, advance cursor, handle Enter and Backspace.

Concrete example: Walk through typing `A`, then Enter, then `B`, describing cursor row/column changes and display writes.

Further reading candidates: [`programs/echo.asm`](../../programs/echo.asm), [`programs/stdlib/keyboard.asm`](../../programs/stdlib/keyboard.asm), [`programs/stdlib/display.asm`](../../programs/stdlib/display.asm), [echo integration tests](../../ComputerSimulator.IntegrationTests/Peripherals/Keyboard/EchoProgramTests.cs).

### 10. The Language Becomes a Tool

Goal: Explain the assembler as a translator from human-readable assembly text to binary instruction bytes. The simulator remains separate and only runs `.bin` files.

Concrete example: A tiny source line with a label and jump becomes bytes with an address filled in by the assembler.

Further reading candidates: [`ComputerSimulator.Assembler/ScottAssembler.cs`](../../ComputerSimulator.Assembler/ScottAssembler.cs), [`AssemblyParser.cs`](../../ComputerSimulator.Assembler/AssemblyParser.cs), [`AssemblySourceLoader.cs`](../../ComputerSimulator.Assembler/AssemblySourceLoader.cs), [`ComputerSimulator.Assembler.Cli/Program.cs`](../../ComputerSimulator.Assembler.Cli/Program.cs), [`docs/ASSEMBLY.md`](../ASSEMBLY.md).

### 11. Words That Save Work

Goal: Explain pseudo-instructions as assembler conveniences, not new CPU instructions.

Concrete example: `LDI` for a small value becomes one `DATA`; `LDI` for a 16-bit value becomes low byte, high byte, shifts, and add. `JMP16` loads a full address and uses `JMPR`.

Further reading candidates: [`docs/ASSEMBLY.md`](../ASSEMBLY.md), [assembler tests](../../ComputerSimulator.Assembler.Tests/), [`InstructionSet.cs`](../../ComputerSimulator.Core/Instructions/InstructionSet.cs), [`programs/*.asm` examples](../../programs/).

### 12. Programs Written in Their Own Language

Goal: Explain dogfooding: demos now live as `.asm`, are assembled to `.bin`, and are loaded by the simulator.

Concrete example: Follow [`display-pattern.asm`](../../programs/display-pattern.asm) or [`hello-world.asm`](../../programs/hello-world.asm) from source, through assembler, to checked-in binary, to [`ProgramLoader`](../../ComputerSimulator.Core/ProgramLoader.cs), to visible behavior.

Further reading candidates: [`programs/display-pattern.asm`](../../programs/display-pattern.asm), [`programs/hello-world.asm`](../../programs/hello-world.asm), [`programs/echo.asm`](../../programs/echo.asm), [`programs/bin/*.bin`](../../programs/bin/), [`DogfoodProgramTests.cs`](../../ComputerSimulator.IntegrationTests/Assembler/DogfoodProgramTests.cs).

### 13. Useful Routines Without New Hardware

Goal: Explain the assembly standard library. Since there is no stack, routines use explicit conventions: return address in `R3`, scratch bytes, includes, and documented clobbers.

Concrete example: Walk through a program that wants to print a string: include display stdlib, put string address in `R0`, display address in `R2`, return address in `R3`, jump to `stdlib_print_string`.

Further reading candidates: [`programs/stdlib/common.asm`](../../programs/stdlib/common.asm), [`io.asm`](../../programs/stdlib/io.asm), [`display.asm`](../../programs/stdlib/display.asm), [`keyboard.asm`](../../programs/stdlib/keyboard.asm), [`math.asm`](../../programs/stdlib/math.asm), [`memory.asm`](../../programs/stdlib/memory.asm), [`StdlibProgramTests.cs`](../../ComputerSimulator.IntegrationTests/Assembler/StdlibProgramTests.cs).

### 14. Where This Leaves the Computer

Goal: Conclude the guide. The machine now has a CPU, RAM, I/O bus, display, keyboard, loader, assembler, dogfood programs, and reusable routines. Set up future work without turning the section into a roadmap.

Concrete example: Describe what happens from writing a line of assembly to seeing a character appear after running the simulator.

Further reading candidates: [`docs/ROADMAP.md`](../ROADMAP.md), [`docs/ASSEMBLY.md`](../ASSEMBLY.md), [`programs/`](../../programs/), [`ComputerSimulator.Core/Computer.cs`](../../ComputerSimulator.Core/Computer.cs).

## Agent Workflow

When drafting sections:

1. Read this guide.
2. Read [`04-peripherals-and-adapters.md`](04-peripherals-and-adapters.md) for style.
3. Read only the code/docs needed for the assigned sections.
4. Write only the assigned section files.
5. Do not edit unrelated files.
6. Use plain Markdown and ASCII text.
7. Avoid code blocks unless the section truly needs one. Prefer prose and small tables.
8. Keep section prose free of roadmap labels.
9. Include `Further reading in the simulator` at the end, with clickable links for file and directory targets.
