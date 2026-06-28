using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Tui;

if (args.Length > 0 && args[0].Equals("profile", StringComparison.OrdinalIgnoreCase))
{
    ProfileRunner.Run(args[1..]);
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, DefaultConfig.Instance);

public class RenderBenchmarks
{
    private DisplayAdapter _gateDisplay = null!;
    private DisplayAdapter _scanDisplay = null!;
    private SilentDisplayOutput _gateOutput = null!;
    private SilentDisplayOutput _scanOutput = null!;
    private IIoBus _scanIoBus = null!;
    private int _dirtyValue;

    [Params(96, 320)]
    public int Width { get; set; }

    [Params(48, 200)]
    public int Height { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var (componentFactory, wireFactory) = PerformanceFactory.Create(Width, Height);

        _gateDisplay = new DisplayAdapter(
            wireFactory.CreateIoBus("gate-render-benchmark"),
            Width,
            Height,
            DisplayScanMode.GateLevel,
            componentFactory,
            wireFactory);
        _gateOutput = new SilentDisplayOutput();
        _gateOutput.Initialize(Width, Height);

        _scanIoBus = wireFactory.CreateIoBus("scan-render-benchmark");
        _scanDisplay = new DisplayAdapter(
            _scanIoBus,
            Width,
            Height,
            DisplayScanMode.ScanBuffer,
            componentFactory,
            wireFactory);
        _scanOutput = new SilentDisplayOutput();
        _scanOutput.Initialize(Width, Height);
        _scanDisplay.RenderFrame(_scanOutput);
    }

    [Benchmark]
    public void GateLevelFrame()
    {
        _gateDisplay.RenderFrame(_gateOutput);
    }

    [Benchmark]
    public void ScanBufferStaticFrame()
    {
        _scanDisplay.RenderFrame(_scanOutput);
    }

    [Benchmark]
    public void ScanBufferDirtyByteFrame()
    {
        _dirtyValue ^= 0xFF;
        DisplayIo.WriteByte(_scanIoBus, _scanDisplay, 0, _dirtyValue);
        _scanDisplay.RenderFrame(_scanOutput);
    }
}

public class TerminalBufferBenchmarks
{
    private TerminalDisplayBuffer _buffer = null!;

    [Params(96, 320)]
    public int Width { get; set; }

    [Params(48, 200)]
    public int Height { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _buffer = new TerminalDisplayBuffer();
        _buffer.Initialize(Width, Height);
    }

    [Benchmark(Baseline = true)]
    public void SetPixelsIndividually()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _buffer.SetPixel(x, y, ((x + y) & 1) == 0);
            }
        }
    }

    [Benchmark]
    public void SetPixelsByByte()
    {
        var bytesPerRow = Width / 8;
        for (var y = 0; y < Height; y++)
        {
            for (var byteColumn = 0; byteColumn < bytesPerRow; byteColumn++)
            {
                _buffer.SetPixelByte(byteColumn * 8, y, 0b1010_1010);
            }
        }
    }

    [Benchmark]
    public TerminalDisplaySnapshot Snapshot()
    {
        return _buffer.GetSnapshot();
    }

    [Benchmark]
    public IReadOnlyList<string> RenderBrailleSnapshot()
    {
        return TerminalFrameRenderer.Render(_buffer.GetSnapshot(), ComputerSimulator.Graphics.TerminalPixelMode.Braille, Width, Height);
    }
}

public class SimulationBenchmarks
{
    private IComputerPart _computerPart = null!;

    [GlobalSetup]
    public void Setup()
    {
        var (componentFactory, _) = PerformanceFactory.Create(96, 48);
        _computerPart = componentFactory.CreateComputerPart();

        var program = DemoProgram.Build(96, 48);
        for (var address = 0; address < program.Count; address++)
        {
            _computerPart.Ram
                .GetSlot(address & 0xFF, address >> 8)
                .Memory
                .SetRegisterValue(program[address]);
        }
    }

    [Benchmark]
    public void ComputerPartUpdates1000()
    {
        for (var i = 0; i < 1_000; i++)
        {
            _computerPart.Update();
        }
    }
}

internal static class ProfileRunner
{
    public static void Run(string[] args)
    {
        var options = ProfileOptions.Parse(args);

        Console.WriteLine($"Profile run: {options.Width}x{options.Height}, iterations={options.Iterations}, cpuUpdates={options.CpuUpdates:N0}, cpuUpdatesPerFrame={options.CpuUpdatesPerFrame:N0}");

        var (componentFactory, wireFactory) = PerformanceFactory.Create(options.Width, options.Height);

        Measure("render.gate.initial", options.Iterations, () =>
        {
            var display = new DisplayAdapter(
                wireFactory.CreateIoBus("profile-gate-initial"),
                options.Width,
                options.Height,
                DisplayScanMode.GateLevel,
                componentFactory,
                wireFactory);
            var output = new SilentDisplayOutput();
            output.Initialize(options.Width, options.Height);
            display.RenderFrame(output);
        });

        Measure("render.scan-buffer.initial", options.Iterations, () =>
        {
            var display = new DisplayAdapter(
                wireFactory.CreateIoBus("profile-scan-initial"),
                options.Width,
                options.Height,
                DisplayScanMode.ScanBuffer,
                componentFactory,
                wireFactory);
            var output = new SilentDisplayOutput();
            output.Initialize(options.Width, options.Height);
            display.RenderFrame(output);
        });

        var scanIoBus = wireFactory.CreateIoBus("profile-scan-dirty");
        var scanDisplay = new DisplayAdapter(
            scanIoBus,
            options.Width,
            options.Height,
            DisplayScanMode.ScanBuffer,
            componentFactory,
            wireFactory);
        var scanOutput = new SilentDisplayOutput();
        scanOutput.Initialize(options.Width, options.Height);
        scanDisplay.RenderFrame(scanOutput);

        var dirtyValue = 0;
        Measure("render.scan-buffer.dirty-byte", options.Iterations, () =>
        {
            dirtyValue ^= 0xFF;
            DisplayIo.WriteByte(scanIoBus, scanDisplay, 0, dirtyValue);
            scanDisplay.RenderFrame(scanOutput);
        });

        Measure("render.scan-buffer.static", options.Iterations, () => scanDisplay.RenderFrame(scanOutput));

        var buffer = new TerminalDisplayBuffer();
        buffer.Initialize(options.Width, options.Height);
        Measure("terminal-buffer.set-pixel", options.Iterations, () =>
        {
            for (var y = 0; y < options.Height; y++)
            {
                for (var x = 0; x < options.Width; x++)
                {
                    buffer.SetPixel(x, y, ((x + y) & 1) == 0);
                }
            }
        });

        Measure("terminal-buffer.set-byte", options.Iterations, () =>
        {
            var bytesPerRow = options.Width / 8;
            for (var y = 0; y < options.Height; y++)
            {
                for (var byteColumn = 0; byteColumn < bytesPerRow; byteColumn++)
                {
                    buffer.SetPixelByte(byteColumn * 8, y, 0b1010_1010);
                }
            }
        });

        Measure("terminal-frame.braille", options.Iterations, () =>
            TerminalFrameRenderer.Render(buffer.GetSnapshot(), ComputerSimulator.Graphics.TerminalPixelMode.Braille, options.Width, options.Height));

        MeasureComputerLoop("loop.gate.silent-output", DisplayScanMode.GateLevel, options, static () => new SilentDisplayOutput());
        MeasureComputerLoop("loop.scan-buffer.silent-output", DisplayScanMode.ScanBuffer, options, static () => new SilentDisplayOutput());
        MeasureComputerLoop("loop.gate.tui-buffer-output", DisplayScanMode.GateLevel, options, static () =>
            new TerminalGuiDisplayOutput(new TerminalDisplayBuffer(), new NoopTerminalGuiApplication()));
        MeasureComputerLoop("loop.scan-buffer.tui-buffer-output", DisplayScanMode.ScanBuffer, options, static () =>
            new TerminalGuiDisplayOutput(new TerminalDisplayBuffer(), new NoopTerminalGuiApplication()));

        var computerPart = componentFactory.CreateComputerPart();
        LoadProgram(computerPart, DemoProgram.Build(options.Width, options.Height));
        Measure("simulation.computer-part-update", 1, () =>
        {
            for (var i = 0; i < options.CpuUpdates; i++)
            {
                computerPart.Update();
            }
        });
    }

    private static void MeasureComputerLoop(
        string name,
        DisplayScanMode scanMode,
        ProfileOptions options,
        Func<IDisplayOutput> outputFactory)
    {
        var settings = new ComputerSettings
        {
            WordSize = 16,
            ScreenWidth = options.Width,
            ScreenHeight = options.Height,
            DisplayScanMode = scanMode
        };
        var wireFactory = new WireFactory(settings);
        var componentFactory = new ComponentFactory(
            wireFactory,
            new LeftShifterWireFactory(wireFactory),
            new RightShifterWireFactory(wireFactory),
            settings);
        var computerPart = componentFactory.CreateComputerPart();
        var display = componentFactory.CreateDisplayAdapter(computerPart.IoBus);
        computerPart.IoBus.ConnectedComponents.Add(display);
        var output = outputFactory();
        output.Initialize(display.Width, display.Height);
        LoadProgram(computerPart, DemoProgram.Build(options.Width, options.Height));

        Measure(name, options.Iterations, () =>
        {
            for (var update = 0; update < options.CpuUpdatesPerFrame; update++)
            {
                computerPart.Update();
            }

            display.RenderFrame(output);
        });
    }

    private static void Measure(string name, int iterations, Action action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);
        var started = System.Diagnostics.Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            action();
        }

        started.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        Console.WriteLine(
            $"{name}: total={started.Elapsed.TotalMilliseconds:N2} ms, avg={started.Elapsed.TotalMilliseconds / iterations:N2} ms, alloc={allocated:N0} B, gen0={GC.CollectionCount(0) - gen0Before}, gen1={GC.CollectionCount(1) - gen1Before}, gen2={GC.CollectionCount(2) - gen2Before}");
    }

    private static void LoadProgram(IComputerPart computerPart, IReadOnlyList<bool[]> program)
    {
        for (var address = 0; address < program.Count; address++)
        {
            computerPart.Ram
                .GetSlot(address & 0xFF, address >> 8)
                .Memory
                .SetRegisterValue(program[address]);
        }
    }
}

internal readonly record struct ProfileOptions(int Width, int Height, int Iterations, int CpuUpdates, int CpuUpdatesPerFrame)
{
    public static ProfileOptions Parse(string[] args)
    {
        var width = 96;
        var height = 48;
        var iterations = 5;
        var cpuUpdates = 100_000;
        var cpuUpdatesPerFrame = 400;

        for (var i = 0; i < args.Length; i++)
        {
            if (i + 1 >= args.Length)
            {
                break;
            }

            switch (args[i])
            {
                case "--width":
                    width = int.Parse(args[++i]);
                    break;
                case "--height":
                    height = int.Parse(args[++i]);
                    break;
                case "--iterations":
                    iterations = int.Parse(args[++i]);
                    break;
                case "--cpu-updates":
                    cpuUpdates = int.Parse(args[++i]);
                    break;
                case "--cpu-updates-per-frame":
                    cpuUpdatesPerFrame = int.Parse(args[++i]);
                    break;
            }
        }

        return new ProfileOptions(width, height, iterations, cpuUpdates, cpuUpdatesPerFrame);
    }
}

internal static class PerformanceFactory
{
    public static (IComponentFactory ComponentFactory, IWireFactory WireFactory) Create(int width, int height)
    {
        var settings = new ComputerSettings
        {
            WordSize = 16,
            ScreenWidth = width,
            ScreenHeight = height,
            DisplayScanMode = DisplayScanMode.ScanBuffer
        };
        settings.Validate();

        var wireFactory = new WireFactory(settings);
        var componentFactory = new ComponentFactory(
            wireFactory,
            new LeftShifterWireFactory(wireFactory),
            new RightShifterWireFactory(wireFactory),
            settings);

        return (componentFactory, wireFactory);
    }
}

internal static class DisplayIo
{
    public static void WriteByte(IIoBus ioBus, IDisplayAdapter displayAdapter, int address, int value)
    {
        OutAddress(ioBus, displayAdapter, (int)IoAddress.Display);
        OutAddress(ioBus, displayAdapter, address);
        OutData(ioBus, displayAdapter, value);
    }

    private static void OutAddress(IIoBus ioBus, IDisplayAdapter displayAdapter, int value)
    {
        ioBus.CpuBus.SetValue(value);
        ioBus.DataAddress.Value = true;
        ioBus.InputOutput.Value = true;
        ioBus.Clk.Set.Value = true;
        displayAdapter.Update();
        ClearStrobe(ioBus, displayAdapter);
    }

    private static void OutData(IIoBus ioBus, IDisplayAdapter displayAdapter, int value)
    {
        ioBus.CpuBus.SetValue(value);
        ioBus.DataAddress.Value = false;
        ioBus.InputOutput.Value = true;
        ioBus.Clk.Set.Value = true;
        displayAdapter.Update();
        ClearStrobe(ioBus, displayAdapter);
    }

    private static void ClearStrobe(IIoBus ioBus, IDisplayAdapter displayAdapter)
    {
        ioBus.Clk.Set.Value = false;
        displayAdapter.Update();
    }
}

internal sealed class SilentDisplayOutput : IDisplayByteOutput
{
    public int SetPixelCalls { get; private set; }

    public int SetPixelByteCalls { get; private set; }

    public int Presents { get; private set; }

    public void Initialize(int width, int height)
    {
    }

    public void SetPixel(int x, int y, bool on)
    {
        SetPixelCalls++;
    }

    public void SetPixelByte(int x, int y, int value)
    {
        SetPixelByteCalls++;
    }

    public void Present()
    {
        Presents++;
    }
}

internal sealed class NoopTerminalGuiApplication : ITerminalGuiApplication
{
    public void Attach(Terminal.Gui.App.IApplication application, ComputerSimulatorWindow window)
    {
    }

    public void Detach()
    {
    }

    public void RefreshDisplay()
    {
    }

    public void RefreshLogs()
    {
    }
}
