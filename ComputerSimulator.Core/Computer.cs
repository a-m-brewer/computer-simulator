using System.Diagnostics;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Core.Programs;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class Computer : IComputer
{
    private readonly IDisplayOutput _output;
    private readonly IComputerPart _computerPart;
    private readonly IDisplayAdapter _display;
    private readonly ComputerSettings _settings;
    private readonly ILogger<Computer> _logger;

    public Computer(IComponentFactory componentFactory, IDisplayOutput output, ComputerSettings settings, ILogger<Computer> logger)
    {
        _output = output;
        _settings = settings;
        _logger = logger;
        _computerPart = componentFactory.CreateComputerPart();
        _display = componentFactory.CreateDisplayAdapter(_computerPart.IoBus);
        _computerPart.IoBus.ConnectedComponents.Add(_display);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting computer simulation with settings: {Settings}", _settings);

        _output.Initialize(_display.Width, _display.Height);
        LoadProgram();

        var updates = 0;
        var stats = new RuntimeStats(_settings.PerformanceStatsIntervalSeconds);
        while (!cancellationToken.IsCancellationRequested)
        {
            var cpuStarted = Stopwatch.GetTimestamp();
            _computerPart.Update();
            stats.AddCpuUpdate(cpuStarted);

            if (++updates % _settings.CpuUpdatesPerFrame != 0)
            {
                LogStatsIfDue(stats);
                continue;
            }

            var renderStarted = Stopwatch.GetTimestamp();
            _display.RenderFrame(_output);
            stats.AddRenderFrame(renderStarted);

            LogStatsIfDue(stats);
            if (_settings.DisplayFrameDelayMs > 0)
            {
                await Task.Delay(_settings.DisplayFrameDelayMs, cancellationToken);
            }
        }
    }

    private void LogStatsIfDue(RuntimeStats stats)
    {
        if (!_settings.EnablePerformanceStats || !stats.TryTakeSnapshot(out var snapshot))
        {
            return;
        }

        _logger.LogInformation(
            "Perf: {CpuUpdatesPerSecond:N0} cpu/s, {FramesPerSecond:N1} frames/s, avg cpu {AverageCpuUpdateMilliseconds:N3} ms/update, avg render {AverageRenderMilliseconds:N3} ms/frame",
            snapshot.CpuUpdatesPerSecond,
            snapshot.FramesPerSecond,
            snapshot.AverageCpuUpdateMilliseconds,
            snapshot.AverageRenderMilliseconds);
    }

    private void LoadProgram()
    {
        if (!string.IsNullOrWhiteSpace(_settings.ProgramPath))
        {
            var image = ProgramLoader.ReadBinaryImage(_settings.ProgramPath);
            ProgramLoader.Load(_computerPart.Ram, image);
            _logger.LogInformation("Loaded {ProgramByteCount} bytes from {ProgramPath}", image.Length, _settings.ProgramPath);
            return;
        }

        if (_settings.BuiltInProgram == BuiltInProgram.HelloWorld)
        {
            ProgramLoader.Load(_computerPart.Ram, TextProgram.BuildHelloWorldImage(_display.Width, _display.Height));
            return;
        }

        ProgramLoader.Load(_computerPart.Ram, DemoProgram.Build(_display.Width, _display.Height));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private sealed class RuntimeStats
    {
        private readonly long _intervalTicks;
        private long _windowStarted = Stopwatch.GetTimestamp();
        private long _cpuTicks;
        private long _renderTicks;
        private int _cpuUpdates;
        private int _frames;

        public RuntimeStats(int intervalSeconds)
        {
            _intervalTicks = intervalSeconds * Stopwatch.Frequency;
        }

        public void AddCpuUpdate(long started)
        {
            _cpuUpdates++;
            _cpuTicks += Stopwatch.GetTimestamp() - started;
        }

        public void AddRenderFrame(long started)
        {
            _frames++;
            _renderTicks += Stopwatch.GetTimestamp() - started;
        }

        public bool TryTakeSnapshot(out RuntimeStatsSnapshot snapshot)
        {
            var now = Stopwatch.GetTimestamp();
            var elapsedTicks = now - _windowStarted;
            if (elapsedTicks < _intervalTicks)
            {
                snapshot = default;
                return false;
            }

            var elapsedSeconds = (double)elapsedTicks / Stopwatch.Frequency;
            snapshot = new RuntimeStatsSnapshot(
                _cpuUpdates / elapsedSeconds,
                _frames / elapsedSeconds,
                ToMilliseconds(_cpuTicks, _cpuUpdates),
                ToMilliseconds(_renderTicks, _frames));

            _windowStarted = now;
            _cpuTicks = 0;
            _renderTicks = 0;
            _cpuUpdates = 0;
            _frames = 0;
            return true;
        }

        private static double ToMilliseconds(long ticks, int count)
        {
            return count == 0 ? 0 : (ticks * 1000.0) / Stopwatch.Frequency / count;
        }
    }

    private readonly record struct RuntimeStatsSnapshot(
        double CpuUpdatesPerSecond,
        double FramesPerSecond,
        double AverageCpuUpdateMilliseconds,
        double AverageRenderMilliseconds);
}
