using System;
using System.Globalization;
using System.Windows.Data;
using MouseRecorderWpf.Models;

namespace MouseRecorderWpf.Converters
{
    public class ClickTypeToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ClickType clickType)
            {
                return (int)clickType;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index && Enum.IsDefined(typeof(ClickType), index))
            {
                return (ClickType)index;
            }
            return ClickType.LeftClick;
        }
    }
}
