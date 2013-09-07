using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
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

            _progressTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100) };
            _progressTimer.Tick += HandleProgress;

            if (AppStartupOptions.SRTFileToLoad != null)
            {
                if (!File.Exists(AppStartupOptions.SRTFileToLoad))
                {
                    _logger.Error("Specified file ({0}) does not exist", AppStartupOptions.SRTFileToLoad);
                }
                else
                {
                    LoadSubtitlesFile(AppStartupOptions.SRTFileToLoad);
                }
            }
        }

        private List<SubtitleInfo> _subtitles;

        /// <summary>
        /// The clock time where progress started
        /// We need to keep it and constantly compare the current time to this anchor time because we can't rely on the
        /// <see cref="DispatcherTimer"/> to tick in fixed times.
        /// </summary>
        private DateTime _anchorTime;

        /// <summary>
        /// The progress offset of <see cref="_anchorTime"/>
        /// </summary>
        private TimeSpan _anchorOffset;

        private TimeSpan _progressTime;

        /// <summary>
        /// Gets or sets the current progress time.
        /// </summary>
        public TimeSpan ProgressTime
        {
            get { return _progressTime; }
            set
            {
                if (value == _progressTime) return;
                _progressTime = value;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged("ProgressTimeInTicks");
                // ReSharper restore ExplicitCallerInfoArgument

                StopCommand.RaiseCanExecuteChanged();

                // todo: Optimize search for subtitles (if needed)
                SubtitleTextToDisplay = string.Join("\n",
                                                    _subtitles.Where(s => s.StartTime <= ProgressTime && ProgressTime <= s.EndTime)
                                                              .Select(s => s.SubtitleText));
            }
        }

        public long ProgressTimeInTicks
        {
            get { return _progressTime.Ticks; }
            set
            {
                if (_progressTime.Ticks == value) return;
                ProgressTime = TimeSpan.FromTicks(value);

                if (!_progressTimer.IsEnabled) return;

                // this setter is usually called from the slider. We need to update our running counters accordingly
                _anchorOffset = ProgressTime;
                _anchorTime = DateTime.Now;
            }
        }

        private long _maximumProgressInTicks = 100;

        public long MaximumProgressInTicks
        {
            get { return _maximumProgressInTicks; }
            set
            {
                if (_maximumProgressInTicks == value) return;
                _maximumProgressInTicks = value;
                OnPropertyChanged();
            }
        }

        public bool HasSubtitles
        {
            get { return _subtitles != null && _subtitles.Count > 0; }
        }

        private string _subtitleTextToDisplay;

        public string SubtitleTextToDisplay
        {
            get { return _subtitleTextToDisplay; }
            set
            {
                if (value == _subtitleTextToDisplay) return;
                _subtitleTextToDisplay = value;
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
        private static readonly string[] TimeRangeSeparator = new[] { "-->" };

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
                lines = File.ReadAllLines(fileName, Encoding.Default);
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
                        var parts = line.Split(TimeRangeSeparator, StringSplitOptions.None);
                        if (parts.Length < 2)
                        {
                            _logger.Error("SRT File Load error at line {0}: Failed to parse time range line: {1}", lineCounter + 1, line);
                        }

                        currentSubtitle.StartTime = ParseTime(parts[0]);
                        currentSubtitle.EndTime = ParseTime(parts[1]);

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

            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("HasSubtitles");
            // ReSharper restore ExplicitCallerInfoArgument

            MaximumProgressInTicks = HasSubtitles ? _subtitles.Max(s => s.EndTime.Ticks) : 100;
            PlayCommand.RaiseCanExecuteChanged();
            DoStop();
        }

        private readonly DispatcherTimer _progressTimer;

        private void HandleProgress(object sender, EventArgs e)
        {
            ProgressTime = _anchorOffset + (DateTime.Now - _anchorTime);
        }

        private TimeSpan ParseTime(string s)
        {
            int i;
            var parts = s.Split(':', ',', '.').Select(p => int.TryParse(p, out i) ? i : -1).ToList();
            if (parts.Count < 3 || parts.Any(p => p < 0))
            {
                _logger.Error("Failed to parse date: ", s);
                return TimeSpan.Zero;
            }

            return parts.Count == 3 ? new TimeSpan(0, parts[0], parts[1], parts[2])
                                    : new TimeSpan(0, parts[0], parts[1], parts[2], parts[3]);
        }

        private RelayCommand _playCommand;

        public RelayCommand PlayCommand
        {
            get
            {
                return _playCommand ?? (_playCommand = new RelayCommand(DoPlayPause, CanPlayPause));
            }
        }

        private bool CanPlayPause(object obj)
        {
            return HasSubtitles; // todo: disable play/pause after play has finished
        }

        private void DoPlayPause(object parameter)
        {
            DoPlayPause();
        }

        private void DoPlayPause()
        {
            if (_progressTimer.IsEnabled)
            {
                _progressTimer.Stop();
            }
            else
            {
                _anchorOffset = ProgressTime;
                _anchorTime = DateTime.Now;
                _progressTimer.Start();
            }

            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsRunning");
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public bool IsRunning
        {
            get { return _progressTimer.IsEnabled; }
        }

        private RelayCommand _stopCommand;

        public RelayCommand StopCommand
        {
            get
            {
                return _stopCommand ?? (_stopCommand = new RelayCommand(DoStop, CanStop));
            }
        }

        private bool CanStop(object obj)
        {
            return ProgressTime > TimeSpan.Zero;
        }

        private void DoStop(object obj)
        {
            DoStop();
        }

        private void DoStop()
        {
            if (_progressTimer.IsEnabled)
            {
                DoPlayPause();
            }
            ProgressTime = TimeSpan.Zero;
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