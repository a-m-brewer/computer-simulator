using Terminal.Gui.App;

namespace ComputerSimulator.Tui;

public interface ITerminalGuiApplication
{
    void Attach(IApplication application, ComputerSimulatorWindow window);

    void Detach();

    void RefreshDisplay();

    void RefreshLogs();
}

public class TerminalGuiApplication : ITerminalGuiApplication
{
    private readonly Lock _sync = new();
    private IApplication? _application;
    private ComputerSimulatorWindow? _window;
    private bool _displayRefreshPending;
    private bool _logRefreshPending;

    public void Attach(IApplication application, ComputerSimulatorWindow window)
    {
        lock (_sync)
        {
            _application = application;
            _window = window;
        }
    }

    public void Detach()
    {
        lock (_sync)
        {
            _application = null;
            _window = null;
        }
    }

    public void RefreshDisplay()
    {
        InvokeCoalesced(true, window => window.RefreshDisplay());
    }

    public void RefreshLogs()
    {
        InvokeCoalesced(false, window => window.RefreshLogs());
    }

    private void InvokeCoalesced(bool displayRefresh, Action<ComputerSimulatorWindow> action)
    {
        IApplication? application;
        ComputerSimulatorWindow? window;
        lock (_sync)
        {
            ref var pending = ref displayRefresh ? ref _displayRefreshPending : ref _logRefreshPending;
            if (pending)
            {
                return;
            }

            application = _application;
            window = _window;
            pending = application is not null && window is not null;
        }

        if (application is null || window is null)
        {
            return;
        }

        application.Invoke(() =>
        {
            lock (_sync)
            {
                if (displayRefresh)
                {
                    _displayRefreshPending = false;
                }
                else
                {
                    _logRefreshPending = false;
                }
            }

            action(window);
        });
    }
}
