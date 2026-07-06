using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MouseRecorderWpf.Models
{
    public class MultiClickPlan : INotifyPropertyChanged
    {
        private string name;
        private List<MultiClickTarget> targets = new List<MultiClickTarget>();
        private int repeatCount = 1;
        private int loopInterval = 1000;

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public List<MultiClickTarget> Targets
        {
            get => targets;
            set { targets = value; OnPropertyChanged(); }
        }

        public int RepeatCount
        {
            get => repeatCount;
            set { repeatCount = value; OnPropertyChanged(); }
        }

        public int LoopInterval
        {
            get => loopInterval;
            set { loopInterval = value; OnPropertyChanged(); }
        }

        public MultiClickPlan(string name)
        {
            Name = name;
        }

        public MultiClickPlan(string name, List<MultiClickTarget> targets, int repeatCount, int loopInterval)
        {
            Name = name;
            Targets = new List<MultiClickTarget>(targets);
            RepeatCount = repeatCount;
            LoopInterval = loopInterval;
        }

        public MultiClickPlan Clone()
        {
            var clonedTargets = new List<MultiClickTarget>();
            foreach (var target in Targets)
            {
                clonedTargets.Add(new MultiClickTarget(target.Position, target.ClickType, target.Delay, target.Name)
                {
                    Id = target.Id
                });
            }
            return new MultiClickPlan(Name, clonedTargets, RepeatCount, LoopInterval);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
