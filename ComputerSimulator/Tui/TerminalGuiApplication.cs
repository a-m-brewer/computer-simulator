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
    private readonly object _sync = new();
    private IApplication? _application;
    private ComputerSimulatorWindow? _window;

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
        Invoke(window => window.RefreshDisplay());
    }

    public void RefreshLogs()
    {
        Invoke(window => window.RefreshLogs());
    }

    private void Invoke(Action<ComputerSimulatorWindow> action)
    {
        IApplication? application;
        ComputerSimulatorWindow? window;
        lock (_sync)
        {
            application = _application;
            window = _window;
        }

        if (application is null || window is null)
        {
            return;
        }

        application.Invoke(() => action(window));
    }
}
