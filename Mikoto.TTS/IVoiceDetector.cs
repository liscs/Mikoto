namespace Mikoto.TTS
{
    public interface IVoiceDetector : IDisposable
    {
        Task<(bool, string)> IsVoicePlaying();
    }
}