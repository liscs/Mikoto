using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static Mikoto.RegionOverride.LoaderWrapper;

namespace Mikoto.RegionOverride;

internal class LoaderWrapper
{
    private const uint CREATE_NORMAL = 0x00000000;
    private const uint CREATE_SUSPENDED = 0x00000004;
    private LEB _leb;
    private LERegistryRedirector _registry = new LERegistryRedirector(0);

    internal LoaderWrapper()
    {
        _leb = new LEB
        {
            AnsiCodePage = 932,
            OemCodePage = 932,
            LocaleID = 0x411,
            DefaultCharset = 128,
            HookUILanguageAPI = 0,
            // As we have abandoned the "default font" parameter,
            // we decide to put here some empty bytes.
            DefaultFaceName = new byte[64]
        };
        Timezone = "Tokyo Standard Time";
    }

    /// <summary>
    ///     Application that will be run.
    /// </summary>
    internal string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    ///     Command arguments.
    /// </summary>
    internal string? CommandLine { get; set; }

    /// <summary>
    ///     Working directory.
    /// </summary>
    internal string? CurrentDirectory { get; set; }

    /// <summary>
    ///     Whether the process should be created with CREATE_SUSPENDED. Useful when debugging with OllyDbg.
    /// </summary>
    internal bool DebugMode { get; set; }

    /// <summary>
    ///     New AnsiCodePage. Default value is 932.
    /// </summary>
    internal uint AnsiCodePage
    {
        get { return _leb.AnsiCodePage; }
        set { _leb.AnsiCodePage = value; }
    }

    /// <summary>
    ///     New OemCodePage. Default value is 932.
    /// </summary>
    internal uint OemCodePage
    {
        get { return _leb.OemCodePage; }
        set { _leb.OemCodePage = value; }
    }

    /// <summary>
    ///     New LocaleID. Default value is 0x411(1041).
    /// </summary>
    internal uint LocaleID
    {
        get { return _leb.LocaleID; }
        set { _leb.LocaleID = value; }
    }

    /// <summary>
    ///     New DefaultCharset. Default value is 128(Shift-JIS).
    /// </summary>
    internal uint DefaultCharset
    {
        get { return _leb.DefaultCharset; }
        set { _leb.DefaultCharset = value; }
    }

    /// <summary>
    ///     Should we hook UI-related API? Default value is 0.
    /// </summary>
    internal uint HookUILanguageAPI
    {
        get { return _leb.HookUILanguageAPI; }
        set { _leb.HookUILanguageAPI = value; }
    }

    /// <summary>
    ///     String that represents a Timezone.
    ///     This can be found in HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones.
    /// </summary>
    internal string Timezone
    {
        get { return _leb.Timezone.GetStandardName(); }
        set
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(value);
            _leb.Timezone.SetStandardName(tzi.StandardName);
            _leb.Timezone.SetDaylightName(tzi.StandardName);

            var tzi2 = ReadTZIFromRegistry(value);
            _leb.Timezone.Bias = tzi2.Bias;
            _leb.Timezone.StandardBias = tzi2.StandardBias;
            _leb.Timezone.DaylightBias = 0; //tzi2.DaylightBias;

        }
    }

    /// <summary>
    ///     Get or set the number of registry redirection entries.
    /// </summary>
    internal int NumberOfRegistryRedirectionEntries
    {
        get { return _registry.NumberOfRegistryRedirectionEntries; }
        set { _registry = new LERegistryRedirector(value); }
    }

    [DllImport("LoaderDll.dll", CharSet = CharSet.Unicode)]
    internal static extern uint LeCreateProcess(IntPtr leb,
                                          [MarshalAs(UnmanagedType.LPWStr), In] string applicationName,
                                          [MarshalAs(UnmanagedType.LPWStr), In] string? commandLine,
                                          [MarshalAs(UnmanagedType.LPWStr), In] string? currentDirectory,
                                          uint creationFlags,
                                          ref STARTUPINFOW startupInfo,
                                          out PROCESS_INFORMATION processInformation,
                                          IntPtr processAttributes,
                                          IntPtr threadAttributes,
                                          IntPtr environment,
                                          IntPtr token);
    //LeCreateProcess(
    //PLEB Leb,
    //PCWSTR ApplicationName,
    //PWSTR CommandLine = nullptr,
    //PCWSTR CurrentDirectory = nullptr,
    //ULONG CreationFlags = 0,
    //LPSTARTUPINFOW StartupInfo = nullptr,
    //PML_PROCESS_INFORMATION ProcessInformation = nullptr,
    //LPSECURITY_ATTRIBUTES ProcessAttributes = nullptr,
    //LPSECURITY_ATTRIBUTES ThreadAttributes = nullptr,
    //PVOID Environment = nullptr,
    //HANDLE Token = nullptr
    //);

    internal bool AddRegistryRedirectEntry(
        string root,
        string subkey,
        string valueName,
        string dataType,
        string data)
    {
        return _registry.AddRegistryEntry(root,
                                          subkey,
                                          valueName,
                                          dataType,
                                          data);
    }

    /// <summary>
    ///     Create process.
    /// </summary>
    /// <returns>Error number</returns>
    internal bool TryStart([NotNullWhen(true)] out PROCESS_INFORMATION processInfo)
    {
        if (string.IsNullOrEmpty(ApplicationName))
            throw new Exception("ApplicationName cannot null.");

        var newLEB = _leb.ToByteArray();
        newLEB = newLEB.Concat(_registry.GetBinaryData()).ToArray();

        var locLEB = Marshal.AllocHGlobal(newLEB.Length);
        Marshal.Copy(newLEB, 0, locLEB, newLEB.Length);

        PInvoke.AttachConsole(ATTACH_PARENT_PROCESS);

        var startupInfo = new STARTUPINFOW();

        var ret = LeCreateProcess(locLEB,
                               ApplicationName,
                               CommandLine,
                               CurrentDirectory,
                               DebugMode ? CREATE_SUSPENDED : CREATE_NORMAL,
                               ref startupInfo,
                               out processInfo,
                               nint.Zero,
                               nint.Zero,
                               nint.Zero,
                               nint.Zero);

        Marshal.FreeHGlobal(locLEB);

        if (ret == 0)
        {
            PInvoke.WaitForSingleObject(new PROCESS_INFORMATION().hProcess, INFINITE);
            PInvoke.CloseHandle(new PROCESS_INFORMATION().hProcess);
            PInvoke.CloseHandle(new PROCESS_INFORMATION().hThread);
        }

        return ret == 0;
    }

    private static byte[] SetBytes(byte[] bytesInput, IEnumerable<byte> bytesValue)
    {
        var i = 0;
        foreach (var byteChar in bytesValue)
        {
            bytesInput[i] = byteChar;
            i++;
        }
        return bytesInput;
    }

    internal static T BytesToStruct<T>(byte[] bytes) where T : struct
    {
        var size = Marshal.SizeOf(typeof(T));
        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, buffer, size);
            return (T)Marshal.PtrToStructure(buffer, typeof(T))!;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static REG_TZI_FORMAT ReadTZIFromRegistry(string id)
    {
        var tzi =
            (byte[])
            Registry.GetValue(
                              $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\{id}",
                              "TZI",
                              null)!;

        return BytesToStruct<REG_TZI_FORMAT>(tzi);
    }



    const uint INFINITE = uint.MaxValue;



    const uint ATTACH_PARENT_PROCESS = uint.MaxValue;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LEB
    {
        internal uint AnsiCodePage;
        internal uint OemCodePage;
        internal uint LocaleID;
        internal uint DefaultCharset;
        internal uint HookUILanguageAPI;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] DefaultFaceName;
        internal RTL_TIME_ZONE_INFORMATION Timezone;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTL_TIME_ZONE_INFORMATION
    {
        internal int Bias;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] StandardName;
        internal TIME_FIELDS StandardDate;
        internal int StandardBias;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] DaylightName;
        internal TIME_FIELDS DaylightDate;
        internal int DaylightBias;

        public override readonly string ToString()
        {
            return
                $"StandardName={Encoding.Unicode.GetString(StandardName)}";
        }

        internal readonly string GetStandardName()
        {
            return Encoding.Unicode.GetString(StandardName).Replace("\x00", "");
        }

        internal void SetStandardName(string name)
        {
            StandardName = SetBytes(new byte[64], Encoding.Unicode.GetBytes(name));
        }

        internal readonly string GetDaylightName()
        {
            return Encoding.Unicode.GetString(DaylightName).Replace("\x00", "");
        }

        internal void SetDaylightName(string name)
        {
            DaylightName = SetBytes(new byte[64], Encoding.Unicode.GetBytes(name));
        }
    }
#pragma warning disable CS0649
    private struct REG_TZI_FORMAT
    {
        internal int Bias;
        internal int StandardBias;
        internal int DaylightBias;
        internal SYSTEMTIME StandardDate;
        internal SYSTEMTIME DaylightDate;
    }
#pragma warning restore CS0649

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        internal ushort wYear;
        internal ushort wMonth;
        internal ushort wDayOfWeek;
        internal ushort wDay;
        internal ushort wHour;
        internal ushort wMinute;
        internal ushort wSecond;
        internal ushort wMilliseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TIME_FIELDS
    {
        internal ushort Year; // range [1601...]
        internal ushort Month; // range [1..12]
        internal ushort Day; // range [1..31]
        internal ushort Hour; // range [0..23]
        internal ushort Minute; // range [0..59]
        internal ushort Second; // range [0..59]
        internal ushort Milliseconds; // range [0..999]
        internal ushort Weekday; // range [0..6] == [Sunday..Saturday]
    }
}
