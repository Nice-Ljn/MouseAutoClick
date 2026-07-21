using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MouseRecorderWpf.Models;

namespace MouseRecorderWpf.Services
{
    public class MixedActionManager
    {
        private ObservableCollection<MixedAction> actions;
        private List<MixedActionPlan> plans = new List<MixedActionPlan>();
        private MixedActionPlan? currentPlan;
        private bool isRunning = false;
        private bool isRecording = false;
        private CancellationTokenSource? cancellationTokenSource;
        private int nextId = 1;
        private int nextActionNumber = 1;

        public event Action<int>? CurrentActionChanged;
        public event Action? ActionCompleted;
        public event Action<bool>? RecordingStateChanged;
        public event Action? PlanChanged;

        public ObservableCollection<MixedAction> Actions => actions;

        public List<MixedActionPlan> Plans => plans;

        public MixedActionPlan? CurrentPlan => currentPlan;

        public bool IsRunning => isRunning;

        public bool IsRecording => isRecording;

        public int RepeatCount { get; set; } = 1;

        public int LoopInterval { get; set; } = 1000;

        public MixedActionManager(ObservableCollection<MixedAction> actions)
        {
            this.actions = actions;
            AddPlan("方案A");
        }

        public void AddPlan(string name)
        {
            plans.Add(new MixedActionPlan(name));
            if (currentPlan == null)
            {
                SwitchPlan(name);
            }
        }

        public bool SwitchPlan(string planName)
        {
            if (isRunning)
                return false;

            var plan = plans.Find(p => p.Name == planName);
            if (plan == null)
                return false;

            SaveCurrentPlan();
            currentPlan = plan;
            LoadPlan(plan);
            PlanChanged?.Invoke();
            return true;
        }

        public void DeletePlan(string planName)
        {
            if (isRunning)
                return;

            if (plans.Count <= 1)
                return;

            var plan = plans.Find(p => p.Name == planName);
            if (plan == null)
                return;

            plans.Remove(plan);

            if (currentPlan?.Name == planName)
            {
                SwitchPlan(plans[0].Name);
            }
        }

        public void RenamePlan(string oldName, string newName)
        {
            var plan = plans.Find(p => p.Name == oldName);
            if (plan != null)
            {
                plan.Name = newName;
                if (currentPlan == plan)
                {
                    PlanChanged?.Invoke();
                }
            }
        }

        private void SaveCurrentPlan()
        {
            if (currentPlan != null)
            {
                currentPlan.Actions.Clear();
                foreach (var action in actions)
                {
                    currentPlan.Actions.Add(new MixedAction(action.ActionType, action.Position, action.KeyName, action.Delay, action.Name)
                    {
                        Id = action.Id
                    });
                }
                currentPlan.RepeatCount = RepeatCount;
                currentPlan.LoopInterval = LoopInterval;
            }
        }

        private void LoadPlan(MixedActionPlan plan)
        {
            actions.Clear();
            foreach (var action in plan.Actions)
            {
                // 分配新的唯一 ID，避免导入后 ID 冲突
                actions.Add(new MixedAction(action.ActionType, action.Position, action.KeyName, action.Delay, action.Name)
                {
                    Id = nextId++
                });
            }
            RepeatCount = plan.RepeatCount;
            LoopInterval = plan.LoopInterval;
            nextActionNumber = actions.Count + 1;
        }

        private void UpdateNextId()
        {
            nextId = actions.Count > 0 ? actions.Max(a => a.Id) + 1 : 1;
            nextActionNumber = actions.Count + 1;
        }

        public void AddMouseAction(MixedActionType actionType, Point position, int delay, string name)
        {
            actions.Add(new MixedAction(actionType, position, "", delay, name) { Id = nextId++ });
        }

        public void AddKeyboardAction(MixedActionType actionType, string keyName, int delay, string name)
        {
            actions.Add(new MixedAction(actionType, new Point(), keyName, delay, name) { Id = nextId++ });
        }

        public void RemoveAction(int id)
        {
            var item = actions.FirstOrDefault(a => a.Id == id);
            if (item != null)
            {
                actions.Remove(item);
            }
        }

        public void ClearActions()
        {
            actions.Clear();
            nextId = 1;
            nextActionNumber = 1;
        }

        public void StartRecording()
        {
            if (isRecording || isRunning) return;
            isRecording = true;
            RecordingStateChanged?.Invoke(true);
        }

        public void StopRecording()
        {
            if (!isRecording) return;
            isRecording = false;
            RecordingStateChanged?.Invoke(false);
        }

        public void RecordKeyboardAction(MixedActionType actionType, string keyName)
        {
            var actionName = $"动作{nextActionNumber}";
            actions.Add(new MixedAction(actionType, new Point(), keyName, 50, actionName) { Id = nextId++ });
            nextActionNumber++;
        }

        public void RecordMouseAction(MixedActionType actionType, Point position)
        {
            var actionName = $"动作{nextActionNumber}";
            actions.Add(new MixedAction(actionType, position, "", 50, actionName) { Id = nextId++ });
            nextActionNumber++;
        }

        public void SwapActions(int index1, int index2)
        {
            if (index1 < 0 || index1 >= actions.Count || index2 < 0 || index2 >= actions.Count)
                return;

            // 交换 ObservableCollection 中的对象位置
            var temp = actions[index1];
            actions[index1] = actions[index2];
            actions[index2] = temp;
        }

        public void UpdateAction(int index, string name, Point position, string keyName, MixedActionType actionType, int delay)
        {
            if (index < 0 || index >= actions.Count)
                return;

            actions[index].Name = name;
            actions[index].Position = position;
            actions[index].KeyName = keyName;
            actions[index].ActionType = actionType;
            actions[index].Delay = delay;
        }

        public void Start()
        {
            if (isRunning || actions.Count == 0)
                return;

            isRunning = true;
            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    int loopIndex = 0;
                    while (true)
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                            break;

                        if (RepeatCount > 0 && loopIndex >= RepeatCount)
                            break;

                        for (int j = 0; j < actions.Count; j++)
                        {
                            if (cancellationTokenSource.Token.IsCancellationRequested)
                                break;

                            var action = actions[j];

                            if (action.IsMouseAction)
                            {
                                MouseSimulator.MoveMouse(action.Position.X, action.Position.Y);
                                await Task.Delay(50, cancellationTokenSource.Token);

                                switch (action.ActionType)
                                {
                                    case MixedActionType.MouseLeftClick:
                                        MouseSimulator.LeftClick();
                                        break;
                                    case MixedActionType.MouseRightClick:
                                        MouseSimulator.RightClick();
                                        break;
                                    case MixedActionType.MouseDoubleClick:
                                        MouseSimulator.DoubleClick();
                                        break;
                                    case MixedActionType.MouseMiddleClick:
                                        MouseSimulator.MiddleClick();
                                        break;
                                }
                            }
                            else
                            {
                                switch (action.ActionType)
                                {
                                    case MixedActionType.KeyboardKeyDown:
                                        KeyboardSimulator.KeyDown(action.KeyName);
                                        break;
                                    case MixedActionType.KeyboardKeyUp:
                                        KeyboardSimulator.KeyUp(action.KeyName);
                                        break;
                                    case MixedActionType.KeyboardKeyPress:
                                        KeyboardSimulator.KeyPress(action.KeyName);
                                        break;
                                }
                            }

                            CurrentActionChanged?.Invoke(j);

                            if (j < actions.Count - 1 && action.Delay > 0)
                            {
                                await Task.Delay(action.Delay, cancellationTokenSource.Token);
                            }
                        }

                        loopIndex++;

                        bool hasNextLoop = RepeatCount == 0 || loopIndex < RepeatCount;
                        if (hasNextLoop && LoopInterval > 0)
                        {
                            await Task.Delay(LoopInterval, cancellationTokenSource.Token);
                        }
                    }
                }
                finally
                {
                    isRunning = false;
                    ActionCompleted?.Invoke();
                }
            });
        }

        public void Stop()
        {
            cancellationTokenSource?.Cancel();
        }

        // 导入导出功能
        public string ExportPlan(string planName)
        {
            var plan = plans.Find(p => p.Name == planName);
            if (plan == null) return string.Empty;

            // 如果导出的是当前方案，先保存当前操作列表
            if (plan == currentPlan)
            {
                SaveCurrentPlan();
            }

            var exportData = new MixedActionPlanExport
            {
                Name = plan.Name,
                RepeatCount = plan.RepeatCount,
                LoopInterval = plan.LoopInterval,
                Actions = new List<MixedActionExport>()
            };

            foreach (var action in plan.Actions)
            {
                exportData.Actions.Add(new MixedActionExport
                {
                    ActionType = (int)action.ActionType,
                    PositionX = action.Position.X,
                    PositionY = action.Position.Y,
                    KeyName = action.KeyName,
                    Delay = action.Delay,
                    Name = action.Name
                });
            }

            return JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        public bool ImportPlan(string json)
        {
            try
            {
                var exportData = JsonSerializer.Deserialize<MixedActionPlanExport>(json);
                if (exportData == null || string.IsNullOrEmpty(exportData.Name))
                    return false;

                // 检查名称是否重复
                if (plans.Any(p => p.Name == exportData.Name))
                {
                    exportData.Name = exportData.Name + "_导入";
                }

                var actionsList = new List<MixedAction>();
                foreach (var actionData in exportData.Actions)
                {
                    actionsList.Add(new MixedAction(
                        (MixedActionType)actionData.ActionType,
                        new Point(actionData.PositionX, actionData.PositionY),
                        actionData.KeyName ?? "",
                        actionData.Delay,
                        actionData.Name ?? ""
                    ));
                }

                var plan = new MixedActionPlan(exportData.Name, actionsList, exportData.RepeatCount, exportData.LoopInterval);
                plans.Add(plan);

                // 切换到新导入的方案
                currentPlan = plan;
                LoadPlan(plan);

                PlanChanged?.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class MixedActionPlanExport
    {
        public string Name { get; set; } = "";
        public int RepeatCount { get; set; }
        public int LoopInterval { get; set; }
        public List<MixedActionExport> Actions { get; set; } = new List<MixedActionExport>();
    }

    public class MixedActionExport
    {
        public int ActionType { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string? KeyName { get; set; }
        public int Delay { get; set; }
        public string? Name { get; set; }
    }
}