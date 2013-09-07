using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace SubtitlesRunner
{
    public static class AppStartupOptions
    {
        private static readonly OptionSet Options;

        static AppStartupOptions()
        {
            Options = new OptionSet()
                .Add<string>("d|debug", "Debug", v => DebugMode = true)
                .Add<string>("f|file=", "SRT File to load", v => SRTFileToLoad = v);
        }

        public static bool DebugMode { get; set; }

        public static string SRTFileToLoad { get; set; }

        public static void ProcessArgs(IEnumerable<string> args)
        {
            Options.Parse(args);
        }

        public static void PrintUsage(TextWriter output)
        {
            Options.WriteOptionDescriptions(output);
        }
    }
}