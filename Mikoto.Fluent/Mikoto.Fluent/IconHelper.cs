using Microsoft.UI.Xaml.Media.Imaging;
using Mikoto.TextHook;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;
using Serilog;

namespace Mikoto.Fluent;

/// <summary>
/// WinUI 图标辅助类
/// </summary>
internal class IconHelper
{
    public static async Task<BitmapSource?> GetIconFromExeAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        path = HookFileHelper.ToEntranceFilePath(path);

        // 1. 优先尝试从目录查找独立的 .ico 文件 (通常这类图标质量最高且包含多尺寸)
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                // 排除卸载程序的图标
                var icoPath = Directory.GetFiles(dir, "*.ico")
                                       .FirstOrDefault(p => !p.Contains("uninst", StringComparison.OrdinalIgnoreCase));

                if (icoPath != null)
                {
                    return new BitmapImage(new Uri(icoPath));
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex,"读取ICO目录失败: {Message}",ex.Message);
        }

        // 2. 如果没找到 .ico，则从 .exe 中提取图标
        return await GetFileIconAsync(path);
    }

    private static async Task<BitmapImage?> GetFileIconAsync(string filepath)
    {
        // 1. 定义一个同步的局部函数来处理 PInvoke
        // 这样 '&' 运算符就在非 async 环境下运行了
        static unsafe HICON ExtractIconInternal(string path)
        {
            HICON hIcon = HICON.Null;
            uint pIconId;
            fixed (char* pathPtr = path)
            {
                uint count = PInvoke.PrivateExtractIcons(
                    (PCWSTR)pathPtr,
                    0, 256, 256, &hIcon, &pIconId, 1, 0);

                if (count == 0 || count == uint.MaxValue)
                    return HICON.Null;

                return hIcon;
            }
        }

        // 2. 调用同步逻辑获取句柄
        HICON hIcon = ExtractIconInternal(filepath);

        try
        {
            // 注意：CsWin32 的 HICON 强转为 IntPtr
            using var icon = System.Drawing.Icon.FromHandle((IntPtr)hIcon);
            using var bitmap = icon.ToBitmap();
            using var ms = new MemoryStream();

            // 使用 PNG 格式保存以保留透明度通道
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            var bi = new BitmapImage();
            using var raStream = ms.AsRandomAccessStream();
            // SetSourceAsync 必须在 UI 线程执行
            await bi.SetSourceAsync(raStream);

            return bi;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "图标转换失败: {Message}", ex.Message);
            return null;
        }
        finally
        {
            // 必须手动销毁原生 HICON 句柄，防止 GDI 对象泄漏
            if (hIcon != HICON.Null)
            {
                PInvoke.DestroyIcon(hIcon);
            }
        }
    }
}