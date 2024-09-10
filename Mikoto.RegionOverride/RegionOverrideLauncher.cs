//参考https://github.com/xupefei/Locale-Emulator


using System;
using System.Diagnostics;
using System.Globalization;
using Windows.Win32;
using Windows.Win32.System.Threading;

namespace Mikoto.RegionOverride;

public class RegionOverrideLauncher
{
    /// <summary>
    /// 默认以日文系统、东京时间启动
    /// </summary>
    public static Process? Start(ProcessStartInfo processStartInfo)
    {
        return Start(processStartInfo, null, null);
    }

    public static Process? Start(ProcessStartInfo processStartInfo,
                                 CultureInfo? cultureInfo,
                                 TimeZoneInfo? timeZoneInfo)
    {
        ArgumentNullException.ThrowIfNull(processStartInfo);

        cultureInfo ??= new CultureInfo("ja-JP");
        timeZoneInfo ??= TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

        string fullPath = Path.GetFullPath(processStartInfo.FileName);
        var currentDirectory = Path.GetDirectoryName(fullPath);
        var ansiCodePage = (uint)cultureInfo.TextInfo.ANSICodePage;
        var oemCodePage = (uint)cultureInfo.TextInfo.OEMCodePage;
        var localeID = (uint)cultureInfo.TextInfo.LCID;
        var defaultCharset = (uint)GetCharsetFromANSICodepage(cultureInfo.TextInfo.ANSICodePage);

        RegistryEntry[] registries = RegistryEntriesLoader.GetRegistryEntries();

        var loaderWrapper = new LoaderWrapper
        {
            ApplicationName = fullPath,
            CommandLine = processStartInfo.Arguments,
            CurrentDirectory = currentDirectory,
            AnsiCodePage = ansiCodePage,
            OemCodePage = oemCodePage,
            LocaleID = localeID,
            DefaultCharset = defaultCharset,
            HookUILanguageAPI = 0,
            Timezone = timeZoneInfo.StandardName,
            NumberOfRegistryRedirectionEntries = registries.Length,
            DebugMode = false,
        };

        registries.ToList()
            .ForEach(
                item =>
                    loaderWrapper.AddRegistryRedirectEntry(item.Root,
                        item.Key,
                        item.Name,
                        item.Type,
                        item.GetValue(cultureInfo)));
        if (loaderWrapper.TryStart(out PROCESS_INFORMATION processInfo))
        {
            int processId = (int)processInfo.dwProcessId;
            PInvoke.CloseHandle(processInfo.hProcess);
            PInvoke.CloseHandle(processInfo.hThread);
            return Process.GetProcessById(processId);
        }
        else
        {
            return null;
        }
    }

    private static int GetCharsetFromANSICodepage(int ansicp)
    {
#pragma warning disable CS0219 // 变量已被赋值，但从未使用过它的值
        const int ANSI_CHARSET = 0;
        const int DEFAULT_CHARSET = 1;
        const int SYMBOL_CHARSET = 2;
        const int SHIFTJIS_CHARSET = 128;
        const int HANGEUL_CHARSET = 129;
        const int HANGUL_CHARSET = 129;
        const int GB2312_CHARSET = 134;
        const int CHINESEBIG5_CHARSET = 136;
        const int OEM_CHARSET = 255;
        const int JOHAB_CHARSET = 130;
        const int HEBREW_CHARSET = 177;
        const int ARABIC_CHARSET = 178;
        const int GREEK_CHARSET = 161;
        const int TURKISH_CHARSET = 162;
        const int VIETNAMESE_CHARSET = 163;
        const int THAI_CHARSET = 222;
        const int EASTEUROPE_CHARSET = 238;
        const int RUSSIAN_CHARSET = 204;
        const int MAC_CHARSET = 77;
        const int BALTIC_CHARSET = 186;
#pragma warning restore CS0219 // 变量已被赋值，但从未使用过它的值

        var charset = ANSI_CHARSET;

        switch (ansicp)
        {
            case 932: // Japanese
                charset = SHIFTJIS_CHARSET;
                break;
            case 936: // Simplified Chinese
                charset = GB2312_CHARSET;
                break;
            case 949: // Korean
                charset = HANGEUL_CHARSET;
                break;
            case 950: // Traditional Chinese
                charset = CHINESEBIG5_CHARSET;
                break;
            case 1250: // Eastern Europe
                charset = EASTEUROPE_CHARSET;
                break;
            case 1251: // Russian
                charset = RUSSIAN_CHARSET;
                break;
            case 1252: // Western European Languages
                charset = ANSI_CHARSET;
                break;
            case 1253: // Greek
                charset = GREEK_CHARSET;
                break;
            case 1254: // Turkish
                charset = TURKISH_CHARSET;
                break;
            case 1255: // Hebrew
                charset = HEBREW_CHARSET;
                break;
            case 1256: // Arabic
                charset = ARABIC_CHARSET;
                break;
            case 1257: // Baltic
                charset = BALTIC_CHARSET;
                break;
        }

        return charset;
    }

}
