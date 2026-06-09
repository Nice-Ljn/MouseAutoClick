using System;
using System.Drawing;

namespace MouseRecorderWpf.Models
{
    public enum MouseActionType
    {
        Move,
        LeftClick,
        RightClick,
        DoubleClick,
        MiddleClick,
        WheelUp,
        WheelDown
    }

    public class MouseAction
    {
        public MouseActionType ActionType { get; set; }
        public Point Position { get; set; }
        public DateTime Timestamp { get; set; }

        public MouseAction()
        {
        }

        public MouseAction(MouseActionType actionType, Point position, DateTime timestamp)
        {
            ActionType = actionType;
            Position = position;
            Timestamp = timestamp;
        }
    }
}
