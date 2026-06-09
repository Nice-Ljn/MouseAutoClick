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
        private bool isRunning = false;
        private CancellationTokenSource? cancellationTokenSource;
        private int nextId = 1;

        public event Action<int>? CurrentTargetChanged;
        public event Action? ClickCompleted;

        public List<MultiClickTarget> Targets => targets;

        public bool IsRunning => isRunning;

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

        public void Start(int repeatCount, int loopInterval)
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

                        // 检查是否达到指定循环次数（repeatCount为0时表示无限循环）
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

                            // 目标点之间的延迟
                            if (j < targets.Count - 1)
                            {
                                await Task.Delay(target.Delay, cancellationTokenSource.Token);
                            }
                        }

                        loopIndex++;

                        // 循环之间的间隙（如果还有下一次循环）
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
