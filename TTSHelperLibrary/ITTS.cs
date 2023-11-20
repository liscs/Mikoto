using System.Threading.Tasks;

namespace TTSHelperLibrary
{
    public interface ITTS
    {
        public Task SpeakAsync(string s);
    }
}
