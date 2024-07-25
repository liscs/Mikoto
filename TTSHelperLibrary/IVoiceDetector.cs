
namespace TTSHelperLibrary
{
    public interface IVoiceDetector : IDisposable
    {
        Task<(bool, string)> IsVoicePlaying();
    }
}