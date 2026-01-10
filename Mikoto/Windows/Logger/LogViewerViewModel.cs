using Mikoto.Helpers.ViewModel;
using Serilog.Events;
using System.Collections.ObjectModel;
namespace Mikoto.Windows.Logger;

internal class LogViewerViewModel : ViewModelBase
{
    // 线程安全的集合
    public ObservableCollection<LogEntry> Logs { get; } = new();

    private string _logCountText = "0 Items";

    private int _currentIndex = 0;
    public string LogCountText
    {
        get => _logCountText;
        set => SetProperty(ref _logCountText, value);
    }
    public void AddLog(string message, LogEventLevel level)
    {
        var entry = new LogEntry(
          Interlocked.Increment(ref _currentIndex),
          DateTime.Now,
          message,
          level
        );

        // 确保在 UI 线程操作集合
        App.Current.Dispatcher.Invoke(() =>
        {
            Logs.Add(entry);
            if (Logs.Count > 1000)
            {
                // 批量删除旧日志
                for (int i = 0; i < 100; i++) Logs.RemoveAt(0);
            }
            LogCountText = $"{Logs.Count} Items";
        });
    }
}