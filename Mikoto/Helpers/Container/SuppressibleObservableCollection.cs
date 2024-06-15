using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Mikoto.Helpers
{
    public class SuppressibleObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;
        private bool _notificationSuppressed = false;
        public bool SuppressNotification
        {
            get
            {
                return _suppressNotification;
            }
            set
            {
                _suppressNotification = value;
                if (_suppressNotification == false && _notificationSuppressed)
                {
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    _notificationSuppressed = false;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SuppressNotification)
            {
                _notificationSuppressed = true;
                return;
            }
            base.OnCollectionChanged(e);
        }
    }
}
