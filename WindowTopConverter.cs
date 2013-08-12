using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SubtitlesRunner
{
    internal class WindowTopConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 4 || !(values[0] is double)) return null;
            var windowsTop = (double)values[0];
            var windowHeight = (double)values[1];
            var buttonsGrid = (double)values[2];
            var titlesGrid = (double)values[3];
            var top = !double.IsNaN(windowsTop) && windowsTop > double.Epsilon && windowsTop + buttonsGrid <= windowHeight
                       ? windowsTop
                       : windowHeight - buttonsGrid - titlesGrid;

            return new Thickness(0, top, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}