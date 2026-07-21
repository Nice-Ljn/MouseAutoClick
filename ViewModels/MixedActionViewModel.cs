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
    public class MixedActionViewModel : INotifyPropertyChanged
    {
        private readonly MixedActionManager manager;
        private readonly DispatcherTimer positionTimer;
        private bool isRecording = false;

        private string statusText = "就绪";
        private string currentPosition = "(0, 0)";
        private int repeatCount = 1;
        private int loopInterval = 1000;
        private MixedAction? selectedAction;
        private int selectedIndex = -1;
        private string currentPlanName = "方案A";
        private int planNumber = 1;

        public ObservableCollection<MixedAction> Actions { get; } = new ObservableCollection<MixedAction>();
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

        public MixedAction? SelectedAction
        {
            get => selectedAction;
            set { selectedAction = value; OnPropertyChanged(); }
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

        public bool IsRecording => isRecording;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MixedActionViewModel()
        {
            manager = new MixedActionManager(Actions);
            manager.CurrentActionChanged += OnCurrentActionChanged;
            manager.ActionCompleted += OnActionCompleted;
            manager.RecordingStateChanged += OnRecordingStateChanged;
            manager.PlanChanged += OnPlanChanged;

            HotkeyRegistry.GlobalKeyEvent += OnGlobalKeyEvent;

            positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            positionTimer.Tick += OnPositionTimerTick;
            positionTimer.Start();

            RefreshPlanNames();
        }

        private void OnGlobalKeyEvent(int keyCode, bool isKeyDown)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                HandleKeyPress(keyCode, isKeyDown);
            });
        }

        private void OnPositionTimerTick(object? sender, EventArgs e)
        {
            var pos = MouseSimulator.GetCurrentPosition();
            CurrentPosition = $"({pos.X}, {pos.Y})";
        }

        private void OnCurrentActionChanged(int index)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                SelectedIndex = index;
            });
        }

        private void OnActionCompleted()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusText = "就绪";
                OnPropertyChanged(nameof(IsRunning));
            });
        }

        private void OnRecordingStateChanged(bool recordingState)
        {
        }

        private void OnPlanChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CurrentPlanName = manager.CurrentPlan?.Name ?? "方案A";
                RepeatCount = manager.RepeatCount;
                LoopInterval = manager.LoopInterval;
                // Actions 已由 manager 直接操作，无需刷新
            });
        }

        public void HandleKeyPress(int keyCode, bool isKeyDown)
        {
            var keyName = KeyboardHook.GetKeyName(keyCode);
            if (string.IsNullOrEmpty(keyName)) return;

            // 检查当前是否在混合录制页面
            var mainWindow = System.Windows.Application.Current?.MainWindow as Views.MainWindow;
            bool isMixedActionTab = mainWindow != null && ((ViewModels.MainWindowViewModel)mainWindow.DataContext).ActiveTab == 2;

            if (isMixedActionTab && keyName == "F4" && isKeyDown)
            {
                RecordMouseClick();
            }
            else if (isMixedActionTab && keyName == "F6" && isKeyDown)
            {
                if (IsRecording)
                {
                    StopRecording();
                }
                else if (IsRunning)
                {
                    Stop();
                }
            }
            else if (isMixedActionTab && IsRecording && keyName != "F4" && keyName != "F5" && keyName != "F6" && keyName != "F7")
            {
                if (isKeyDown)
                {
                    RecordKeyboardAction(MixedActionType.KeyboardKeyDown, keyName);
                }
                else
                {
                    RecordKeyboardAction(MixedActionType.KeyboardKeyUp, keyName);
                }
            }
        }

        private void RecordMouseClick()
        {
            var pos = MouseSimulator.GetCurrentPosition();
            manager.RecordMouseAction(MixedActionType.MouseLeftClick, pos);
        }

        private void RecordKeyboardKeyPress(string keyName)
        {
            manager.RecordKeyboardAction(MixedActionType.KeyboardKeyPress, keyName);
        }

        private void RecordKeyboardAction(MixedActionType actionType, string keyName)
        {
            manager.RecordKeyboardAction(actionType, keyName);
        }

        public void AddMouseAction(MixedActionType actionType)
        {
            var pos = MouseSimulator.GetCurrentPosition();
            manager.RecordMouseAction(actionType, pos);
        }

        public void AddKeyboardAction(MixedActionType actionType, string keyName)
        {
            manager.RecordKeyboardAction(actionType, keyName);
        }

        public void RemoveSelected()
        {
            if (SelectedAction == null) return;
            manager.RemoveAction(SelectedAction.Id);
        }

        public void ClearAll()
        {
            manager.ClearActions();
        }

        public void MoveUp()
        {
            if (SelectedIndex <= 0) return;
            manager.SwapActions(SelectedIndex, SelectedIndex - 1);
            SelectedIndex--;
        }

        public void MoveDown()
        {
            if (SelectedIndex < 0 || SelectedIndex >= Actions.Count - 1) return;
            manager.SwapActions(SelectedIndex, SelectedIndex + 1);
            SelectedIndex++;
        }

        public void StartRecording()
        {
            if (manager.IsRunning) return;
            manager.StartRecording();
            isRecording = true;
            OnPropertyChanged(nameof(IsRecording));
            StatusText = "录制中... (按 F4 添加鼠标点击, F6 停止录制)";
        }

        public void StopRecording()
        {
            if (!isRecording) return;
            manager.StopRecording();
            isRecording = false;
            OnPropertyChanged(nameof(IsRecording));
            StatusText = "就绪";
        }

        public void Start()
        {
            if (manager.IsRunning || manager.Actions.Count == 0) return;
            StatusText = "运行中... (按 F6 停止)";
            OnPropertyChanged(nameof(IsRunning));
            manager.Start();
        }

        public void Stop()
        {
            if (!manager.IsRunning) return;
            manager.Stop();
        }

        public void RemoveSelectedItems(List<MixedAction> items)
        {
            if (items == null || items.Count == 0) return;
            foreach (var item in items)
            {
                manager.RemoveAction(item.Id);
            }
        }

        // 方案管理
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

        public string ExportCurrentPlan()
        {
            return manager.ExportPlan(CurrentPlanName);
        }

        public bool ImportPlan(string json)
        {
            return manager.ImportPlan(json);
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