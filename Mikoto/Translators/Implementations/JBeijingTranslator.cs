using Mikoto.Translators.Interfaces;
using System.Runtime.InteropServices;
using System.Windows;

namespace Mikoto.Translators.Implementations
{
    public class JBeijingTranslator : ITranslator
    {
        private JBeijingTranslator() { }
        /*
         * 根据猜测得到的DLL函数 来源 https://github.com/Artikash/VNR-Core/blob/6ed038bda9dcd35696040bd45d31afa6a30e8978/py/libs/jbeijing/jbjct.py
        int __cdecl JC_Transfer_Unicode(
            HWND hwnd,
            UINT fromCodePage,
            UINT toCodePage,
            int unknown,
            int unknown,
            LPCWSTR from,
            LPWSTR to,
            int &toCapacity,
            LPWSTR buffer,
            int &bufferCapacity)
        */

        [DllImport("JBJCT.dll", EntryPoint = "JC_Transfer_Unicode", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int JC_Transfer_Unicode(
            int hwnd,
            uint fromCodePage,
            uint toCodePage,
            int unknown,
            int unknown1,
            nint from,
            nint to,
            ref int toCapacity,
            nint buffer,
            ref int bufferCapacity);

        public string JBJCTDllPath = string.Empty;//DLL路径
        private string errorInfo = string.Empty;//错误信息

        public string TranslatorDisplayName { get { return Application.Current.Resources["JBeijingTranslator"].ToString()!; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        public Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            string JBeijingTranslatorPath = JBJCTDllPath;

            if (JBeijingTranslatorPath == string.Empty)
            {
                return Task.FromResult<string?>(null);
            }

            /*
            CP932   SAP Shift-JIS
            CP950   SAP 繁体中文
            CP936   SAP 简体中文
            */

            string path = Environment.CurrentDirectory;
            Environment.CurrentDirectory = JBeijingTranslatorPath;

            nint jp = Marshal.StringToHGlobalUni(sourceText);

            nint jp2 = Marshal.AllocHGlobal(3000);
            nint jp3 = Marshal.AllocHGlobal(3000);

            int p1 = 1500;
            int p2 = 1500;

            try
            {
                int a = JC_Transfer_Unicode(0, 932, 936, 1, 1, jp, jp2, ref p1, jp3, ref p2);
            }
            catch (Exception ex)
            {
                errorInfo = ex.Message;
                return Task.FromResult<string?>(null);
            }

            Environment.CurrentDirectory = path;

            string? ret = Marshal.PtrToStringAuto(jp2);

            Marshal.FreeHGlobal(jp);
            Marshal.FreeHGlobal(jp2);
            Marshal.FreeHGlobal(jp3);

            return Task.FromResult(ret);
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            JBeijingTranslator jBeijingTranslator = new();
            jBeijingTranslator.JBJCTDllPath = param.First();
            return jBeijingTranslator;
        }
    }
}
