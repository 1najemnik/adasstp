using System;
using System.Globalization;
using System.Windows.Data;

namespace Foundation.Converters
{
    public class EqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null) return Equals(parameter, value);
            return parameter.GetType().IsValueType
                ? string.Equals(value.ToString(), parameter.ToString(), StringComparison.CurrentCultureIgnoreCase)
                : Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, true) ? parameter : null;
        }
    }
}
