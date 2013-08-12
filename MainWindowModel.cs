using System.ComponentModel;
using System.Runtime.CompilerServices;
using SubtitlesRunner.Annotations;

namespace SubtitlesRunner
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel(double windowHeight)
        {
            CurrentSubtitle = "This is...\n...just a test";
            WindowTop = windowHeight - 100;
        }

        private string _currentSubtitle;

        public string CurrentSubtitle
        {
            get { return _currentSubtitle; }
            set
            {
                if (value == _currentSubtitle) return;
                _currentSubtitle = value;
                OnPropertyChanged();
            }
        }

        private double _windowTop;

        public double WindowTop
        {
            get { return _windowTop; }
            set
            {
                if (value.Equals(_windowTop)) return;
                _windowTop = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}