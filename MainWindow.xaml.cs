using System.Windows;

namespace SubtitlesRunner
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowModel(Height);
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}