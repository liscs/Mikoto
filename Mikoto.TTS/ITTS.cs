namespace Mikoto.TTS
{
    public interface ITTS
    {
        Task SpeakAsync(string s);
        Task StopSpeakAsync();
    }
}
