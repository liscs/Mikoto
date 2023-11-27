﻿using Microsoft.CognitiveServices.Speech;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TTSHelperLibrary
{
    public class AzureTTS : ITTS
    {
        //形如127.0.0.1:7890的代理字符串
        public string ProxyString { get; set; }
        private SpeechSynthesizer _synthesizer;

        string subscriptionKey = string.Empty;
        string subscriptionRegion = string.Empty;

        string Voice { get; set; }
        public AzureTTS() { }
        public void TTSInit(string key, string location)
        {
            subscriptionKey = key;
            subscriptionRegion = location;

            var config = SpeechConfig.FromSubscription(subscriptionKey, subscriptionRegion);
            if (ProxyString != string.Empty)
            {
                if (ProxyString.Contains(':'))
                {
                    try
                    {
                        config.SetProxy(ProxyString.Split(':').ElementAt(0), Convert.ToInt32(ProxyString.Split(':').ElementAt(1)));
                    }
                    catch (Exception)
                    {
                        //设置代理失败
                        ErrorMessage += "Failed to set proxy! ";
                    }
                }
                else
                {
                    ErrorMessage += "Failed to set proxy! ";
                }
            }
            config.SpeechSynthesisVoiceName = Voice;
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff44100Hz16BitMonoPcm);
            _synthesizer?.Dispose();
            _synthesizer = new SpeechSynthesizer(config);
        }

        public async Task SpeakAsync(string text)
        {
            ErrorMessage = string.Empty;
            if (subscriptionKey == string.Empty || subscriptionRegion == string.Empty || string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(Voice))
                return;
            var synthesizer = _synthesizer;
            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    ErrorMessage += $"CANCELED: Reason={cancellation.Reason}";
                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        ErrorMessage += $", ErrorCode={cancellation.ErrorCode}, ErrorDetails=[{cancellation.ErrorDetails}]";
                    }
                }
            }
        }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// AzureTTSAPI申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_allpyAPI()
        {
            return "https://azure.microsoft.com/en-us/products/ai-services/text-to-speech";
        }

        /// <summary>
        /// AzureTTSAPI额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_bill()
        {
            return "https://portal.azure.com/#home";
        }

        public static string GetUrl_VoiceList()
        {
            return "https://speech.microsoft.com/portal";
        }

        public void SetTTSVoice(string azureTTSVoice)
        {
            Voice = azureTTSVoice;
        }
    }
}
