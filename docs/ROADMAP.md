# Roadmap / Task Board

A guide for taking the simulator beyond "a working display" toward something close to a real machine
with software and (eventually) an OS. Ordered roughly by priority. Each task lists a **Goal**, the
**files** it touches, **Done when** acceptance criteria, and **Notes** (including gotchas we already hit).

## Where we are

We have implemented essentially the whole of *But How Do It Know?* (J. Clark Scott) at the hardware
level — gates → circuits → a working CPU (the "Scott CPU") with the full instruction set, an IO bus,
a keyboard adapter, and a display adapter that renders to the terminal. Multi-instruction programs
(loops, arithmetic, conditional branches) work. A demo program fills the screen with a pattern.

**The book's remaining material is text rendering** (ASCII + fonts, ch. 54–55). Past that we are in
original territory: assembler, more peripherals, an OS, higher-level languages.

### Key facts a future agent needs

- **Registers:** 4 general-purpose (`R0`–`R3`), 16-bit words (`ComputerSettings.WordSize`), but `DATA`
  immediates are only **8 bits**. Hold larger values in a register and build them with `SHL`/`ADD`
  (see `DemoProgram.LoadConstant`).
- **Instruction encodings** live informally in `DemoProgram` and `IntegrationTests/Parts/ComputerPartTests`.
  Gotcha: jump-if uses prefix `0101` with flag-select bits in IW(4..7) = C,A,E,Z. "Jump if Equal" is
  **`0x52`**, not `0x42` (`0x42` is unconditional `JMP`).
- **Peripherals** attach via `computerPart.IoBus.ConnectedComponents.Add(adapter)` and update each tick.
- **IO protocol:** `OUT Addr Rn` puts a device address on the bus. `0x07` = display, `0x0F` = keyboard
  (`Core/Peripherals/IoAddress.cs`). After selecting a device, `OUT Data`/`IN Data` exchange bytes.
- **Display writes:** once selected, `OUT Addr Rn` latches a display-RAM **byte address**, `OUT Data Rn`
  writes one **pixel byte** (8 px, bit 0 = leftmost). Linear byte address `N` → row `N / bytesPerRow`,
  byte-column `N % bytesPerRow`, where `bytesPerRow = width / 8`.
- **Keyboard:** `KeyboardAdapter` (built + tested, **not yet wired into the running machine**). Select
  `0x0F`, then `IN Data Rn` reads the ASCII keycode (`0` if no key); the keycode register auto-clears
  after a read.
- **Render abstraction:** Core defines `IDisplayOutput`; the host implements it (`TerminalDisplayOutput`
  → `Graphics.Screen`). `DisplayAdapter.RenderFrame` scans display RAM using a config-selected scan mode.
  `Computer.RunAsync` drives the loop.
- **Display scan modes:** `ComputerSettings.DisplayScanMode` selects `GateLevel` (default/reference) or
  `ScanBuffer` (fast path). Gate-level uses `ScreenControl`; scan-buffer walks display RAM bytes directly.
- **Terminal renderer:** `Terminal.PixelMode` selects `Braille` (2×4 px per terminal cell, default) or
  `Block` (old 1 px per cell style). `Terminal.LogLines` reserves rows below the display for recent logs,
  so app settings and runtime messages stay visible without corrupting the screen.
- **Configuration:** runtime settings are bound through options from the appsettings file beside the executable.
  This matters when using `dotnet run --project ComputerSimulator`, because the process content root is the repo
  root while the copied config lives under the app output directory.
- **CLI overrides:** common runtime settings can be passed as short flags, e.g.
  `dotnet run --project ComputerSimulator -- --scan-mode buffer` or `--scan-mode gate`.
- **Perf:** everything is modelled gate-by-gate, so it is slow. Keep an eye on this; it bounds resolution
  and frame rate. The display now has a faster scan-buffer path, but the CPU and gate-level scanner are still
  intentionally faithful and slow.

---

## Foundations (enablers — do these as the milestones that need them come up)

- [x] **F1. Centralize the instruction set.** Extract the encodings/mnemonics scattered in `DemoProgram`
  and the tests into one `Core/Instructions/InstructionSet.cs` (or similar): an enum/table mapping
  mnemonic + operands → bytes, plus a tiny encoder. *Done when* `DemoProgram` and tests use it and the
  byte output is unchanged. *Enables M4 (assembler).* *Status:* `InstructionSet` and `JumpCondition` now
  encode the current instruction families. `DemoProgram`, display tests, CPU sequencing tests, and full-instruction
  CPU/pin integration tests use it where that improves clarity. Remaining raw bytes are intentional encoding-contract
  assertions, instruction operands, bit-level decoder assertions, or non-instruction test data.
- [x] **F2. Program loader (`.bin` images).** Let the simulator load a raw binary image into RAM and run
   it, instead of baking programs into `DemoProgram`. Accept a file path on the command line (e.g.
   `dotnet run --project ComputerSimulator -- run demo.bin`); the loader reads the bytes into RAM starting at
   address 0 and the CPU executes from there. *Touches* `Computer`, `Program.cs`/`Startup`, a small
   `ProgramLoader`. *Done when* the machine boots an arbitrary `.bin` from disk. *Enables M2–M6.*
   *Notes:* keep the format dead simple — raw bytes loaded at 0. (A tiny header with a load address/entry
   point can come later if needed.) This is the **second half** of the assembler flow in M4. *Status:*
   `ProgramLoader` reads raw binary files and loads bytes into RAM at address 0; the simulator accepts
   `run <path>`, `--program <path>`, and `--program-path <path>` while preserving the built-in demo default.
- [x] **F3. Symmetric input abstraction.** Add `Core/Peripherals/Keyboard/IKeyboardInput.cs` (mirror of
   `IDisplayOutput`): the host pushes key events, the adapter reads them. *Enables M3.* *Status:*
   `IKeyboardInput` and `BufferedKeyboardInput` provide a host-pushed FIFO key queue consumed by
   `KeyboardAdapter` during `IN Data`.
- [x] **F4. Minimal CPU program emitter.** A narrow C# byte emitter for generated CPU programs before the
  full M4 assembler exists. *Done when* generated programs can load 16-bit constants, use `InstructionSet`,
  produce raw bytes, and avoid hand-maintained byte arrays for M2 text programs. *Status:* `MachineProgramBuilder`
  now emits M2 CPU text programs, including a 16-bit `JMPR` halt for programs beyond the 8-bit `JMP` range.

---

## Milestone 1 — Better display (terminal-first)

The display has two **independent** axes; keep them decoupled:

- **Scan mode** — *how* display RAM becomes pixels: the faithful **gate-level** scanner (what we have) vs
  a faster **scan-buffer** path. This lives in `ScreenControl`/`DisplayAdapter` and is renderer-agnostic.
- **Renderer** — *how* pixels are shown: terminal (now) or a GUI window (deferred to M5). This is the
  `IDisplayOutput` seam.

A GUI is **not** a priority while the terminal renderer works well — focus M1 on the terminal and on the
dual scan modes, which both the terminal and the eventual GUI will reuse.

- [x] **M1.1 Dual scan modes (gate-level + scan-buffer).** Keep the current **gate-level** `ScreenControl`
  (real horizontal/vertical counter registers, one pixel per clock — faithful to the book, but slow and
  unlikely to sustain higher resolutions in real time). Add a faster **scan-buffer** mode, like
  [djhworld/simple-computer](https://github.com/djhworld/simple-computer): a routine walks display RAM by
  computed address (~30 Hz), reads whole bytes (8 px at a time) into a frame buffer, and hands it to the
  renderer — no per-pixel gate counters, ~an order of magnitude cheaper. Make the mode **config-selectable**
  and **renderer-agnostic** (works for terminal now and GUI later). *Touches* `ScreenControl`/`IScreenControl`,
  `DisplayAdapter.RenderFrame`, `ComputerSettings`. *Done when* both modes render the same image, the buffer
  mode is much faster, and both are tested. *Notes:* `DisplayRam` already exposes `UpdateRead()` and
  `GetSlot(x, y)`; byte→pixel mapping is `bytesPerRow = width/8`, bit 0 = leftmost. Gate-level stays the
  default/reference; scan-buffer is the opt-in fast path, not a replacement. *Status:* implemented via
  `DisplayScanMode`; scan-buffer and gate-level parity are covered by integration tests.
- [x] **M1.2 Improve the terminal renderer.** Get more out of the terminal before reaching for a GUI:
  - Higher pixel density per character cell — half-blocks (`▀`/`▄`, 1×2 px), quadrants (2×2 px), or
    **braille** (`⠿`, 2×4 px). Braille fits 320×200 in ~160×50 cells — a large but real terminal window.
  - ANSI color / grayscale output (pairs with M1.4).
  - Terminal resize handling, cursor hidden, minimal flicker (already buffer + single flush).
  *Touches* `Graphics/Screen.cs`, `TerminalDisplayOutput`. *Done when* the terminal shows noticeably higher
  effective resolution and (optionally) color, and degrades gracefully on small windows. *Status:* braille and
  block modes are config-selectable via `Terminal.PixelMode`; terminal-safe log rows are rendered below the
  display via `Terminal.LogLines`. Color remains deferred with M1.5.
- [x] **M1.3 Dirty-region / decoupled refresh.** Only re-scan/redraw changed display-RAM bytes; decouple
  the display refresh rate from the CPU loop. *Touches* `DisplayAdapter.RenderFrame`, `ScreenControl`,
  `Computer.RunAsync`. *Done when* a static screen costs ~0 per frame. (Most relevant to the gate-level mode.)
  *Status:* display RAM tracks dirty byte addresses; scan-buffer mode skips `Present()` when unchanged and
  re-renders changed bytes only. Gate-level still performs full reference scans.
- [x] **M1.4 Fully data-driven sizing.** Make width/height (and `bytesPerRow`, DisplayRam capacity) flow
  cleanly from `ComputerSettings`; document the constraints. *Done when* changing the config resizes the
  display with no code edits and tests cover a couple of sizes. *Status:* `ScreenWidth`, `ScreenHeight`,
  `CpuUpdatesPerFrame`, `DisplayFrameDelayMs`, and `DisplayScanMode` are options-bound and validated.
- [ ] **M1.5 (deferred until after GUI) Grayscale/color.** Extend DisplayRam to >1 bit/pixel or add a palette. Diverges
  from the book's monochrome; design the IO protocol for it. Feeds the terminal color work (M1.2) and the
  later GUI. *Done when* the demo can draw shades/colors. *Notes:* intentionally not a near-term priority.
- [ ] **M1.6 Performance pass (partial).** Profile the gate simulation (`EventBus`, register/word updates, the
  per-tick `ComputerPart.Update`). Land targeted optimizations without changing behavior. *Done when*
  there is a benchmark and a measured speedup; all tests still green. *Status:* an explicit render benchmark
  exists for gate-level, scan-buffer, and static scan-buffer frames. A broader CPU/gate simulation profile is
  still open.

---

## Milestone 2 — Text to the screen (fonts) — *finishes the book*

- [x] **M2.1 Font data.** An 8×8 bitmap font for printable ASCII (32–126), as 8 bytes/glyph. Store as a
   C# resource and as a RAM-loadable "font ROM" image. *Touches* `Core` (font resource), F2 loader.
   *Done when* the font is addressable as `font[ascii*8 + row]`. *Status:* `AsciiFont8x8` provides an
   addressable `font[ascii*8 + row]` API with tests for the printable range. It now exposes a 1024-byte
   RAM-loadable ROM image (`AsciiFont8x8.CreateRomImage()`), and M2 text program images place that ROM in
   simulated RAM at `TextProgram.FontBaseAddress`. Uppercase letters, digits, and common punctuation use
   hand-tuned glyphs; other printable ASCII slots have deterministic generated glyphs until a nicer font is added.
- [x] **M2.2 Draw-character routine.** Blit a glyph to display RAM at character cell `(cx, cy)`. Validate
   first as a C# helper, then as a CPU subroutine. Address math: `displayByteAddr = ((cy*8 + r) *
   bytesPerRow) + cx` for glyph row `r` (cx is a byte column). *Done when* a known glyph appears at a known
   cell and a test asserts the pixels. *Status:* `GlyphRenderer.DrawCharacter` validates the C# helper path
   through the existing display IO protocol, with gate-level and scan-buffer tests asserting glyph pixels.
   `TextProgram` now emits CPU instructions that load glyph rows from the RAM font ROM and write them through
   the display IO protocol; tests assert CPU-rendered glyph pixels in both scan modes.
- [x] **M2.3 Draw-string routine.** Iterate characters, advance `cx`, handle newline/wrap. *Done when*
   a string renders correctly across line wraps. *Status:* `GlyphRenderer.DrawString` validates the C# helper
   path through the existing display IO protocol, with gate-level and scan-buffer tests covering adjacent
   characters, newline handling, wrapping, and overflow. `TextProgram` now emits CPU-driven string programs
   with newline and wrap handled at generation time, and tests assert CPU-rendered adjacent characters plus
   newline/wrap behavior.
- [x] **M2.4 "HELLO WORLD" demo.** A program that prints text via the CPU. *Done when* readable text
   shows on screen (replace or sit alongside `DemoProgram`). *Status:* `TextProgram.BuildHelloWorldImage`
   builds a raw CPU image with program bytes at address 0 and the font ROM in RAM. The simulator can run it
   via `dotnet run --project ComputerSimulator -- --demo hello-world`; the original display-pattern demo remains
   the default.
- [x] **M2.5 Tests.** Glyph blit + string render assertions using `FakeDisplayOutput`.
   *Notes:* much easier after M4 (assembler); until then generate the program in C# like `DemoProgram`.
   *Status:* glyph blit and C# string render assertions exist for both display scan modes; CPU-driven string
   render assertions now cover single glyphs, adjacent characters, newline/wrap behavior, HELLO WORLD, font ROM
   image placement, raw `.bin` loader behavior, and host CLI program selection.

---

## Milestone 3 — Keyboard input (type and see text)

- [x] **M3.1 Wire the keyboard into the live machine.** Add `KeyboardAdapter` to
  `IoBus.ConnectedComponents` in `Computer`. *Done when* a program can `IN Data` a keycode the host supplied.
  *Status:* `Computer` creates the keyboard adapter and connects it alongside the display adapter. The adapter
  consumes queued keycodes during selected keyboard `IN Data` cycles and returns `0` when no key is available.
- [x] **M3.2 Host key source.** Capture real keystrokes (terminal raw mode `Console.ReadKey`, or GUI key
  callbacks from M1.1) and feed them to the adapter via F3's `IKeyboardInput`. *Done when* pressing a key
  makes the adapter return its ASCII code, then `0` until the next key. *Status:* the Terminal.Gui host maps
  printable ASCII, Enter (`13`), Backspace/Delete (`8`) and pushes them through `IKeyboardInput`.
- [x] **M3.3 Echo loop program.** Poll the keyboard; on a non-zero keycode, draw the character at the
  cursor (M2 routines) and advance it. Handle Enter (newline) and Backspace. *Done when* typing shows text.
  *Notes:* For the first M3 implementation, Backspace at column `0` is a no-op. This keeps cursor
  bookkeeping simple before the assembler exists. Revisit later if line-wrapping/backspacing across lines
  becomes important. *Status:* `EchoProgram` is available as `--demo echo` / `--demo keyboard`. It polls the
  keyboard, draws glyphs from the M2 font ROM, advances cursor state, handles Enter, and erases the previous
  character for Backspace. Cursor state currently lives in low RAM bytes `0x02`-`0x04` behind an initial jump;
  this can become cleaner once M4 assembly/pseudo-instructions exist.
- [x] **M3.4 Tests.** Inject a keycode on the adapter, run the echo program, assert the glyph appears.
  *Notes:* the CPU polls (no interrupts yet — see M6.2). The host must refresh the keyboard's input
  between CPU ticks. Beware buffering: the adapter holds one keycode and clears on read. *Status:* tests cover
  the buffered input queue, adapter one-shot/no-key reads, Terminal.Gui key mapping, CLI aliases, raw echo
  glyph rendering, runtime keypresses while polling, quickly queued characters, runtime display sizing, lowercase
  input, font ROM placement, and gate-level echo rendering.

---

## Milestone 4 — Assembler (write programs in text, not byte arrays)

**Target flow** (assembler and simulator are separate tools that meet at a `.bin` file):

```
program.asm  ──[ asm CLI ]──►  program.bin  ──[ ComputerSimulator ]──►  runs on the CPU
```

The assembler is its own command-line program. The simulator just loads and runs the resulting binary
(F2) — it knows nothing about assembly. The shared encoding table (F1) is the only thing both use.

- [x] **M4.1 Assembly syntax.** Define mnemonics (`DATA, LD, ST, ADD, SHL, SHR, NOT, AND, OR, XOR, CMP,
  CLF, JMP, JMPR, JC/JA/JE/JZ (+combos), IN, OUT`), `R0`–`R3` operands, labels, immediates, comments.
  Track the book's notation where sensible. *Status:* implemented in `ComputerSimulator.Assembler`; mutating
  operations use conventional destination-first syntax while `CMP` preserves written operand order.
- [x] **M4.2 Assembler CLI (`.asm` → `.bin`).** A standalone command-line tool, e.g.
  `asm program.asm -o program.bin`, that reads assembly text and writes a raw binary. Internally a
  two-pass assembler: pass 1 resolves labels → addresses, pass 2 emits bytes (handling the 2-byte
  `DATA`/`JMP`/jump-if forms and label fixups), built on F1's shared encoder. Put the logic in a library
  (`ComputerSimulator.Assembler`) with a thin CLI project on top. *Touches* a new assembler project; pairs
  with **F2** (the simulator loading `.bin`). *Done when* `asm demo.asm -o demo.bin` then running `demo.bin`
  on the simulator reproduces the demo, and a multi-label program assembles and runs correctly. *Status:*
  `ComputerSimulator.Assembler.Cli` writes raw binaries, supports `-D NAME=value`, and a dogfood integration
  test assembles `programs/display-pattern.asm`, loads the binary, and verifies the display output.
- [x] **M4.3 Pseudo-instructions.** Provide conveniences the tight ISA needs: `LDI Rn, imm16` (expands to
  `DATA`+`SHL`+`ADD`), `MOV`, `HALT`, `CALL`/`RET` if a stack lands (M6.1). *Done when* programs read
  cleanly despite 8-bit immediates and 4 registers. *Status:* `LDI`, `MOV`, `HALT`, and explicit-register
  `JMP16` are implemented. `CALL`/`RET` remain deferred until the stack work in M6.1.
- [x] **M4.4 Dogfood.** Re-express `DemoProgram` and the M2/M3 routines as `.asm` files, assemble them to
  `.bin`, and load those. *Done when* the hand-coded byte arrays are gone and programs live as `.asm`.
  *Status:* `programs/display-pattern.asm`, `programs/hello-world.asm`, and `programs/echo.asm` are the source
  programs. Default-dimension binaries live in `programs/bin/` and are loaded by built-in runtime selection.
  Integration tests assemble source, load the emitted bytes through `ProgramLoader`, run the simulator, and verify
  display/keyboard behavior. Tests also compare checked-in default `.bin` files to fresh assembler output so binary
  assets cannot drift from source. Custom dimensions are handled by assembling with `-D` values and running the
  resulting `.bin`.
- [x] **M4.5 Standard library (.asm).** Routines the platform lacks in hardware: `mul`, `div`, `memcpy`,
  `print_char`, `print_string`, `read_line`. *Done when* programs can `include`/reuse them. *Status:*
  `programs/stdlib/` contains shared constants plus reusable `math`, `memory`, `display`, and `keyboard` routines.
  `programs/hello-world.asm` reuses `stdlib_print_string`; integration tests compile stdlib-using programs, load the
  emitted bytes, run the simulator, and verify `mul`, `div`, `memcpy`, `print_char`, `print_string`, and `read_line`.

---

## Milestone 5 — Visual workbench (TUI)

Reframed from a separate GUI into the **existing Terminal.Gui TUI** (`ComputerSimulator/Tui/`). A second
native-window renderer is redundant — the TUI *is* the renderer, already drawing the display, logs, and
feeding keyboard input. Once you can write and assemble programs (M4), the workbench becomes worthwhile:
not a prettier screen, but a terminal where you **watch your assembly execute** next to the live display.
The window's `RightFrame` (25% width) is currently empty and is the home for the new debug panels; a
bottom controls bar drives execution. Same features as the old GUI plan, all inside the TUI.

- [x] **M5.1 ~~Windowed display renderer~~ — obsolete (the TUI is the renderer).** The Terminal.Gui host
  already implements `IDisplayOutput` (`TerminalGuiDisplayOutput` → `TerminalDisplayBuffer`) and reuses the
  M1.1 scan modes unchanged behind the existing seam. No separate Silk.NET/Raylib/SDL window is needed; a
  real-pixel GUI window can return later as just another renderer behind `IDisplayOutput` if ever wanted.
- [ ] **M5.2 System view / live execution (TUI panels).** Fill `RightFrame` with stacked views — CPU
  registers (`R0`–`R3`, IAR, IR, ACC, TMP, hex + decimal), CAEZ flags, and a scrollable RAM/hex view that
  highlights the IAR row — all updating as the program runs, beside the display. *Touches*
  `ComputerSimulatorWindow` (new `RegistersView`/`RamHexView`), `TerminalGuiApplication` (a coalesced
  `RefreshDebug()` driven from `Computer.RunAsync`). State is read from the public `IComputerPart` surface
  (`Acc`, `Iar`, `Ir`, `Tmp`, `GeneralPurposeRegisters`, `Caez`, `Ram`) via `BinaryHelpers.ToInt`. *Done
  when* you can see state change instruction-by-instruction in the terminal.
- [ ] **M5.3 Debugger controls + disassembler.** Add an `IExecutionController` in Core that gates the
  `Computer.RunAsync` loop: run / pause / single-step (one clock tick) / step-instruction (run to the next
  fetch boundary via `ICentralProcessingUnit.Stepper.CurrentStep`, fetch = step 1) and address breakpoints
  (compare `Iar` at the fetch boundary, auto-pause). Drive it from a TUI controls bar (`[Run] [Pause]
  [Step] [Step Insn] [Breakpoint…]`) with key bindings. Add a **disassembler** in Core
  (`Instructions/Disassembler.cs`, the inverse of F1's `InstructionSet` encoder, knowing which opcodes
  consume a trailing operand byte) and a `DisassemblyView` showing a window around IAR with the current
  instruction highlighted. *Done when* you can stop on an address and step through with live disassembly.
- [ ] **M5.4 Assembly editor (in-TUI).** An editor dialog (Terminal.Gui `TextView`, opened by a key) to
  edit `.asm`, assemble via `ScottAssembler.AssembleText` (M4), reload the bytes with `ProgramLoader.Load`
  (F2), and run — errors surfaced in the log/status line, the result visible on the embedded display.
  *Done when* you can write a program and watch it run end-to-end without leaving the TUI.

*Start mode:* the simulator free-runs by default; a new `--debug` flag (`Computer:StartPaused`) boots
paused at entry so you can step from the first instruction.

---

## Milestone 6 — The wilder stuff (toward a real machine + OS)

Roughly increasing ambition. Several of these unlock the others.

- [ ] **M6.1 Stack + `CALL`/`RET`.** The Scott CPU has no stack; subroutines are faked with saved
  addresses and `JMPR`. Add a stack pointer register + push/pop + `CALL`/`RET` (new control wiring and a
  couple of instructions). *Why important:* real structured programs and an OS need cheap subroutines.
- [ ] **M6.2 Interrupts.** Add an IRQ line + vector + state save/restore so the keyboard/timer can be
  event-driven instead of polled. A genuine architectural extension; design carefully (new control wiring,
  maybe `STI`/`CLI`/`IRET`).
- [ ] **M6.3 More peripherals.** Each is an `IAdapter` on the IO bus with its own address:
  - [ ] **Timer** (tick counter / interval) — system clock, delays, animation.
  - [ ] **Storage / "disk"** (block read/write to a backing file) — persistence for programs and data.
  - [ ] **RNG** device.
  - [ ] **Serial/console** text device for debugging.
  - [ ] **Sound** (tone/beep).
- [ ] **M6.4 Boot ROM / monitor program.** On reset, run a ROM that clears the screen, prints a banner,
  and drops to a prompt. *Depends on* M2 (text) + M3 (keyboard) + F2 (loader).
- [ ] **M6.5 Tiny OS / shell.** A command interpreter (`load`, `run`, `peek`, `poke`, `clear`, `dir`),
  a line editor, and a simple filesystem on the disk device. Written in assembly (or M6.7). *Depends on*
  M6.1 (stack) heavily.
- [ ] **M6.6 Showcase programs.** Conway's Life, a clock, Snake, a text adventure — proves display +
  keyboard + timer together.
- [ ] **M6.7 (very wild) High-level language.** A minimal compiler targeting the ISA — a tiny C-subset or
  Forth (Forth fits constrained machines beautifully). Lets programs stop being hand-assembly.
- [ ] **M6.8 (very wild) Self-hosting.** Run the assembler (M4) or the language (M6.7) *on the simulated
  machine itself*.
- [ ] **M6.9 (very wild) ISA expansion.** Document and optionally extend the architecture: more registers,
  wider immediates, indexed/relative addressing, larger address space. Diverges from the book but is what
  "real software" wants. Keep a compatibility note since it touches the whole CPU.

---

## Suggested next step

Continue to **M4 (assembler)**. M2 text and M3 keyboard echo now work through generated CPU programs, but the
real `print_char`/`print_string`/`read_key` standard-library shape will become much cleaner once M4 assembly
syntax, labels, and pseudo-instructions exist. The visual workbench (M5) is now reframed to land inside the
existing TUI rather than a separate GUI; grayscale/color (M1.5) stays deferred until it is worth designing
the protocol change.
