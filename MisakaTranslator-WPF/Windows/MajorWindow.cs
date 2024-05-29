using System.Windows;
using System.Windows.Input;

namespace MisakaTranslator
{
    /// <summary>
    /// 提供打开日志快捷键的窗口基类
    /// </summary>
    public class MajorWindow : Window
    {
        public MajorWindow() : base()
        {
            InputBindings.Add(new KeyBinding()
            {
                Command = new ActionCommand(LogViewer.LogWindow.Show),
                Gesture = new KeyGesture(Key.OemTilde),
            });
        }
        private class ActionCommand(Action action) : ICommand
        {
            public event EventHandler? CanExecuteChanged { add { } remove { } }

            public void Execute(object? parameter)
            {
                action();
            }

            public bool CanExecute(object? parameter)
            {
                return true;
            }
        }
    }
}
