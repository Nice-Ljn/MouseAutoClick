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

        public ObservableCollection<MultiClickTarget> Targets { get; } = new ObservableCollection<MultiClickTarget>();

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
            set { repeatCount = Math.Max(0, value); OnPropertyChanged(); }
        }

        public int LoopInterval
        {
            get => loopInterval;
            set { loopInterval = Math.Max(0, value); OnPropertyChanged(); }
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

        public bool IsRunning => manager.IsRunning;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MultiClickViewModel()
        {
            manager = new MultiClickManager();
            manager.CurrentTargetChanged += OnCurrentTargetChanged;
            manager.ClickCompleted += OnClickCompleted;

            positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            positionTimer.Tick += OnPositionTimerTick;
            positionTimer.Start();
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
            manager.Start(RepeatCount, LoopInterval);
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

        private void RefreshTargets()
        {
            Targets.Clear();
            foreach (var target in manager.Targets)
            {
                Targets.Add(target);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
