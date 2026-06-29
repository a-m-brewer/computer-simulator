# Extended Machine Design Notes

This note is for future coding agents implementing stack and interrupt support. It is intentionally more detailed than the roadmap. The goal is to let the simulator grow beyond the book without making the book-accurate machine harder to understand, test, or trust.

## Core Rule

Keep the current Scott/book machine as the default. Stack and interrupts are extensions, not corrections. A program that runs in book mode should keep seeing the current CPU, instruction set, polling keyboard behavior, raw `.bin` loader behavior, and existing assembler dialect.

Do not add a trail of `if (extended)` branches inside the current CPU control logic. The CPU is the most important artifact for book faithfulness. If extended behavior needs different control wiring, put that behavior beside the book CPU rather than inside it.

## Concepts

A stack is not a peripheral. It is normally ordinary RAM used with a stack pointer and CPU instructions such as `PUSH`, `POP`, `CALL`, and `RET`. The display and keyboard sit on the I/O bus because they are outside devices. A stack is part of the CPU/RAM programming model.

Interrupts are larger than a stack. They let an outside event, such as a keyboard key or timer tick, change CPU control flow between instructions. That requires a way to request service, choose a handler address, save the current instruction address, disable or mask further interrupts, acknowledge the source, and return later.

## Proposed Shape

Add an explicit machine profile:

```csharp
public enum MachineProfile
{
    Book,
    Extended
}
```

Add this to `ComputerSettings`, defaulting to `Book`. Expose it through appsettings and CLI flags only when the extended work is ready enough to run.

The book profile should use the existing components:

| Area | Book profile |
| --- | --- |
| CPU | Current `CentralProcessingUnit` behavior |
| Instruction set | Current `InstructionSet` encodings only |
| RAM/bus/wires | Current shared components |
| Peripherals | Current display and polling keyboard adapters |
| Loader | Current raw `.bin` loader |
| Assembler | Current Scott assembler dialect |

The extended profile should reuse the stable lower layers, but choose extended CPU/instruction behavior:

| Area | Extended profile |
| --- | --- |
| CPU | Extended CPU with stack pointer and optional interrupt entry path |
| Instruction set | Book encodings plus stack/interrupt encodings |
| RAM/bus/wires | Same shared components |
| Peripherals | Same adapters by default; optional interrupt-capable adapters later |
| Loader | Same raw `.bin` loader |
| Assembler | Extended dialect behind an explicit option |

## C# Seams To Use

There is already an `ICentralProcessingUnit` interface in `ComputerSimulator.Core/Parts/CentralProcessingUnit.cs`, and `ComputerPart` creates the CPU through `IComponentFactory.CreateCentralProcessingUnit`. Use that seam first.

Suggested minimal path:

1. Preserve the current CPU behavior under a clear book name, either by keeping `CentralProcessingUnit` as the book CPU or by renaming it to `BookCentralProcessingUnit` in one mechanical change.
2. Add `ExtendedCentralProcessingUnit` only when there is real extended behavior to implement.
3. Teach `ComponentFactory.CreateCentralProcessingUnit` to choose the implementation from `ComputerSettings.MachineProfile`.
4. Keep `ComputerPart` mostly unchanged. It should still ask the factory for an `ICentralProcessingUnit` and wire the rest of the machine around the interface.

Avoid making `ComputerPart` own stack or interrupt policy unless the extended CPU needs an extra external component. The book machine should remain easy to read from `ComputerPart`: CPU, RAM, registers, ALU, buses, and adapters.

## Instruction Set Boundary

The current `InstructionSet` is a static encoding helper. Do not silently add extended opcodes to it in a way that makes book-mode callers think the book CPU supports them.

Prefer one of these shapes:

| Shape | When to use |
| --- | --- |
| Keep `InstructionSet` as the book instruction set and add `ExtendedInstructionSet` | Best first step; lowest risk |
| Add an `IInstructionSetDialect` abstraction for assembler/disassembler use | Use if book/extended dialect selection starts spreading |

Book assembler output must remain unchanged. Extended mnemonics such as `PUSH`, `POP`, `CALL`, `RET`, `EI`, `DI`, and `IRET` should be rejected in book mode with a clear diagnostic.

## Stack Plan

Implement stack support in stages.

### Stage 1: Software Convention

Before adding hardware, consider a small `programs/stdlib/stack.asm` convention. It could reserve a RAM location or a register as the stack pointer and provide routines/macros for push/pop-like behavior using existing instructions.

This keeps the book CPU untouched and proves whether programs actually benefit from stack-style structure.

Acceptance criteria:

| Requirement | Expected result |
| --- | --- |
| Book CPU unchanged | Existing tests and binaries still pass |
| Stack helpers are ordinary assembly | No new CPU instructions required |
| Convention is documented | Callers know which registers/RAM locations are clobbered |

### Stage 2: Hardware Stack In Extended Mode

Add a stack pointer only to the extended CPU. The stack pointer can be a 16-bit register wired like the other CPU-owned registers, but controlled by extended CPU control signals.

Likely new pieces:

| Piece | Purpose |
| --- | --- |
| `StackPointerRegister` or ordinary register instance | Holds the current stack address |
| Extended CPU control wires | Enable/set/increment/decrement the stack pointer |
| `PUSH Rn` | Store `Rn` to RAM at `SP`, then move `SP` |
| `POP Rn` | Move `SP`, read RAM at `SP`, store into `Rn` |
| `CALL addr` | Push return address, then jump |
| `RET` | Pop address into the instruction address register |

Decide stack direction explicitly and document it. A downward-growing stack from high RAM is conventional, but any direction is acceptable if tests and docs agree.

Be careful with CPU timing. The current CPU is built around the existing stepper and `WireConstants.ExpectedNumberOfSteps`. Stack operations may need extra steps or may need to be decomposed into existing steps. Do not change the book CPU's timing to make extended instructions convenient.

Acceptance criteria:

| Requirement | Expected result |
| --- | --- |
| Book mode rejects stack opcodes | Book programs cannot accidentally depend on extended ISA |
| Extended mode executes `PUSH`/`POP` | A value round-trips through RAM using `SP` |
| Extended mode executes `CALL`/`RET` | Nested routines return correctly if the stack has capacity |
| Debug views know about `SP` | TUI/debug tooling can show the stack pointer in extended mode |

## Interrupt Plan

Do interrupts after the stack design. Interrupts need a save/restore convention, and a stack is the cleanest place to save the interrupted instruction address and any scratch state.

Likely new pieces:

| Piece | Purpose |
| --- | --- |
| `IInterruptSource` | Interface for adapters/devices that can request service |
| `InterruptController` | Collects pending requests and exposes a vector/handler address |
| `InterruptKeyboardAdapter` | Optional keyboard adapter that raises IRQ when input is available |
| Timer adapter | Useful second interrupt source for tests and OS work |
| Extended CPU interrupt entry path | Checks IRQ at a safe boundary, saves state, jumps to handler |
| `EI`/`DI` | Enable/disable interrupt acceptance |
| `IRET` | Return from an interrupt handler and restore interrupt state |

Suggested flow inside the extended CPU:

```text
At a safe boundary, usually before fetching the next instruction:
  if interrupts are enabled and an interrupt is pending:
    save current instruction address
    disable interrupts
    load handler/vector address into the instruction address register
    acknowledge the interrupt source
  else:
    continue normal fetch/decode/execute
```

Do not retrofit the existing keyboard adapter into an interrupt device by default. Book mode should keep polling: select keyboard address `0x0F`, perform `IN DATA`, receive ASCII or `0`. Extended mode can either wrap the keyboard adapter or use a separate interrupt-capable implementation.

Acceptance criteria:

| Requirement | Expected result |
| --- | --- |
| Book keyboard behavior unchanged | Existing polling echo program still works |
| Extended IRQ can be disabled | `DI` or reset state prevents handler entry |
| Extended IRQ can be enabled | `EI` allows handler entry at a safe boundary |
| Handler returns | `IRET` resumes the interrupted program |
| Tests cover no polling loop | A keyboard or timer event can be handled without repeatedly reading the adapter |

## Configuration And CLI

Use explicit opt-in flags. Do not infer extended mode from the presence of an opcode unless there is a documented file format with metadata.

Possible simulator usage:

```text
dotnet run --project ComputerSimulator -- run program.bin --machine book
dotnet run --project ComputerSimulator -- run program.bin --machine extended
```

Possible assembler usage:

```text
dotnet run --project ComputerSimulator.Assembler.Cli -- program.asm -o program.bin --machine book
dotnet run --project ComputerSimulator.Assembler.Cli -- program.asm -o program.bin --machine extended
```

Default both tools to `book` until there is a strong reason to do otherwise.

## Test Strategy

Add tests in three layers:

| Layer | Tests |
| --- | --- |
| Book regression | Existing CPU, assembler, dogfood, display, and keyboard tests run in book mode |
| Extended CPU | Direct instruction/control tests for `SP`, push/pop, call/return, interrupt entry/return |
| Extended integration | Assemble extended programs, load raw binaries, run the simulator, assert RAM/display/keyboard/timer behavior |

Important regression tests:

| Risk | Test |
| --- | --- |
| Extended opcodes leak into book assembler | Assembling `PUSH R0` in book mode fails clearly |
| Book CPU accidentally changes timing | Existing CPU step tests still pass without extended profile |
| Existing binaries change behavior | Dogfood `.bin` comparison tests still pass |
| Keyboard polling breaks | Echo program still reads `0` when no key is available in book mode |

## Implementation Order

Recommended order for agents:

1. Add `MachineProfile` and wire it through settings/config without changing behavior.
2. Add book/extended selection seams in the factory while both profiles still use the current CPU.
3. Add assembler machine-profile options while both profiles still accept only the book dialect.
4. Add software stack helpers if useful.
5. Add extended stack pointer and `PUSH`/`POP`.
6. Add `CALL`/`RET` once push/pop are proven.
7. Add `InterruptController` and a simple timer or test interrupt source.
8. Add `EI`/`DI`/`IRET` and a handler convention.
9. Add interrupt-capable keyboard behavior only after the core interrupt path is tested.

This order keeps every step reviewable. It also prevents a half-finished interrupt design from forcing changes into the book CPU.

## Non-Goals

Do not start by expanding the number of general-purpose registers, widening immediates, changing the raw program image format, or replacing polling keyboard behavior. Those may be useful later, but they are separate ISA or platform changes. Stack and interrupts are already enough architecture work for one extension path.
