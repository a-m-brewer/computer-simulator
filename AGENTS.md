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

# Run the simulator
dotnet run --project ComputerSimulator
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

### CPU control signals

`CentralProcessingUnit` wires together all control signals using a 7-step clock (see `WireConstants.ExpectedNumberOfSteps = 7`). Steps gate which control lines fire — e.g. step 1 is fetch, step 2 is decode, etc. The stepper advances each clock tick.

### IO Bus and peripherals

`IoBus` connects the CPU to peripherals via `InputOutput`, `DataAddress`, and `Clk` wires alongside the main `CpuBus`. Peripherals implement `IAdapter` (extends `IComponent`) and connect to the bus. Only the display adapter (`DisplayAdapter`) currently exists.

### Dependency injection

`ServiceCollectionExtensions.RegisterCoreServices` registers everything. It uses **Scrutor** to scan the `ComputerSimulator.Core` assembly and auto-register all `IComponent` implementations as transient. `IWireFactory` is singleton (shares wire instances across the simulation); `IComponentFactory` is transient.

### Testing

- **Unit tests** (`ComputerSimulator.Core.Tests`): use NUnit + Moq + FluentAssertions. Extend `MockBase<T>` from `ComputerSimulator.TestUtilities` — it provides a `GetMock<TDep>()` helper. Wires are mocked via `Mock.Of<IWire<bool>>`.
- **Integration tests** (`ComputerSimulator.IntegrationTests`): extend `IntegrationTestBase`, which boots the full DI host via `HostTestBase`. Use `CreateTestWire`, `CreateTestWireGroup`, `CreateTestBus`, etc. for real wire instances.

### Coding standards

- Prefer `System.Collections.Concurrent` types over lock objects for thread safety.
- Prefer collection expressions when creating collections. e.g. [] not `new List<T>()`