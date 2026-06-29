# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestName"

# Run tests in a specific project
dotnet test ComputerSimulator.Core.Tests
dotnet test ComputerSimulator.IntegrationTests
dotnet test ComputerSimulator.Assembler.Tests

# Run the simulator
dotnet run --project ComputerSimulator

# Assemble a program, then run it
dotnet run --project ComputerSimulator.Assembler.Cli -- programs/display-pattern.asm -o display-pattern.bin -D BYTES_PER_FRAME=576
dotnet run --project ComputerSimulator -- run display-pattern.bin
```

## Roadmap

See [ROADMAP.md](docs/ROADMAP.md) for a list of planned features and improvements.

## Architecture

This is a bottom-up hardware computer simulation in C# (.NET 10). Every logical component is modelled as software, from individual gates up to a full CPU.

### Layered simulation model

```
Gates       → And, Or, Not, NAnd, XOr (ComputerSimulator.Core/Gates/)
Circuits    → Word, Register, WordAdder, Decoder, Shifter, etc. (Core/Circuits/)
Parts       → CPU, ALU, RAM, Bus, Clock, Stepper, IoBus (Core/Parts/)
Peripherals → DisplayAdapter, IoBusControl, ScreenControl, DisplayRam (Core/Peripherals/)
Computer    → IComputer / Computer (Core/Computer.cs)
Host        → BackgroundService (ComputerSimulator/ComputerService.cs)
Assembler   → .asm parser/emitter + CLI (ComputerSimulator.Assembler/, ComputerSimulator.Assembler.Cli/)
```

### Update propagation

There is **no event system**. Every component implements `IComponent` with a single `Update()` method. The caller is responsible for calling `Update()` on each component in the correct dependency order each clock tick. `PartsBase` / `CircuitBase` compose components and wire them by passing `IWire<T>` / `IWireGroup<T>` instances through constructors — a wire output from one component is the input wire of the next.

### Wires and buses

- `IWire<T>` — carries a single value (almost always `bool`)
- `IWireGroup<T>` — ordered collection of wires (word-sized; default 16 bits via `ComputerSettings.WordSize`)
- `IBus` — a `IWireGroup<bool>` shared across many components (the main data highway)
- `ISetEnableWire<T>` — a pair of wires (Set / Enable) used for register control signals
- All wires are created via `IWireFactory`; all components via `IComponentFactory`

### Instruction set

Instructions are 8 bits. The upper 4 bits are an `InstructionPrefix`, lower 4 bits encode operands.

Currently defined prefixes (see `Core/Enums/InstructionPrefix.cs` and `Core/Instructions/`):
- `0111` — IO instruction (`IoInstruction`): bits encode InputOutput mode, DataAddress, and RegisterB

ALU operations are defined in `OpCode` (Add, Shr, Shl, Not, And, Or, XOr, Cmp).

### Assembler

The assembler is a separate toolchain from the simulator. `ComputerSimulator.Assembler` parses `.asm` files and emits raw `.bin` images; `ComputerSimulator.Assembler.Cli` is the thin command-line wrapper. The simulator still only loads bytes through `ProgramLoader` and should not learn assembly syntax.

Key assembler conventions:

- Real instruction bytes must go through `ComputerSimulator.Core.Instructions.InstructionSet`; do not duplicate opcode constants in the assembler.
- Mutating ALU syntax is destination-first, e.g. `ADD R3, R0` means `R3 = R3 + R0`, even though the book/hardware encoding is `RA,RB` with `RB` as the destination.
- `CMP` has no destination and preserves written operand order, e.g. `CMP R3, R2` maps to compare A=`R3`, B=`R2`.
- `JMP` and conditional jumps are real short jumps with one-byte targets. Use explicit-register `JMP16 label, Raddr, Rtmp` for long software jumps.
- `LDI` is optimized: one-byte values emit `DATA`; word values require an explicit scratch register and expand to `DATA`/`SHL`/`ADD`.
- Labels are case-sensitive; mnemonics and registers are case-insensitive.
- `.org` zero-fills gaps in the raw image. `.incbin` is the path for custom fonts/assets without simulator code changes.
- See `docs/ASSEMBLY.md` for syntax details and `programs/display-pattern.asm` for the current dogfood program.

### CPU control signals

`CentralProcessingUnit` wires together all control signals using a 7-step clock (see `WireConstants.ExpectedNumberOfSteps = 7`). Steps gate which control lines fire — e.g. step 1 is fetch, step 2 is decode, etc. The stepper advances each clock tick.

### IO Bus and peripherals

`IoBus` connects the CPU to peripherals via `InputOutput`, `DataAddress`, and `Clk` wires alongside the main `CpuBus`. Peripherals implement `IAdapter` (extends `IComponent`) and connect to the bus. Only the display adapter (`DisplayAdapter`) currently exists.

### Dependency injection

`ServiceCollectionExtensions.RegisterCoreServices` registers everything. It uses **Scrutor** to scan the `ComputerSimulator.Core` assembly and auto-register all `IComponent` implementations as transient. `IWireFactory` is singleton (shares wire instances across the simulation); `IComponentFactory` is transient.

### Testing

- **Unit tests** (`ComputerSimulator.Core.Tests`): use NUnit + Moq + FluentAssertions. Extend `MockBase<T>` from `ComputerSimulator.TestUtilities` — it provides a `GetMock<TDep>()` helper. Wires are mocked via `Mock.Of<IWire<bool>>`.
- **Assembler tests** (`ComputerSimulator.Assembler.Tests`): use NUnit + FluentAssertions. Cover parser/emitter behavior, pseudo-instruction expansion, `.include`/`.incbin`, CLI smoke tests, and byte output against `InstructionSet`.
- **Integration tests** (`ComputerSimulator.IntegrationTests`): extend `IntegrationTestBase`, which boots the full DI host via `HostTestBase`. Use `CreateTestWire`, `CreateTestWireGroup`, `CreateTestBus`, etc. for real wire instances.
- **Dogfood assembler tests** live in `ComputerSimulator.IntegrationTests/Assembler/` and should assemble `.asm`, load the emitted bytes with `ProgramLoader`, and verify simulated behavior.

### Coding standards

- Prefer `System.Collections.Concurrent` types over lock objects for thread safety.
- Prefer collection expressions when creating collections. e.g. [] not `new List<T>()`
