using System;
using System.Globalization;
using System.Windows.Data;
using MouseRecorderWpf.Models;

namespace MouseRecorderWpf.Converters
{
    public class ActionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MouseAction action)
            {
                string actionType = action.ActionType switch
                {
                    MouseActionType.Move => "移动",
                    MouseActionType.LeftClick => "左键点击",
                    MouseActionType.RightClick => "右键点击",
                    MouseActionType.DoubleClick => "双击",
                    MouseActionType.MiddleClick => "中键点击",
                    MouseActionType.WheelUp => "滚轮向上",
                    MouseActionType.WheelDown => "滚轮向下",
                    _ => "未知操作"
                };
                return $"{actionType} - X: {action.Position.X}, Y: {action.Position.Y}";
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
