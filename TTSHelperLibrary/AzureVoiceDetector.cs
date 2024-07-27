using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System.Diagnostics;
using System.Globalization;

namespace TTSHelperLibrary
{
    public class AzureVoiceDetector : IDisposable, IVoiceDetector
    {
        private bool disposedValue;

        private readonly string _subscriptionKey;
        private readonly string _subscriptionRegion;

        private readonly PushAudioInputStream _pushAudioInputStream;
        private readonly SpeechRecognizer _recognizer;
        private WaveFormat _captureWaveFormat = new();

        public AzureVoiceDetector(string key, string region, string lang)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentException.ThrowIfNullOrWhiteSpace(region, nameof(region));
            ArgumentException.ThrowIfNullOrWhiteSpace(lang, nameof(lang));

            _subscriptionKey = key;
            _subscriptionRegion = region;

            var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _subscriptionRegion);
            speechConfig.SpeechRecognitionLanguage = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                                                .Where(c => c.TwoLetterISOLanguageName == lang)
                                                                .First(p => !p.IsNeutralCulture).Name;


            var buffer = new BufferedWaveProvider(_captureWaveFormat);
            _pushAudioInputStream = AudioInputStream.CreatePushStream(
                          AudioStreamFormat.GetWaveFormat(Convert.ToUInt32(_captureWaveFormat.SampleRate),
                                                          Convert.ToByte(_captureWaveFormat.BitsPerSample),
                                                          Convert.ToByte(_captureWaveFormat.Channels),
                                                          AudioStreamWaveFormat.PCM));
            AudioConfig audioConfig = AudioConfig.FromStreamInput(_pushAudioInputStream);
            _recognizer = new SpeechRecognizer(speechConfig, audioConfig);
        }

        private long _recordTimeLimit = 2000;
        public async Task<(bool, string)> IsVoicePlaying()
        {
            bool speechDetected = false;
            string resultInfo = string.Empty;
            _recognizer.SpeechStartDetected += (s, e) =>
            {
                speechDetected = true;
            };

            using WasapiLoopbackCapture _capture = new WasapiLoopbackCapture
            {
                WaveFormat = _captureWaveFormat
            };
            // 事件处理器
            _capture.DataAvailable += (s, e) =>
            {
                _pushAudioInputStream.Write(e.Buffer[..e.BytesRecorded]);

            };
            var task = _recognizer.RecognizeOnceAsync();
            // 开始录音
            _capture.StartRecording();

            // 捕获音频 1 秒钟
            Stopwatch sw = Stopwatch.StartNew();
            while (!speechDetected && sw.ElapsedMilliseconds < _recordTimeLimit)
            {
                await Task.Delay(250);
            }
            //动态调整录音时间
            _recordTimeLimit = sw.ElapsedMilliseconds;

            // 停止录音
            _capture.StopRecording();

            await _recognizer.StopContinuousRecognitionAsync();


            //ApplicationException
            //原因未知
            //TODO 查出原因
            if (task.Exception is null)
            {
                var result = await task;
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    resultInfo = $"Recognized: {result.Text}";
                    return (true, resultInfo);
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    resultInfo = "No speech could be recognized.";
                    return (false, resultInfo);
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    throw new TaskCanceledException($"CANCELED: Reason={cancellation.Reason};ErrorCode={cancellation.ErrorCode};ErrorDetails={cancellation.ErrorDetails}");
                }
            }
            else
            {
                throw task.Exception;
            }

            throw new UnreachableException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //释放托管状态(托管对象)
                    _pushAudioInputStream.Dispose();
                    _recognizer.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
