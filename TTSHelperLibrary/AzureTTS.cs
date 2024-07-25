using Microsoft.CognitiveServices.Speech;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TTSHelperLibrary
{
    public partial class AzureTTS : ITTS
    {
        /// <summary>
        /// 形如127.0.0.1:7890的代理字符串
        /// </summary>
        private string _proxy = string.Empty;
        private SpeechSynthesizer? _synthesizer;
        private string subscriptionKey = string.Empty;
        private string subscriptionRegion = string.Empty;

        private string _voice = string.Empty;
        private double _volume;
        private string _style = string.Empty;

        public AzureTTS(string key, string region, string voice, double volume, string style, string proxy)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(region) || string.IsNullOrEmpty(voice))
            {
                return;
            }
            subscriptionKey = key;
            subscriptionRegion = region;
            _voice = voice;
            _volume = volume;
            _style = style;
            _proxy = proxy;

            var config = SpeechConfig.FromSubscription(subscriptionKey, subscriptionRegion);
            if (!string.IsNullOrWhiteSpace(_proxy))
            {
                Regex regex = ProxyStringRegex();
                Match match = regex.Match(_proxy);
                if (match.Success)
                {
                    config.SetProxy(match.Result("${host}"), int.Parse(match.Result("${port}")));
                }
                else
                {
                    ErrorMessage += "Failed to set proxy! ";
                }
            }
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
            XElement ssml = BuildSsml(text);

            ErrorMessage = string.Empty;
            if (subscriptionKey == string.Empty
                || subscriptionRegion == string.Empty
                || string.IsNullOrWhiteSpace(text)
                || string.IsNullOrWhiteSpace(_voice)
                || _synthesizer == null)
                return;

            using var result = await _synthesizer.SpeakSsmlAsync(ssml.ToString());
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

        public async Task StopSpeakAsync()
        {
            if (_synthesizer == null)
                return;

            await _synthesizer.StopSpeakingAsync();
        }

        private XElement BuildSsml(string text)
        {
            const string ssmlString = """
                <speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xmlns:mstts="https://www.w3.org/2001/mstts" xml:lang="ja-JP">
                <voice name="ja-JP-NanamiNeural"><prosody volume="100">
                        <mstts:express-as style="sad">
                This is the text that is spoken.
                </mstts:express-as>
            </prosody>
                </voice>
            </speak>
            """;
            XElement ssml = XElement.Parse(ssmlString);
            XNamespace xNamespace = ssml.Name.Namespace;
            XElement expressNode = ssml.Descendants(ssml.GetNamespaceOfPrefix("mstts")! + "express-as").First();
            expressNode.Value = text;
            expressNode.Attribute("style")!.Value = _style;

            ssml.Descendants(xNamespace + "voice").First().Attribute("name")!.Value = _voice;
            ssml.Descendants(xNamespace + "prosody").First().Attribute("volume")!.Value = (_volume - 100).ToString("0.00") + "%";
            return ssml;
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
