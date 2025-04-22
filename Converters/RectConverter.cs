using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace osu_notsodirect_overlay.Converters
{
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double width && values[1] is double height)
            {
                return new Rect(0, 0, width, height);
            }
            return new Rect(0, 0, 100, 10);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}