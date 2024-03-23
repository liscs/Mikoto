using FontAwesome.WPF;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MisakaTranslator
{
    public class TranslateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (!(object.Equals(field, newValue)))
            {
                field = (newValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private Visibility _sourcePanelVisibility;

        public Visibility SourcePanelVisibility
        {
            get => _sourcePanelVisibility;

            set => SetProperty(ref _sourcePanelVisibility, value);
        }

        private FontAwesomeIcon _showSourceIcon = FontAwesomeIcon.Eye;

        public FontAwesomeIcon ShowSourceIcon
        {
            get
            {
                if (Common.AppSettings.TF_ShowSourceText)
                {
                    _showSourceIcon = FontAwesomeIcon.Eye;
                    SourcePanelVisibility = Visibility.Visible;

                }
                else
                {
                    _showSourceIcon = FontAwesomeIcon.EyeSlash;
                    SourcePanelVisibility = Visibility.Collapsed;
                }
                return _showSourceIcon;
            }

            set
            {
                if (value == FontAwesomeIcon.Eye)
                {
                    Common.AppSettings.TF_ShowSourceText = true;
                    SourcePanelVisibility = Visibility.Visible;
                }
                else
                {
                    Common.AppSettings.TF_ShowSourceText = false;
                    SourcePanelVisibility = Visibility.Collapsed;

                }
                SetProperty(ref _showSourceIcon, value);
            }
        }

        private bool _copyRubyVisibility = true;

        public bool CopyRubyVisibility
        {
            get
            {
                return _copyRubyVisibility;
            }

            set
            {
                SetProperty(ref _copyRubyVisibility, value);
            }
        }
    }
}