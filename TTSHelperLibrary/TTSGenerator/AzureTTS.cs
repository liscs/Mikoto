using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;

namespace TTSHelperLibrary.TTSGenerator
{
    public class AzureTTS
    {
        string subscriptionKey = string.Empty;
        string subscriptionRegion = string.Empty;
        public AzureTTS() { }
        public void TTSInit(string key, string location)
        {
            subscriptionKey = key;
            subscriptionRegion = location;
        }

        public async Task TextToSpeechAsync(string text, string voice)
        {
            if (subscriptionKey == string.Empty || subscriptionRegion == string.Empty)
                return;
            ErrorMessage = null;
            var config = SpeechConfig.FromSubscription(subscriptionKey, subscriptionRegion);
            config.SpeechSynthesisVoiceName = voice;
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff44100Hz16BitMonoPcm);
            using (var synthesizer = new SpeechSynthesizer(config))
            {
                using (var result = await synthesizer.SpeakTextAsync(text))
                {
                    if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                        ErrorMessage = $"CANCELED: Reason={cancellation.Reason}";
                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            ErrorMessage += $", ErrorCode={cancellation.ErrorCode}, ErrorDetails=[{cancellation.ErrorDetails}]";
                        }
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
    }
}
