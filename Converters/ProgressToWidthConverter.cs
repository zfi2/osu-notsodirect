using System.Globalization;
using System.Windows.Data;

namespace osu_notsodirect_overlay.Converters
{
    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 || 
                !(values[0] is double value) || 
                !(values[1] is double minimum) ||
                !(values[2] is double maximum) || 
                !(values[3] is double trackWidth))
            {
                return 0.0;
            }

            if (maximum == minimum)
                return trackWidth;

            double percent = (value - minimum) / (maximum - minimum);
            return percent * trackWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 