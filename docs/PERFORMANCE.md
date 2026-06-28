# Performance Profiling

The simulator remains gate/circuit faithful. Optimizations should remove host-language overhead around the simulation, not replace simulated registers, adders, decoders, or gates with direct logical shortcuts.

## Benchmarks

Run all benchmark suites:

```bash
dotnet run -c Release --project ComputerSimulator.Performance -- --filter '*'
```

Run targeted benchmark suites:

```bash
dotnet run -c Release --project ComputerSimulator.Performance -- --filter '*RenderBenchmarks*'
dotnet run -c Release --project ComputerSimulator.Performance -- --filter '*TerminalBufferBenchmarks*'
dotnet run -c Release --project ComputerSimulator.Performance -- --filter '*SimulationBenchmarks*'
```

## Finite Profile Run

The `profile` mode emits per-stage timings and allocation counts and exits, which makes it suitable for dotTrace:

```bash
dotnet run -c Release --project ComputerSimulator.Performance -- profile --width 320 --height 200 --iterations 5 --cpu-updates 100000
```

The profile mode also includes combined CPU+render loop timings for both silent output and TUI-buffer output. These are closer to the live application than render-only timings because they include the configured CPU updates performed before each frame.

## Live TUI Stats

Enable runtime stats in the TUI to separate CPU simulation time from display render time:

```bash
dotnet run --project ComputerSimulator -- --scan-mode buffer --perf-stats true --perf-stats-interval 2
dotnet run --project ComputerSimulator -- --scan-mode gate --perf-stats true --perf-stats-interval 2
```

For unthrottled comparisons, disable the configured frame delay:

```bash
dotnet run --project ComputerSimulator -- --scan-mode buffer --frame-delay-ms 0 --perf-stats true
```

If apparent screen-fill speed is similar between scan modes, check the reported CPU update time. The demo program writes display bytes through the simulated CPU and IO bus, so scan-buffer mode only accelerates RAM-to-terminal refresh; it does not make the CPU draw into display RAM faster.

## dotTrace

Install the command-line profiler if needed:

```bash
dotnet tool install --global JetBrains.dotTrace.GlobalTools
```

Capture a sampling snapshot:

```bash
dotnet publish ComputerSimulator.Performance -c Release -o artifacts/perf
dottrace start --framework=NetCore --profiling-type=Sampling --overwrite --save-to=artifacts/profiles/render.dtp /opt/homebrew/bin/dotnet artifacts/perf/ComputerSimulator.Performance.dll profile -- --width 320 --height 200 --iterations 5 --cpu-updates 100000
```

Use `which dotnet` if your `dotnet` executable is not `/opt/homebrew/bin/dotnet`. The `--` before `--width` is required so dotTrace passes the remaining options to the profiled app instead of parsing them as profiler options.

Open the `.dtp` in dotTrace or Rider and inspect hot methods under `DisplayAdapter.RenderFrame`, `ScreenControl.Update`, `TerminalDisplayBuffer`, `TerminalFrameRenderer`, and `ComputerPart.Update`.

## dotMemory

JetBrains does not publish a `JetBrains.dotMemory.GlobalTools` package. Use the standalone dotMemory UI for snapshots, or JetBrains' command-line dotMemory package if installed separately. The finite profile command above is the target process to run under dotMemory.
