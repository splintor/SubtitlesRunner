using System;
using System.Windows;

namespace SubtitlesRunner
{
    public partial class App
    {
        private readonly Logger _logger = new Logger();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                AppStartupOptions.ProcessArgs(e.Args);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Processing failed for command line {0}", string.Join(" ", e.Args));
            }
        }
    }
}