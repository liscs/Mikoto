using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MisakaTranslator.Styles
{
    public partial class WindowStyle : ResourceDictionary
    {
        public WindowStyle()
        {
            InitializeComponent();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void MaximizeRestoreClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            if (window.WindowState == WindowState.Normal)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        private Border? _mainBorder;
        private void InitMainBorder(object? sender, EventArgs e)
        {
            _mainBorder = sender as Border;
            var window = (Window)((FrameworkElement)sender!).TemplatedParent;
            window.Activated += Window_Activated;
            window.Deactivated += Window_Deactivated;
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            if (_mainBorder != null)
            {
                _mainBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(50, 61, 85));
                _mainBorder.BorderBrush.Opacity = 0.5;
            }
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            if (_mainBorder != null)
            {
                _mainBorder.BorderBrush = (SolidColorBrush)Application.Current.Resources["MainBtnColor"];
            }
        }
    }
}
