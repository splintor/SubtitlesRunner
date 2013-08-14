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
        private readonly Logger _logger;

        public MainWindowModel()
        {
            _logger = new Logger();
            _logger.Clear();
        }

        private List<SubtitleInfo> _subtitles;

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
                    Filter = "SubRip Text files (*.srt)|*.srt|All files (*.*)|*.*",
                    Title = "Select Subtitles File"
                };
            if (dialog.ShowDialog() == true)
            {
                LoadSubtitlesFile(dialog.FileName);
            }
        }

        private enum ParseStep
        {
            Id,
            TimeRange,
            Subtitle,
        }

        private void LoadSubtitlesFile(string fileName)
        {
            string[] lines;

            try
            {
                lines = File.ReadAllLines(fileName);
            }
            catch (Exception exception)
            {
                _logger.Exception(exception, "Failed to load subtitles file {0}", fileName);
                return;
            }

            _subtitles = new List<SubtitleInfo>();
            var currentSubtitle = new SubtitleInfo();
            var currentStep = ParseStep.Id;

            for (int lineCounter = 0; lineCounter < lines.Length; ++lineCounter)
            {
                var line = lines[lineCounter];
                switch (currentStep)
                {
                    case ParseStep.Id:
                        {
                            int id;
                            if (int.TryParse(line, out id))
                            {
                                currentSubtitle.Id = id;
                                currentStep = ParseStep.TimeRange;
                            }
                            else if (line.Trim().Length > 0) // allow multiple empty lines as separators
                            {
                                _logger.Error("SRT File Load error at line {0}: Failed to parse id line: {1}", lineCounter + 1, line);
                            }
                        }
                        break;

                    case ParseStep.TimeRange:
                        // todo: parse time range
                        currentStep = ParseStep.Subtitle;
                        break;

                    case ParseStep.Subtitle:
                        if (line.Length == 0)
                        {
                            _subtitles.Add(currentSubtitle);
                            _logger.Debug("Parsed step {0}", currentSubtitle);
                            currentSubtitle = new SubtitleInfo();
                            currentStep = ParseStep.Id;
                        }
                        else
                        {
                            if (currentSubtitle.SubtitleText == null)
                            {
                                currentSubtitle.SubtitleText = line;
                            }
                            else
                            {
                                currentSubtitle.SubtitleText = currentSubtitle.SubtitleText + Environment.NewLine + line;
                            }
                        }
                        break;
                }
            }

            if (currentSubtitle.Id > 0)
            {
                _subtitles.Add(currentSubtitle);
                _logger.Debug("Parsed last step {0}", currentSubtitle);
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