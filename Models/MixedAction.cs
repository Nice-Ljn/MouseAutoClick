using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MouseRecorderWpf.Models
{
    public enum MixedActionType
    {
        MouseLeftClick,
        MouseRightClick,
        MouseDoubleClick,
        MouseMiddleClick,
        KeyboardKeyDown,
        KeyboardKeyUp,
        KeyboardKeyPress
    }

    public class MixedAction : INotifyPropertyChanged
    {
        private int id;
        private MixedActionType actionType;
        private Point position;
        private string keyName;
        private int delay;
        private string name;

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(); }
        }

        public MixedActionType ActionType
        {
            get => actionType;
            set { actionType = value; OnPropertyChanged(); OnPropertyChanged(nameof(ActionTypeName)); }
        }

        public Point Position
        {
            get => position;
            set { position = value; OnPropertyChanged(); }
        }

        public string KeyName
        {
            get => keyName;
            set { keyName = value; OnPropertyChanged(); }
        }

        public int Delay
        {
            get => delay;
            set { delay = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public string ActionTypeName
        {
            get => ActionType switch
            {
                MixedActionType.MouseLeftClick => "鼠标左键",
                MixedActionType.MouseRightClick => "鼠标右键",
                MixedActionType.MouseDoubleClick => "鼠标双击",
                MixedActionType.MouseMiddleClick => "鼠标中键",
                MixedActionType.KeyboardKeyDown => "键盘按下",
                MixedActionType.KeyboardKeyUp => "键盘释放",
                MixedActionType.KeyboardKeyPress => "键盘按键",
                _ => "未知"
            };
            set
            {
                ActionType = value switch
                {
                    "鼠标左键" => MixedActionType.MouseLeftClick,
                    "鼠标右键" => MixedActionType.MouseRightClick,
                    "鼠标双击" => MixedActionType.MouseDoubleClick,
                    "鼠标中键" => MixedActionType.MouseMiddleClick,
                    "键盘按下" => MixedActionType.KeyboardKeyDown,
                    "键盘释放" => MixedActionType.KeyboardKeyUp,
                    "键盘按键" => MixedActionType.KeyboardKeyPress,
                    _ => MixedActionType.MouseLeftClick
                };
            }
        }

        public bool IsMouseAction => ActionType is MixedActionType.MouseLeftClick 
                                    or MixedActionType.MouseRightClick 
                                    or MixedActionType.MouseDoubleClick 
                                    or MixedActionType.MouseMiddleClick;

        public MixedAction()
        {
            Id = 0;
            ActionType = MixedActionType.MouseLeftClick;
            Position = new Point();
            KeyName = "";
            Delay = 50;
            Name = "";
        }

        public MixedAction(MixedActionType actionType, Point position, string keyName, int delay, string name)
        {
            Id = 0;
            ActionType = actionType;
            Position = position;
            KeyName = keyName;
            Delay = delay;
            Name = name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
