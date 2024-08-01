using System.Windows.Input;

namespace Mikoto.Helpers.Input;

public static class GlobalKeyboardHook
{
    public static void Initialize() { }

    static GlobalKeyboardHook()
    {
        InputManager.Current.PreProcessInput += Current_PreProcessInput;
    }

    private static void Current_PreProcessInput(object sender, PreProcessInputEventArgs args)
    {
        if (args.StagingItem.Input is KeyEventArgs k
            && k.RoutedEvent == Keyboard.PreviewKeyDownEvent
            && (k.Key == Key.OemTilde || k.ImeProcessedKey == Key.OemTilde))
        {
            // 标记事件为已处理
            k.Handled = true;
            LogViewer.LogWindow.Show();
        }
    }
}
