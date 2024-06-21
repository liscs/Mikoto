using Microsoft.CognitiveServices.Speech;

namespace Mikoto
{
    public class AzureTTSSettingsViewModel : ViewModelBase
    {

        private VoiceInfo? selectedVoice;
        public VoiceInfo? SelectedVoice
        {
            get => selectedVoice;
            set
            {
                SetProperty(ref selectedVoice, value);
            }
        }

        private string? selectedVoiceStyle = string.Empty;
        public string? SelectedVoiceStyle
        {
            get => selectedVoiceStyle;
            set
            {
                SetProperty(ref selectedVoiceStyle, value);
            }
        }

        private List<VoiceInfo> voices = [];
        public List<VoiceInfo> Voices
        {
            get => voices;
            set
            {
                voices = value;
                VoiceLocales = value.Select(p => p.Locale).Distinct();
            }
        }
        private IEnumerable<string> voiceLocales = [];

        public IEnumerable<string> VoiceLocales
        {
            get => voiceLocales; set
            {
                SetProperty(ref voiceLocales, value);
            }
        }

        private IEnumerable<string> voiceNames = [];

        public IEnumerable<string> VoiceNames
        {
            get => voiceNames; set
            {
                SetProperty(ref voiceNames, value);
            }
        }

        private IEnumerable<string> voiceStyles = [];
        public IEnumerable<string> VoiceStyles
        {
            get => voiceStyles; set
            {
                SetProperty(ref voiceStyles, value);
                SelectedVoiceStyle = VoiceStyles.First();
            }
        }
    }
}