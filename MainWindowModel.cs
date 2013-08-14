using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using SubtitlesRunner.Annotations;

namespace SubtitlesRunner
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private List<SubtitleInfo> _subTitles;

        public MainWindowModel()
        {
            //CurrentSubtitle = "This is...\n...just a test";
        }

        public DateTime CurrentTime
        {
            get { return new DateTime(CurrentTimeInTicks); }
        }

        private long _currentTimeInTicks;

        public long CurrentTimeInTicks
        {
            get { return _currentTimeInTicks; }
            set
            {
                if (_currentTimeInTicks == value) return;
                _currentTimeInTicks = value;
                OnPropertyChanged();
            }
        }

        private SubtitleInfo _currentSubtitle;

        public SubtitleInfo CurrentSubtitle
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

        private ICommand _openFileDialogCommand;

        public ICommand OpenFileDialogCommand
        {
            get { return _openFileDialogCommand ?? (_openFileDialogCommand = new RelayCommand(OpenSubTitlesFileDialog)); }
        }

        private void OpenSubTitlesFileDialog(object obj)
        {
            var dialog = new OpenFileDialog
                {
                    CheckFileExists = true,
                    DefaultExt = "srt",
                    Filter = "Subtitles files (*.srt)|*.srt|All files (*.*)|*.*",
                    Title = "Select Subtitles File"
                };
            if (dialog.ShowDialog() == true)
            {
                LoadSubtitlesFile(dialog.FileName);
            }
        }

        private void LoadSubtitlesFile(string fileName)
        {
            _subTitles = new List<SubtitleInfo>();
            var lines = File.ReadAllLines(fileName);

            //throw new NotImplementedException();
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