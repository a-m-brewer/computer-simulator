using ComputerSimulator.Graphics;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ComputerSimulator.Tui;

public class ComputerSimulatorWindow : Window
{
    public ComputerSimulatorWindow(TerminalDisplayBuffer displayBuffer, TerminalSettings settings, ITerminalLogSink logSink)
    {
        Title = "Computer Simulator";

        DisplayFrame = new FrameView
        {
            Title = "Display",
            X = 0,
            Y = 0,
            Width = Dim.Percent(75),
            Height = Dim.Percent(70)
        };

        DisplayView = new TerminalDisplayView(displayBuffer, settings)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        DisplayFrame.Add(DisplayView);

        LogFrame = new FrameView
        {
            Title = "Logs",
            X = 0,
            Y = Pos.Percent(70),
            Width = Dim.Percent(75),
            Height = Dim.Fill()
        };

        LogView = new TerminalLogView(logSink)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        LogFrame.Add(LogView);

        RightFrame = new FrameView
        {
            X = Pos.Percent(75),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        Add(DisplayFrame, LogFrame, RightFrame);
    }

    public FrameView DisplayFrame { get; }

    public FrameView LogFrame { get; }

    public FrameView RightFrame { get; }

    public TerminalDisplayView DisplayView { get; }

    public TerminalLogView LogView { get; }

    public void RefreshDisplay()
    {
        DisplayView.Refresh();
    }

    public void RefreshLogs()
    {
        LogView.Refresh();
    }
}
