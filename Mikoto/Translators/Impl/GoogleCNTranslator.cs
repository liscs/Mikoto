﻿using Mikoto.Helpers.Exceptions;
using System.Globalization;
using System.Web;
using System.Windows;

namespace Mikoto.Translators
{
    public class GoogleCNTranslator : ITranslator
    {
        private GoogleCNTranslator() { }
        private string errorInfo = string.Empty;//错误信息

        public string TranslatorDisplayName { get { return Application.Current.Resources["GoogleCNTranslator"].ToString()!; } }

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            srcLang = GetLanguageCode(new CultureInfo(srcLang));
            desLang = GetLanguageCode(new CultureInfo(desLang));

            string googleTransUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&sl=" + srcLang + "&tl=" + desLang + "&q=" + HttpUtility.UrlEncode(sourceText);

            var hc = TranslatorCommon.HttpClientInstance;

            try
            {
                var ResultHtml = await hc.GetStringAsync(googleTransUrl);

                dynamic TempResult = System.Text.Json.JsonSerializer.Deserialize<dynamic>(ResultHtml, TranslatorCommon.JsonSerializerOptions)!;

                string ResultText = "";

                for (int i = 0; i < TempResult[0].GetArrayLength(); i++)
                {
                    ResultText += TempResult[0][i][0];
                }

                return ResultText;
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                errorInfo = ex.GetOriginalException().Message;
                return null;
            }
            catch (TaskCanceledException ex)
            {
                errorInfo = ex.GetOriginalException().Message;
                return null;
            }
        }
        public static string GetLanguageCode(CultureInfo cultureInfo)
        {
            return GoogleCNLanguageCodeConverter.GetLanguageCode(cultureInfo);
        }
        public static ITranslator TranslatorInit(params string[] param)
        {

            return new GoogleCNTranslator();
        }
    }
}
