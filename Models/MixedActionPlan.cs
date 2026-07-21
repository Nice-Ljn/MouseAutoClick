using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MouseRecorderWpf.Models
{
    public class MixedActionPlan : INotifyPropertyChanged
    {
        private string name;
        private List<MixedAction> actions = new List<MixedAction>();
        private int repeatCount = 1;
        private int loopInterval = 1000;

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public List<MixedAction> Actions
        {
            get => actions;
            set { actions = value; OnPropertyChanged(); }
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

        public MixedActionPlan(string name)
        {
            Name = name;
        }

        public MixedActionPlan(string name, List<MixedAction> actions, int repeatCount, int loopInterval)
        {
            Name = name;
            Actions = new List<MixedAction>();
            foreach (var action in actions)
            {
                Actions.Add(new MixedAction(action.ActionType, action.Position, action.KeyName, action.Delay, action.Name)
                {
                    Id = action.Id
                });
            }
            RepeatCount = repeatCount;
            LoopInterval = loopInterval;
        }

        public MixedActionPlan Clone()
        {
            return new MixedActionPlan(Name, Actions, RepeatCount, LoopInterval);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}