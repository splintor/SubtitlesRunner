using System;

namespace SubtitlesRunner
{
    public class SubtitleInfo
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string SubtitleText { get; set; }

        public int Id { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}--{2}: {3}", Id, StartTime, EndTime, SubtitleText);
        }
    }
}