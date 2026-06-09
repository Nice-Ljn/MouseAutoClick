using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MouseRecorderWpf.Models
{
    public class Script : INotifyPropertyChanged
    {
        private string name;
        private DateTime createdAt;
        private List<MouseAction> actions = new List<MouseAction>();

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAt
        {
            get => createdAt;
            set { createdAt = value; OnPropertyChanged(); }
        }

        public List<MouseAction> Actions
        {
            get => actions;
            set { actions = value; OnPropertyChanged(); }
        }

        public Script()
        {
            name = "未命名脚本";
            createdAt = DateTime.Now;
        }

        public Script(string name)
        {
            this.name = name;
            createdAt = DateTime.Now;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
