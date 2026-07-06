using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using MouseRecorderWpf.Models;
using MouseRecorderWpf.Services;

namespace MouseRecorderWpf.ViewModels
{
    public class MultiClickViewModel : INotifyPropertyChanged
    {
        private readonly MultiClickManager manager;
        private readonly DispatcherTimer positionTimer;

        private string statusText = "就绪";
        private string currentPosition = "(0, 0)";
        private int repeatCount = 1;
        private int loopInterval = 1000;
        private MultiClickTarget? selectedTarget;
        private int selectedIndex = -1;
        private int nextTargetNumber = 1;
        private string currentPlanName = "方案A";
        private int planNumber = 1;

        public ObservableCollection<MultiClickTarget> Targets { get; } = new ObservableCollection<MultiClickTarget>();
        public ObservableCollection<string> PlanNames { get; } = new ObservableCollection<string>();

        public string StatusText
        {
            get => statusText;
            set { statusText = value; OnPropertyChanged(); }
        }

        public string CurrentPosition
        {
            get => currentPosition;
            set { currentPosition = value; OnPropertyChanged(); }
        }

        public int RepeatCount
        {
            get => repeatCount;
            set 
            { 
                repeatCount = Math.Max(0, value); 
                manager.RepeatCount = repeatCount;
                OnPropertyChanged(); 
            }
        }

        public int LoopInterval
        {
            get => loopInterval;
            set 
            { 
                loopInterval = Math.Max(0, value); 
                manager.LoopInterval = loopInterval;
                OnPropertyChanged(); 
            }
        }

        public MultiClickTarget? SelectedTarget
        {
            get => selectedTarget;
            set { selectedTarget = value; OnPropertyChanged(); }
        }

        public int SelectedIndex
        {
            get => selectedIndex;
            set { selectedIndex = value; OnPropertyChanged(); }
        }

        public string CurrentPlanName
        {
            get => currentPlanName;
            set { currentPlanName = value; OnPropertyChanged(); }
        }

        public bool IsRunning => manager.IsRunning;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MultiClickViewModel()
        {
            manager = new MultiClickManager();
            manager.CurrentTargetChanged += OnCurrentTargetChanged;
            manager.ClickCompleted += OnClickCompleted;
            manager.PlanChanged += OnPlanChanged;

            positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            positionTimer.Tick += OnPositionTimerTick;
            positionTimer.Start();

            RefreshPlanNames();
            RefreshTargets();
        }

        private void OnPositionTimerTick(object? sender, EventArgs e)
        {
            var pos = MouseSimulator.GetCurrentPosition();
            CurrentPosition = $"({pos.X}, {pos.Y})";
        }

        private void OnCurrentTargetChanged(int index)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                SelectedIndex = index;
            });
        }

        private void OnClickCompleted()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusText = "就绪";
                OnPropertyChanged(nameof(IsRunning));
            });
        }

        private void OnPlanChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CurrentPlanName = manager.CurrentPlan?.Name ?? "方案A";
                RepeatCount = manager.RepeatCount;
                LoopInterval = manager.LoopInterval;
                RefreshTargets();
            });
        }

        public void AddCurrentPosition()
        {
            var pos = MouseSimulator.GetCurrentPosition();
            var targetName = $"点{nextTargetNumber}";
            manager.AddTarget(pos, ClickType.LeftClick, 500, targetName);
            nextTargetNumber++;
            RefreshTargets();
        }

        public void RemoveSelected()
        {
            if (SelectedTarget == null) return;
            manager.RemoveTarget(SelectedTarget.Id);
            RefreshTargets();
        }

        public void ClearAll()
        {
            manager.ClearTargets();
            Targets.Clear();
            nextTargetNumber = 1;
        }

        public void MoveUp()
        {
            if (SelectedIndex <= 0) return;
            manager.SwapTargets(SelectedIndex, SelectedIndex - 1);
            RefreshTargets();
            SelectedIndex--;
        }

        public void MoveDown()
        {
            if (SelectedIndex < 0 || SelectedIndex >= Targets.Count - 1) return;
            manager.SwapTargets(SelectedIndex, SelectedIndex + 1);
            RefreshTargets();
            SelectedIndex++;
        }

        public void Start()
        {
            if (manager.IsRunning || manager.Targets.Count == 0) return;
            StatusText = "运行中... (按 F6 停止)";
            OnPropertyChanged(nameof(IsRunning));
            manager.Start();
        }

        public void Stop()
        {
            if (!manager.IsRunning) return;
            manager.Stop();
        }

        public void SyncTargetFromGrid(int index)
        {
            if (index >= 0 && index < Targets.Count && index < manager.Targets.Count)
            {
                manager.Targets[index] = Targets[index];
            }
        }

        public void AddPlan()
        {
            string newName;
            do
            {
                newName = $"方案{(char)('A' + planNumber)}";
                planNumber++;
            } while (PlanNames.Contains(newName));

            manager.AddPlan(newName);
            RefreshPlanNames();
            CurrentPlanName = newName;
        }

        public void DeletePlan()
        {
            if (PlanNames.Count <= 1) return;
            manager.DeletePlan(CurrentPlanName);
            RefreshPlanNames();
            CurrentPlanName = manager.CurrentPlan?.Name ?? PlanNames.FirstOrDefault() ?? "方案A";
        }

        public void SwitchPlan(string planName)
        {
            if (manager.SwitchPlan(planName))
            {
                CurrentPlanName = planName;
                nextTargetNumber = manager.Targets.Count > 0 
                    ? manager.Targets.Max(t => int.TryParse(t.Name.Replace("点", ""), out var n) ? n : 0) + 1 
                    : 1;
            }
        }

        public void RenamePlan(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (PlanNames.Contains(newName) && newName != CurrentPlanName) return;

            manager.RenamePlan(CurrentPlanName, newName);
            RefreshPlanNames();
            CurrentPlanName = newName;
        }

        private void RefreshTargets()
        {
            Targets.Clear();
            foreach (var target in manager.Targets)
            {
                Targets.Add(target);
            }
        }

        private void RefreshPlanNames()
        {
            PlanNames.Clear();
            foreach (var plan in manager.Plans)
            {
                PlanNames.Add(plan.Name);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
