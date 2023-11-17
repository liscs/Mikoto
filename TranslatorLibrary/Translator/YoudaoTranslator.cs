using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TranslatorLibrary.lang;
using TranslatorLibrary.LanguageCode;

namespace TranslatorLibrary.Translator
{
    public class YoudaoTranslator : ITranslator
    {
        private string errorInfo;//错误信息

        public string TranslatorDisplayName { get { return Strings.YoudaoTranslator; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (sourceText == "" || desLang == "" || srcLang == "")
            {
                errorInfo = "Param Missing";
                return null;
            }
            srcLang = GetLanguageCode(new CultureInfo(srcLang));
            desLang = GetLanguageCode(new CultureInfo(desLang));

            // 原文
            string q = HttpUtility.UrlEncode(sourceText);
            string retString;

            string trans_type = srcLang + "2" + desLang;
            trans_type = trans_type.ToUpper();
            string url = "https://fanyi.youdao.com/translate?&doctype=json&type=" + trans_type + "&i=" + q;

            var hc = TranslatorCommon.GetHttpClient();
            try
            {
                retString = await hc.GetStringAsync(url);
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = ex.Message;
                return null;
            }

            YoudaoTransResult oinfo;
            try
            {
                oinfo = JsonSerializer.Deserialize<YoudaoTransResult>(retString, TranslatorCommon.JsonOP);
            }
            catch (JsonException)
            {
                errorInfo = "Deserialize failed. Possibly due to quota limits.";
                return null;
            }

            if (oinfo.errorCode == 0)
            {
                var sb = new StringBuilder(32);
                foreach (var youdaoTransDataList in oinfo.translateResult)
                {
                    foreach (var youdaoTransDataItem in youdaoTransDataList)
                    {
                        sb.Append(youdaoTransDataItem.tgt);
                    }
                }
                return sb.ToString();
            }
            else
            {
                errorInfo = "ErrorID:" + oinfo.errorCode;
                return null;
            }
        }

        private string GetLanguageCode(CultureInfo cultureInfo)
        {
            return YoudaoLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }

        public void TranslatorInit(string param1 = "", string param2 = "")
        {
            //不用初始化
        }
    }

#pragma warning disable 0649
    struct YoudaoTransResult
    {
        public string type;
        public int errorCode;
        public int elapsedTime;
        public YoudaoTransData[][] translateResult;
    }

    struct YoudaoTransData
    {
        public string src;
        public string tgt;
    }
}
