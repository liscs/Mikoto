using Mikoto.Translators.Interfaces;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Mikoto.Translators.Implementations
{
    //参考方法：https://www.lgztx.com/?p=209

    public class DreyeTranslator : ITranslator
    {
        private DreyeTranslator() { }
        private const int EC_DAT = 1;   //英中
        private const int CE_DAT = 2;   //中英
        private const int CJ_DAT = 3;   //中日
        private const int JC_DAT = 10;  //日中

        [DllImport("TransCOM.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MTInitCJ(int dat_index);

        [DllImport("TransCOM.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MTEndCJ();

        [DllImport("TransCOM.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int TranTextFlowCJ(
            byte[] src,
            byte[] dest,
            int dest_size,
            int dat_index
            );


        [DllImport("TransCOMEC.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MTInitEC(int dat_index);

        [DllImport("TransCOMEC.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int MTEndEC();

        [DllImport("TransCOMEC.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int TranTextFlowEC(
            byte[] src,
            byte[] dest,
            int dest_size,
            int dat_index
            );

        public string FilePath = string.Empty;//文件路径
        private string errorInfo = string.Empty;//错误信息

        public string TranslatorDisplayName { get; private set; }

        public string GetLastError()
        {
            return errorInfo;
        }

        public Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (FilePath == "")
            {
                return Task.FromResult<string?>(null);
            }

            Encoding shiftjis = Encoding.GetEncoding("shift-jis");
            Encoding gbk = Encoding.GetEncoding("gbk");
            Encoding utf8 = Encoding.GetEncoding("utf-8");
            string currentpath = Environment.CurrentDirectory;
            string workingDirectory = FilePath + "\\DreyeMT\\SDK\\bin";
            string ret;

            if (desLang == "zh")
            {
                if (srcLang == "ja")
                {
                    try
                    {
                        Directory.SetCurrentDirectory(workingDirectory);
                        MTInitCJ(JC_DAT); //返回值为-255
                        byte[] src = shiftjis.GetBytes(sourceText);
                        byte[] buffer = new byte[3000];
                        TranTextFlowCJ(src, buffer, 3000, JC_DAT);
                        ret = gbk.GetString(buffer);
                        MTEndCJ();
                    }
                    catch (Exception ex)
                    {
                        Environment.CurrentDirectory = currentpath;
                        errorInfo = ex.Message;
                        return Task.FromResult<string?>(null);
                    }
                }
                else if (srcLang == "en")
                {
                    try
                    {
                        Directory.SetCurrentDirectory(workingDirectory);
                        MTInitEC(EC_DAT); //返回值为-255
                        byte[] src = utf8.GetBytes(sourceText);
                        byte[] buffer = new byte[3000];
                        TranTextFlowEC(src, buffer, 3000, EC_DAT);
                        ret = gbk.GetString(buffer);
                        MTEndEC();
                    }
                    catch (Exception ex)
                    {
                        Environment.CurrentDirectory = currentpath;
                        errorInfo = ex.Message;
                        return Task.FromResult<string?>(null);
                    }
                }
                else
                {
                    errorInfo = "语言不支持";
                    return Task.FromResult<string?>(null);
                }
            }
            else
            {
                errorInfo = "语言不支持";
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult<string?>(ret);
        }

        public static ITranslator TranslatorInit(params string[] param)
        {
            DreyeTranslator dreyeTranslator = new()
            {
                TranslatorDisplayName = param[0],
                FilePath = param[1]
            };
            return dreyeTranslator;
        }
    }
}
