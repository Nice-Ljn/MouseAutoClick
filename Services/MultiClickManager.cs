using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using MouseRecorderWpf.Models;

namespace MouseRecorderWpf.Services
{
    public class MultiClickManager
    {
        private List<MultiClickTarget> targets = new List<MultiClickTarget>();
        private List<MultiClickPlan> plans = new List<MultiClickPlan>();
        private MultiClickPlan? currentPlan;
        private bool isRunning = false;
        private CancellationTokenSource? cancellationTokenSource;
        private int nextId = 1;

        public event Action<int>? CurrentTargetChanged;
        public event Action? ClickCompleted;
        public event Action? PlanChanged;

        public List<MultiClickTarget> Targets => targets;

        public List<MultiClickPlan> Plans => plans;

        public MultiClickPlan? CurrentPlan => currentPlan;

        public bool IsRunning => isRunning;

        public MultiClickManager()
        {
            AddPlan("方案A");
        }

        public void AddPlan(string name)
        {
            plans.Add(new MultiClickPlan(name));
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
                currentPlan.Targets.Clear();
                currentPlan.Targets.AddRange(targets);
                currentPlan.RepeatCount = repeatCount;
                currentPlan.LoopInterval = loopInterval;
            }
        }

        private void LoadPlan(MultiClickPlan plan)
        {
            targets.Clear();
            foreach (var target in plan.Targets)
            {
                targets.Add(new MultiClickTarget(target.Position, target.ClickType, target.Delay, target.Name)
                {
                    Id = target.Id
                });
            }
            repeatCount = plan.RepeatCount;
            loopInterval = plan.LoopInterval;
            UpdateNextId();
        }

        private void UpdateNextId()
        {
            nextId = targets.Count > 0 ? targets.Max(t => t.Id) + 1 : 1;
        }

        private int repeatCount = 1;
        private int loopInterval = 1000;

        public int RepeatCount
        {
            get => repeatCount;
            set => repeatCount = value;
        }

        public int LoopInterval
        {
            get => loopInterval;
            set => loopInterval = value;
        }

        public void AddTarget(Point position, ClickType clickType, int delay, string name)
        {
            targets.Add(new MultiClickTarget(position, clickType, delay, name) { Id = nextId++ });
        }

        public void RemoveTarget(int id)
        {
            targets.RemoveAll(t => t.Id == id);
        }

        public void ClearTargets()
        {
            targets.Clear();
            nextId = 1;
        }

        public void SwapTargets(int index1, int index2)
        {
            if (index1 < 0 || index1 >= targets.Count || index2 < 0 || index2 >= targets.Count)
                return;

            var temp = targets[index1];
            targets[index1] = targets[index2];
            targets[index2] = temp;
        }

        public void UpdateTarget(int index, string name, Point position, ClickType clickType, int delay)
        {
            if (index < 0 || index >= targets.Count)
                return;

            targets[index].Name = name;
            targets[index].Position = position;
            targets[index].ClickType = clickType;
            targets[index].Delay = delay;
        }

        public void Start()
        {
            if (isRunning || targets.Count == 0)
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

                        if (repeatCount > 0 && loopIndex >= repeatCount)
                            break;

                        for (int j = 0; j < targets.Count; j++)
                        {
                            if (cancellationTokenSource.Token.IsCancellationRequested)
                                break;

                            var target = targets[j];

                            MouseSimulator.MoveMouse(target.Position.X, target.Position.Y);
                            await Task.Delay(50, cancellationTokenSource.Token);

                            switch (target.ClickType)
                            {
                                case ClickType.LeftClick:
                                    MouseSimulator.LeftClick();
                                    break;
                                case ClickType.RightClick:
                                    MouseSimulator.RightClick();
                                    break;
                                case ClickType.DoubleClick:
                                    MouseSimulator.DoubleClick();
                                    break;
                            }

                            CurrentTargetChanged?.Invoke(j);

                            if (j < targets.Count - 1)
                            {
                                await Task.Delay(target.Delay, cancellationTokenSource.Token);
                            }
                        }

                        loopIndex++;

                        bool hasNextLoop = repeatCount == 0 || loopIndex < repeatCount;
                        if (hasNextLoop && loopInterval > 0)
                        {
                            await Task.Delay(loopInterval, cancellationTokenSource.Token);
                        }
                    }
                }
                finally
                {
                    isRunning = false;
                    ClickCompleted?.Invoke();
                }
            });
        }

        public void Stop()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
