using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Mikoto.Core.ViewModels;
using Windows.Win32;


namespace Mikoto.Fluent
{

    public sealed partial class TranslateFlowWindow
    {
        private int nXWindow;
        private int nYWindow;
        private int nX;
        private int nY;

        public TranslateViewModel ViewModel { get; }

        public TranslateFlowWindow(TranslateViewModel vm)
        {

            this.InitializeComponent();
            ViewModel = vm;
            var settings = vm.Env.AppSettings;
            AppWindow.Move(new Windows.Graphics.PointInt32(Convert.ToInt32(settings.TF_LocX), Convert.ToInt32(settings.TF_LocY)));
            AppWindow.Resize(new Windows.Graphics.SizeInt32(Convert.ToInt32(settings.TF_SizeW), Convert.ToInt32(settings.TF_SizeH)));
            IsAlwaysOnTop = true;

            // 1. 隐藏标题栏（工具栏）
            this.IsTitleBarVisible = false;

        }

        private void Root_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((UIElement)sender).ReleasePointerCaptures();
        }

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {
                ((UIElement)sender).CapturePointer(e.Pointer);
                nXWindow = AppWindow.Position.X;
                nYWindow = AppWindow.Position.Y;
                PInvoke.GetCursorPos(out System.Drawing.Point pt);
                nX = pt.X;
                nY = pt.Y;
            }
        }
        private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {
                PInvoke.GetCursorPos(out var pt);
                AppWindow.Move(new Windows.Graphics.PointInt32(nXWindow + (pt.X - nX), nYWindow + (pt.Y - nY)));
                e.Handled = true;
            }
        }

    }
}