using Serilog.Events;
using System.Windows;
using System.Windows.Media;

namespace Mikoto.Windows.Logger;

internal class LogViewerController
{
    internal static void Sink(LogEvent logEvent)
    {
        var brush = GetBrush(logEvent.Level);
        var message = logEvent.RenderMessage();

        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            LogViewer.LogWindow.LogEntries.Add(new LogEntry
            {
                Message = message,
                Color = brush
            });
        });
    }

    private static SolidColorBrush GetBrush(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => Brushes.Gray,
            LogEventLevel.Debug => Brushes.LightGray,
            LogEventLevel.Information => Brushes.Green,
            LogEventLevel.Warning => Brushes.Yellow,
            LogEventLevel.Error => Brushes.Red,
            LogEventLevel.Fatal => Brushes.DarkRed,
            _ => Brushes.Black
        };
    }
}
