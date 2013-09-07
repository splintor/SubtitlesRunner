using System;
using System.IO;

namespace SubtitlesRunner
{
    internal class Logger
    {
        private string _logFilename;

        private string LogFilename
        {
            get
            {
                return _logFilename ?? (_logFilename = Path.Combine(Path.GetTempPath(), "SubtitlesRunner.log"));
            }
        }

        private static string Timestamp
        {
            get
            {
                var now = DateTime.Now;
                return now.ToLongDateString() + " " + now.ToLongTimeString();
            }
        }

        public bool IsDebugMode { get { return AppStartupOptions.DebugMode; } }

        public void Debug(string s, params object[] parameters)
        {
            if (!IsDebugMode) return;
            File.AppendAllText(LogFilename, Timestamp + @" DEBUG: " + string.Format(s, parameters) + Environment.NewLine);
        }

        public void Error(string s, params object[] parameters)
        {
            File.AppendAllText(LogFilename, Timestamp + @" ERROR: " + string.Format(s, parameters) + Environment.NewLine);
        }

        public void Exception(Exception exception, string s, params object[] parameters)
        {
            File.AppendAllText(LogFilename, Timestamp + @" ERROR: " + string.Format(s, parameters) + Environment.NewLine + exception + Environment.NewLine);
        }

        public void Clear()
        {
            File.Delete(LogFilename);
        }
    }
}