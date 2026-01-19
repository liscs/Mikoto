using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Mikoto.Fluent;

internal class IconHelper
{
    internal static async Task<ImageSource> GetIconFromExeAsync(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath)) return new BitmapImage();

        try
        {
            // 1. 获取文件对象
            StorageFile file = await StorageFile.GetFileFromPathAsync(exePath);

            // 2. 提取缩略图 (256像素通常是 .exe 包含的最大高清图标)
            // ThumbnailMode.SingleItem 专门用于获取单个文件的图标
            using var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);

            if (thumbnail != null)
            {
                // 3. 在内存中创建 BitmapImage
                BitmapImage bitmapImage = new BitmapImage();

                // 4. 将缩略图流直接设置给图片源（完全不经过磁盘，纯内存操作）
                await bitmapImage.SetSourceAsync(thumbnail);

                return bitmapImage;
            }
        }
        catch (Exception ex)
        {
            // 打印错误方便调试，比如路径权限问题或文件不存在
            System.Diagnostics.Debug.WriteLine($"图标提取失败: {ex.Message}");
        }

        // 5. 兜底处理：如果提取失败，返回一个默认的占位图（需确保 Assets 下有这个文件）
        return new BitmapImage();
    }
}
