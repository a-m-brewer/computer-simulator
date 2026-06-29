using ComputerSimulator.Graphics;
using ComputerSimulator.Core.Peripherals.Keyboard;
using Microsoft.Extensions.Hosting;
using Terminal.Gui.App;

namespace ComputerSimulator.Tui;

public class TerminalGuiService : IHostedService
{
    private readonly TerminalDisplayBuffer _displayBuffer;
    private readonly TerminalSettings _settings;
    private readonly ITerminalLogSink _logSink;
    private readonly ITerminalGuiApplication _terminalGui;
    private readonly IKeyboardInput _keyboardInput;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly CancellationTokenSource _cts = new();
    private Task? _runTask;

    public TerminalGuiService(
        TerminalDisplayBuffer displayBuffer,
        TerminalSettings settings,
        ITerminalLogSink logSink,
        ITerminalGuiApplication terminalGui,
        IKeyboardInput keyboardInput,
        IHostApplicationLifetime applicationLifetime)
    {
        _displayBuffer = displayBuffer;
        _settings = settings;
        _logSink = logSink;
        _terminalGui = terminalGui;
        _keyboardInput = keyboardInput;
        _applicationLifetime = applicationLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logSink.Changed += OnLogsChanged;
        _runTask = Task.Run(RunAsync, cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logSink.Changed -= OnLogsChanged;
        await _cts.CancelAsync();

        if (_runTask is not null)
        {
            await Task.WhenAny(_runTask, Task.Delay(System.Threading.Timeout.Infinite, cancellationToken));
        }
    }

    private async Task RunAsync()
    {
        try
        {
            using var app = Application.Create().Init();
            using var window = new ComputerSimulatorWindow(_displayBuffer, _settings, _logSink);

            app.Keyboard.KeyDown += (_, key) => TerminalKeyboardInput.PushMappedKey(_keyboardInput, key);
            _terminalGui.Attach(app, window);
            await app.RunAsync(window, _cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _terminalGui.Detach();
            _applicationLifetime.StopApplication();
        }
    }

    private void OnLogsChanged()
    {
        _terminalGui.RefreshLogs();
    }
}
