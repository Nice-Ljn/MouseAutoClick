using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MouseRecorderWpf.Models
{
    public enum ClickType
    {
        LeftClick,
        RightClick,
        DoubleClick
    }

    public class MultiClickTarget : INotifyPropertyChanged
    {
        private int id;
        private Point position;
        private ClickType clickType;
        private int delay;
        private string name;

        public int Id
        {
            get => id;
            set { id = value; OnPropertyChanged(); }
        }

        public Point Position
        {
            get => position;
            set { position = value; OnPropertyChanged(); }
        }

        public ClickType ClickType
        {
            get => clickType;
            set { clickType = value; OnPropertyChanged(); OnPropertyChanged(nameof(ClickTypeName)); }
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

        public MultiClickTarget(Point position, ClickType clickType, int delay, string name)
        {
            Id = 0;
            Position = position;
            ClickType = clickType;
            Delay = delay;
            Name = name;
        }

        public string ClickTypeName
        {
            get => ClickType switch
            {
                ClickType.LeftClick => "左键",
                ClickType.RightClick => "右键",
                ClickType.DoubleClick => "双击",
                _ => "左键"
            };
            set
            {
                ClickType = value switch
                {
                    "左键" => ClickType.LeftClick,
                    "右键" => ClickType.RightClick,
                    "双击" => ClickType.DoubleClick,
                    _ => ClickType.LeftClick
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
