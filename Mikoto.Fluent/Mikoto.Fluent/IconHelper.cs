using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Mikoto.Fluent;

public static class IconHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        out SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags);

    const uint SHGFI_ICONLOCATION = 0x000001000;


    static (string modulePath, int iconIndex) GetIconLocation(string filePath)
    {
        SHFILEINFO info;
        SHGetFileInfo(
            filePath,
            0,
            out info,
            (uint)Marshal.SizeOf<SHFILEINFO>(),
            SHGFI_ICONLOCATION);

        return (info.szDisplayName, info.iIcon);
    }


    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport("kernel32.dll")]
    static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll")]
    static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

    [DllImport("kernel32.dll")]
    static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport("kernel32.dll")]
    static extern IntPtr LockResource(IntPtr hResData);

    [DllImport("kernel32.dll")]
    static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
    static readonly IntPtr RT_GROUP_ICON = (IntPtr)14;
    static readonly IntPtr RT_ICON = (IntPtr)3;


    public static byte[] ReadFileIconBytes_NoSystemDrawing(string filePath)
    {
        var (module, index) = GetIconLocation(filePath);
        if (string.IsNullOrEmpty(module)) module = filePath; // 兜底使用原文件路径

        IntPtr hModule = LoadLibraryEx(module, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
        if (hModule == IntPtr.Zero) return Array.Empty<byte>();

        try
        {
            // 游戏图标 ID 映射逻辑通常较复杂
            // 如果 index 是正数，它通常是索引；如果是负数，它是资源的绝对 ID (取反)
            IntPtr resId = index >= 0 ? (IntPtr)(index + 1) : (IntPtr)(-index);

            IntPtr hGroup = FindResource(hModule, resId, RT_GROUP_ICON);
            if (hGroup == IntPtr.Zero) return Array.Empty<byte>(); // 找不到资源时返回空

            IntPtr hGroupData = LoadResource(hModule, hGroup);
            IntPtr pGroup = LockResource(hGroupData);
            uint size = SizeofResource(hModule, hGroup);

            byte[] groupData = new byte[size];
            Marshal.Copy(pGroup, groupData, 0, (int)size);

            return BuildIco(hModule, groupData);
        }
        catch
        {
            return Array.Empty<byte>();
        }
        finally
        {
            FreeLibrary(hModule);
        }
    }

    static byte[] BuildIco(IntPtr hModule, byte[] groupData)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // ICONDIR
        bw.Write((ushort)0); // reserved
        bw.Write((ushort)1); // type
        ushort count = BitConverter.ToUInt16(groupData, 4);
        bw.Write(count);

        int entryOffset = 6;
        int imageOffset = 6 + 16 * count;

        var imageDataList = new List<byte[]>();

        for (int i = 0; i < count; i++)
        {
            bw.Write(groupData, entryOffset, 12);

            ushort iconId = BitConverter.ToUInt16(groupData, entryOffset + 12);
            IntPtr hIcon = FindResource(hModule, (IntPtr)iconId, RT_ICON);
            IntPtr hData = LoadResource(hModule, hIcon);
            IntPtr pData = LockResource(hData);
            uint size = SizeofResource(hModule, hIcon);

            byte[] img = new byte[size];
            Marshal.Copy(pData, img, 0, (int)size);
            imageDataList.Add(img);

            bw.Write((uint)size);
            bw.Write((uint)imageOffset);

            imageOffset += img.Length;
            entryOffset += 14;
        }

        foreach (var img in imageDataList)
            bw.Write(img);

        return ms.ToArray();
    }

    internal static Task<byte[]> GetIconFromExeAsync(string filePath)
    {
        return Task.Run(() => ReadFileIconBytes_NoSystemDrawing(filePath));
    }
}