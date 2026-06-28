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
- **Perf:** everything is modelled gate-by-gate, so it is slow. Keep an eye on this; it bounds resolution
  and frame rate. The display now has a faster scan-buffer path, but the CPU and gate-level scanner are still
  intentionally faithful and slow.

---

## Foundations (enablers — do these as the milestones that need them come up)

- [ ] **F1. Centralize the instruction set.** Extract the encodings/mnemonics scattered in `DemoProgram`
  and the tests into one `Core/Instructions/InstructionSet.cs` (or similar): an enum/table mapping
  mnemonic + operands → bytes, plus a tiny encoder. *Done when* `DemoProgram` and tests use it and the
  byte output is unchanged. *Enables M4 (assembler).*
- [ ] **F2. Program loader (`.bin` images).** Let the simulator load a raw binary image into RAM and run
  it, instead of baking programs into `DemoProgram`. Accept a file path on the command line (e.g.
  `dotnet run --project ComputerSimulator -- run demo.bin`); the loader reads the bytes into RAM starting at
  address 0 and the CPU executes from there. *Touches* `Computer`, `Program.cs`/`Startup`, a small
  `ProgramLoader`. *Done when* the machine boots an arbitrary `.bin` from disk. *Enables M2–M6.*
  *Notes:* keep the format dead simple — raw bytes loaded at 0. (A tiny header with a load address/entry
  point can come later if needed.) This is the **second half** of the assembler flow in M4.
- [ ] **F3. Symmetric input abstraction.** Add `Core/Peripherals/Keyboard/IKeyboardInput.cs` (mirror of
  `IDisplayOutput`): the host pushes key events, the adapter reads them. *Enables M3.*

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

- [ ] **M2.1 Font data.** An 8×8 bitmap font for printable ASCII (32–126), as 8 bytes/glyph. Store as a
  C# resource and as a RAM-loadable "font ROM" image. *Touches* `Core` (font resource), F2 loader.
  *Done when* the font is addressable as `font[ascii*8 + row]`.
- [ ] **M2.2 Draw-character routine.** Blit a glyph to display RAM at character cell `(cx, cy)`. Validate
  first as a C# helper, then as a CPU subroutine. Address math: `displayByteAddr = ((cy*8 + r) *
  bytesPerRow) + cx` for glyph row `r` (cx is a byte column). *Done when* a known glyph appears at a known
  cell and a test asserts the pixels.
- [ ] **M2.3 Draw-string routine.** Iterate characters, advance `cx`, handle newline/wrap. *Done when*
  a string renders correctly across line wraps.
- [ ] **M2.4 "HELLO WORLD" demo.** A program that prints text via the CPU. *Done when* readable text
  shows on screen (replace or sit alongside `DemoProgram`).
- [ ] **M2.5 Tests.** Glyph blit + string render assertions using `FakeDisplayOutput`.
  *Notes:* much easier after M4 (assembler); until then generate the program in C# like `DemoProgram`.

---

## Milestone 3 — Keyboard input (type and see text)

- [ ] **M3.1 Wire the keyboard into the live machine.** Add `KeyboardAdapter` to
  `IoBus.ConnectedComponents` in `Computer`. *Done when* a program can `IN Data` a keycode the host supplied.
- [ ] **M3.2 Host key source.** Capture real keystrokes (terminal raw mode `Console.ReadKey`, or GUI key
  callbacks from M1.1) and feed them to the adapter via F3's `IKeyboardInput`. *Done when* pressing a key
  makes the adapter return its ASCII code, then `0` until the next key.
- [ ] **M3.3 Echo loop program.** Poll the keyboard; on a non-zero keycode, draw the character at the
  cursor (M2 routines) and advance it. Handle Enter (newline) and Backspace. *Done when* typing shows text.
- [ ] **M3.4 Tests.** Inject a keycode on the adapter, run the echo program, assert the glyph appears.
  *Notes:* the CPU polls (no interrupts yet — see M6.2). The host must refresh the keyboard's input
  between CPU ticks. Beware buffering: the adapter holds one keycode and clears on read.

---

## Milestone 4 — Assembler (write programs in text, not byte arrays)

**Target flow** (assembler and simulator are separate tools that meet at a `.bin` file):

```
program.asm  ──[ asm CLI ]──►  program.bin  ──[ ComputerSimulator ]──►  runs on the CPU
```

The assembler is its own command-line program. The simulator just loads and runs the resulting binary
(F2) — it knows nothing about assembly. The shared encoding table (F1) is the only thing both use.

- [ ] **M4.1 Assembly syntax.** Define mnemonics (`DATA, LD, ST, ADD, SHL, SHR, NOT, AND, OR, XOR, CMP,
  CLF, JMP, JMPR, JC/JA/JE/JZ (+combos), IN, OUT`), `R0`–`R3` operands, labels, immediates, comments.
  Track the book's notation where sensible.
- [ ] **M4.2 Assembler CLI (`.asm` → `.bin`).** A standalone command-line tool, e.g.
  `asm program.asm -o program.bin`, that reads assembly text and writes a raw binary. Internally a
  two-pass assembler: pass 1 resolves labels → addresses, pass 2 emits bytes (handling the 2-byte
  `DATA`/`JMP`/jump-if forms and label fixups), built on F1's shared encoder. Put the logic in a library
  (`ComputerSimulator.Assembler`) with a thin CLI project on top. *Touches* a new assembler project; pairs
  with **F2** (the simulator loading `.bin`). *Done when* `asm demo.asm -o demo.bin` then running `demo.bin`
  on the simulator reproduces the demo, and a multi-label program assembles and runs correctly.
- [ ] **M4.3 Pseudo-instructions.** Provide conveniences the tight ISA needs: `LDI Rn, imm16` (expands to
  `DATA`+`SHL`+`ADD`), `MOV`, `HALT`, `CALL`/`RET` if a stack lands (M6.1). *Done when* programs read
  cleanly despite 8-bit immediates and 4 registers.
- [ ] **M4.4 Dogfood.** Re-express `DemoProgram` and the M2/M3 routines as `.asm` files, assemble them to
  `.bin`, and load those. *Done when* the hand-coded byte arrays are gone and programs live as `.asm`.
- [ ] **M4.5 Standard library (.asm).** Routines the platform lacks in hardware: `mul`, `div`, `memcpy`,
  `print_char`, `print_string`, `read_line`. *Done when* programs can `include`/reuse them.

---

## Milestone 5 — Visual workbench (GUI / IDE)

Deferred until after the assembler on purpose: a real GUI is *not* needed while the terminal display
works, but once you can write and assemble programs (M4) a full GUI becomes worthwhile — not just a
prettier screen, but a window where you **watch your assembly execute** next to the live display.

- [ ] **M5.1 Windowed display renderer.** Implement `IDisplayOutput` against a real window (Silk.NET,
  Raylib-cs, or SDL2), showing true pixels at the book's 320×200 (or larger). Reuses the M1.1 scan modes
  unchanged — this is just another renderer behind the existing seam. *Touches* a new host project/class;
  register it in `Startup` alongside `TerminalDisplayOutput`. *Done when* a window shows the display at full
  resolution. *Notes:* at 320×200 prefer the **scan-buffer** mode (M1.1); the gate-level scanner will be too
  slow for real-time.
- [ ] **M5.2 System view / live execution.** Panels showing CPU registers (`R0`–`R3`, IAR, IR, ACC, TMP,
  flags), the stepper/clock, and a RAM/hex view — all updating as the program runs, beside the display.
  *Touches* a GUI app over `ComputerPart` (everything is already inspectable via its public surface, as the
  diagnostics in `CpuSequencingTests` showed). *Done when* you can see state change instruction-by-instruction.
- [ ] **M5.3 Debugger controls.** Run / pause / single-step / step-instruction, breakpoints (by address),
  and a **disassembler** (bytes → mnemonics, the inverse of F1's encoder). *Done when* you can stop on an
  address and step through. *Notes:* a minimal **CLI** stepping/inspection harness is worth building much
  earlier (anytime after F1) — it pays for itself on every program; the GUI version is the richer form.
- [ ] **M5.4 Assembly IDE.** Edit `.asm`, assemble (M4), load (F2), and run — all in the GUI, with errors
  surfaced and the result visible on the embedded screen. *Done when* you can write a program and watch it
  run end-to-end without leaving the app.

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

**M2 (fonts/text)** is now the highest-leverage move: M1's terminal-first display work is largely in place,
so text rendering finishes the book and makes the display meaningful. M2 is also a prerequisite for the
keyboard echo loop (M3) and any OS prompt (M6.4). Doing **F1** (centralized encoder) and a thin slice of
**M4** (even a minimal assembler) first will make writing the font/echo/OS programs dramatically easier than
hand-assembling byte arrays. The GUI (M5) remains deliberately parked until after M4; grayscale/color is also
deferred until GUI work makes it worth designing the protocol change.
