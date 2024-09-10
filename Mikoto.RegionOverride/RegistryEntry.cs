using System.Globalization;

namespace Mikoto.RegionOverride
{
    internal class RegistryEntry
    {

        internal delegate string ValueReceiver(CultureInfo culture);

        /// <summary>
        /// e.g. HKEY_LOCAL_MACHINE
        /// </summary>
        internal string Root { get; private set; }

        /// <summary>
        /// e.g. System\CurrentControlSet\Control\Nls\CodePage
        /// </summary>
        internal string Key { get; private set; }

        /// <summary>
        /// e.g. ACP
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// e.g. REG_SZ
        /// </summary>
        internal string Type { get; private set; }

        /// <summary>
        /// e.g. culture => culture.TextInfo.ANSICodePage.ToString()
        /// In the example above, if the culture corresponds to ja-jp, the result is "932".
        /// </summary>
        internal ValueReceiver GetValue { get; private set; }

        internal RegistryEntry(string root, string key, string name, string type, ValueReceiver getValue)
        {
            Root = root;
            Key = key;
            Name = name;
            Type = type;
            GetValue = getValue;
        }
    }
}