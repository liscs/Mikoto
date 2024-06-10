using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;

namespace MisakaTranslator
{
    /// <summary>
    /// 提供打开日志快捷键的窗口基类
    /// </summary>
    public partial class MajorWindow : Window
    {

        private static unsafe int SetWindowAttribute(IntPtr hwnd, Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE attribute, int parameter)
        {
            return PInvoke.DwmSetWindowAttribute((Windows.Win32.Foundation.HWND)hwnd,
                attribute,
                (void*)parameter,
                (uint)Marshal.SizeOf<int>());
        }
        public MajorWindow() : base()
        {
            if (this is not LogViewer)
            {
                InputBindings.Add(new KeyBinding()
                {
                    Command = new ActionCommand(LogViewer.LogWindow.Show),
                    Gesture = new KeyGesture(Key.OemTilde),
                });
            }
            //this.Style = (Style)Application.Current.Resources["MicaWindowStyle"];
            //Loaded += MajorWindow_Loaded;
        }

        private void MajorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Content is FrameworkElement frameworkElement)
            {
                frameworkElement.Margin = new Thickness(0, 32, 0, 0);
            }
            // Apply Mica brush
            SetWindowAttribute(
                new WindowInteropHelper(this).Handle,
                Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                2);
            Background = Brushes.Transparent;
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
