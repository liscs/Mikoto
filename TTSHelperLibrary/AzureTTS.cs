using Microsoft.CognitiveServices.Speech;
using System.Text.RegularExpressions;

namespace TTSHelperLibrary
{
    public partial class AzureTTS : ITTS
    {
        /// <summary>
        /// 形如127.0.0.1:7890的代理字符串
        /// </summary>
        public string ProxyString { get; set; } = string.Empty;
        private SpeechSynthesizer? _synthesizer;
        private string subscriptionKey = string.Empty;
        private string subscriptionRegion = string.Empty;

        private string Voice { get; set; } = string.Empty;
        public AzureTTS(string key, string location, string voice, string proxy)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(location) || string.IsNullOrEmpty(voice))
            {
                return;
            }
            subscriptionKey = key;
            subscriptionRegion = location;
            Voice = voice;
            ProxyString = proxy;

            var config = SpeechConfig.FromSubscription(subscriptionKey, subscriptionRegion);
            if (!string.IsNullOrWhiteSpace(ProxyString))
            {
                Regex regex = ProxyStringRegex();
                Match match = regex.Match(ProxyString);
                if (match.Success)
                {
                    config.SetProxy(match.Result("${host}"), int.Parse(match.Result("${port}")));
                }
                else
                {
                    ErrorMessage += "Failed to set proxy! ";
                }
            }
            config.SpeechSynthesisVoiceName = Voice;
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff44100Hz16BitMonoPcm);

            try
            {
                _synthesizer?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
            _synthesizer = new SpeechSynthesizer(config);
        }


        public async Task SpeakAsync(string text)
        {
            ErrorMessage = string.Empty;
            if (subscriptionKey == string.Empty || subscriptionRegion == string.Empty || string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(Voice))
                return;
            var synthesizer = _synthesizer;
            if (synthesizer == null) { ErrorMessage += "Synthesizer should not be null!"; return; }
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

        public Task<SynthesisVoicesResult?> GetVoices()
        {
            if (_synthesizer == null)
            {
                return Task.FromResult<SynthesisVoicesResult?>(null);
            }
            else
            {
                return _synthesizer.GetVoicesAsync();
            }
        }
        /// <summary>
        /// 错误代码
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// AzureTTSAPI申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_API()
        {
            return "https://azure.microsoft.com/en-us/products/ai-services/text-to-speech";
        }

        /// <summary>
        /// AzureTTSAPI额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Bill()
        {
            return "https://portal.azure.com/#home";
        }

        public static string GetUrl_VoiceList()
        {
            return "https://speech.microsoft.com/portal/voicegallery";
        }

        [GeneratedRegex(@"(?<host>[^/]+)?:(?<port>\d+)?")]
        private static partial Regex ProxyStringRegex();
    }
}
