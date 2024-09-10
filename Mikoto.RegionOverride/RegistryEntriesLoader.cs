﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace Mikoto.RegionOverride
{
    internal static class RegistryEntriesLoader
    {
        private static readonly RegistryEntry[] entries;
        private static readonly RegistryEntry[] entriesIncludingAdvanced;

        static RegistryEntriesLoader()
        {
            entries = new[]
            {
                new RegistryEntry("HKEY_LOCAL_MACHINE", @"System\CurrentControlSet\Control\Nls\CodePage", "InstallLanguage", "REG_SZ", culture => culture.TextInfo.LCID.ToString()),
                new RegistryEntry("HKEY_LOCAL_MACHINE", @"System\CurrentControlSet\Control\Nls\CodePage", "Default", "REG_SZ", culture => culture.TextInfo.LCID.ToString()),
                new RegistryEntry("HKEY_LOCAL_MACHINE", @"System\CurrentControlSet\Control\Nls\CodePage", "OEMCP", "REG_SZ", culture => culture.TextInfo.OEMCodePage.ToString()),
                new RegistryEntry("HKEY_LOCAL_MACHINE", @"System\CurrentControlSet\Control\Nls\CodePage", "ACP", "REG_SZ", culture => culture.TextInfo.ANSICodePage.ToString())

            };

            var advancedEntries = new[]
            {
                new RegistryEntry("HKEY_CURRENT_USER", @"Control Panel\International", "Locale", "REG_SZ", culture => culture.TextInfo.LCID.ToString("X8")),
                new RegistryEntry("HKEY_CURRENT_USER", @"Control Panel\International", "LocaleName", "REG_SZ", culture => $"{culture.TextInfo.CultureName}\x00"),
                new RegistryEntry("HKEY_CURRENT_USER", @"Control Panel\Desktop", "PreferredUILanguages", "REG_MULTI_SZ", culture => $"{culture.TextInfo.CultureName}\x00"),
                new RegistryEntry("HKEY_CURRENT_USER", @"Control Panel\Desktop\MuiCached", "MachinePreferredUILanguages", "REG_MULTI_SZ", culture => $"{culture.TextInfo.CultureName}\x00")
            };

            entriesIncludingAdvanced = entries.Concat(advancedEntries).ToArray();
        }

        internal static RegistryEntry[] GetRegistryEntries()
        {
            if (Environment.IsPrivilegedProcess)
            {
                return entriesIncludingAdvanced;
            }
            else
            {
                return entries;
            }
        }


    }
}