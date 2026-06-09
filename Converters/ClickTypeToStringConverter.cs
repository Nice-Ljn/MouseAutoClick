using System;
using System.Globalization;
using System.Windows.Data;
using MouseRecorderWpf.Models;

namespace MouseRecorderWpf.Converters
{
    public class ClickTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ClickType clickType)
            {
                return clickType switch
                {
                    ClickType.LeftClick => "左键",
                    ClickType.RightClick => "右键",
                    ClickType.DoubleClick => "双击",
                    _ => "左键"
                };
            }
            return "左键";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    "左键" => ClickType.LeftClick,
                    "右键" => ClickType.RightClick,
                    "双击" => ClickType.DoubleClick,
                    _ => ClickType.LeftClick
                };
            }
            return ClickType.LeftClick;
        }
    }
}
