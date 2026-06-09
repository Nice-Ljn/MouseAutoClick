using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MouseRecorderWpf.Models;
using MouseRecorderWpf.Services;
using MouseAction = MouseRecorderWpf.Models.MouseAction;

namespace MouseRecorderWpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly LowLevelMouseHook mouseHook;
        private CancellationTokenSource? playbackCts;

        private bool isRecording;
        private bool isPlaying;
        private string statusText = "就绪";
        private int actionCount;
        private int repeatCount = 1;
        private double speedMultiplier = 1.0;
        private Script? activeScript;
        private bool isRecordingNewScript = false;

        public ObservableCollection<Script> Scripts { get; } = new ObservableCollection<Script>();

        public ObservableCollection<MouseAction> RecordedActions { get; } = new ObservableCollection<MouseAction>();

        public bool IsRecording
        {
            get => isRecording;
            set { isRecording = value; OnPropertyChanged(); }
        }

        public bool IsPlaying
        {
            get => isPlaying;
            set { isPlaying = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => statusText;
            set { statusText = value; OnPropertyChanged(); }
        }

        public int ActionCount
        {
            get => actionCount;
            set { actionCount = value; OnPropertyChanged(); }
        }

        public int RepeatCount
        {
            get => repeatCount;
            set { repeatCount = Math.Max(1, value); OnPropertyChanged(); }
        }

        public double SpeedMultiplier
        {
            get => speedMultiplier;
            set
            {
                // 限制范围：0.1 - 100倍
                speedMultiplier = Math.Max(0.1, Math.Min(100, value));
                OnPropertyChanged();
            }
        }

        public Script? ActiveScript
        {
            get => activeScript;
            set
            {
                activeScript = value;
                OnPropertyChanged();
                if (value != null)
                {
                    LoadScriptActions(value);
                    DataStorageService.SaveActiveScriptName(value.Name);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            mouseHook = new LowLevelMouseHook();
            mouseHook.MouseAction += OnMouseAction;
            LoadScripts();
        }

        private void LoadScripts()
        {
            var savedScripts = DataStorageService.LoadAllScripts();
            var sortedScripts = savedScripts.OrderBy(s => s.CreatedAt).ToList();
            foreach (var script in sortedScripts)
            {
                Scripts.Add(script);
            }

            var activeScriptName = DataStorageService.GetActiveScriptName();
            if (!string.IsNullOrEmpty(activeScriptName))
            {
                var script = Scripts.FirstOrDefault(s => s.Name == activeScriptName);
                if (script != null)
                {
                    ActiveScript = script;
                }
            }

            if (ActiveScript == null && Scripts.Count > 0)
            {
                ActiveScript = Scripts[0];
            }
            // 不要自动创建空脚本
        }

        private void LoadScriptActions(Script script)
        {
            RecordedActions.Clear();
            foreach (var action in script.Actions)
            {
                RecordedActions.Add(action);
            }
            ActionCount = RecordedActions.Count;
            StatusText = "就绪";
        }

        public void CreateNewScript()
        {
            if (IsRecording || IsPlaying) return;
            var script = new Script($"脚本 {Scripts.Count + 1}");
            InsertScriptInOrder(script);
            ActiveScript = script;
            DataStorageService.SaveScript(script);
        }

        private void InsertScriptInOrder(Script script)
        {
            int index = 0;
            while (index < Scripts.Count && Scripts[index].CreatedAt < script.CreatedAt)
            {
                index++;
            }
            Scripts.Insert(index, script);
        }

        public void ExportScript(string filePath)
        {
            if (ActiveScript == null) throw new Exception("没有可导出的脚本");
            DataStorageService.ExportScript(ActiveScript, filePath);
        }

        public void ImportScript(string filePath)
        {
            var script = DataStorageService.ImportScript(filePath);
            if (script != null)
            {
                InsertScriptInOrder(script);
                ActiveScript = script;
            }
        }

        public void DeleteScript(Script script)
        {
            if (IsRecording || IsPlaying) return;
            DataStorageService.DeleteScript(script);
            Scripts.Remove(script);
            if (ActiveScript == script)
            {
                if (Scripts.Count > 0)
                {
                    ActiveScript = Scripts[0];
                }
                else
                {
                    CreateNewScript();
                }
            }
        }

        public void RenameScript(Script script, string newName)
        {
            if (IsRecording || IsPlaying) return;
            DataStorageService.DeleteScript(script);
            script.Name = newName;
            DataStorageService.SaveScript(script);
            if (ActiveScript == script)
            {
                DataStorageService.SaveActiveScriptName(newName);
            }
        }

        private void OnMouseAction(object? sender, MouseAction e)
        {
            if (!IsRecording) return;

            RecordedActions.Add(e);
            ActionCount = RecordedActions.Count;
        }

        public void StartRecording()
        {
            if (IsPlaying) return;
            isRecordingNewScript = true;
            RecordedActions.Clear();
            ActionCount = 0;
            IsRecording = true;
            mouseHook.Start();
            StatusText = "录制中...";
        }

        public void StopRecording()
        {
            if (!IsRecording) return;
            IsRecording = false;
            mouseHook.Stop();

            if (isRecordingNewScript && RecordedActions.Count > 0)
            {
                var newScript = new Script($"脚本 {Scripts.Count + 1}");
                newScript.Actions = new List<MouseAction>(RecordedActions);
                InsertScriptInOrder(newScript);
                ActiveScript = newScript;
                DataStorageService.SaveScript(newScript);
                isRecordingNewScript = false;
                StatusText = $"已保存 {ActionCount} 个操作";
            }
            else
            {
                if (ActiveScript != null)
                {
                    ActiveScript.Actions = new List<MouseAction>(RecordedActions);
                    DataStorageService.SaveScript(ActiveScript);
                    StatusText = "录制已停止";
                }
            }
        }

        public async Task StartPlayback()
        {
            if (IsRecording || IsPlaying || RecordedActions.Count == 0) return;

            IsPlaying = true;
            StatusText = "回放中...";
            playbackCts = new CancellationTokenSource();

            try
            {
                for (int i = 0; i < RepeatCount; i++)
                {
                    await PlaybackActions(playbackCts.Token);
                    if (playbackCts.IsCancellationRequested) break;
                }
            }
            catch (OperationCanceledException)
            {
                // 用户取消是正常情况，不需要抛出异常
            }
            finally
            {
                IsPlaying = false;
                StatusText = "就绪";
            }
        }

        private async Task PlaybackActions(CancellationToken ct)
        {
            for (int i = 0; i < RecordedActions.Count; i++)
            {
                if (ct.IsCancellationRequested) break;

                var action = RecordedActions[i];
                var nextAction = i < RecordedActions.Count - 1 ? RecordedActions[i + 1] : null;

                MouseSimulator.PerformAction(action);

                if (nextAction != null)
                {
                    var delay = (nextAction.Timestamp - action.Timestamp).TotalMilliseconds / SpeedMultiplier;
                    if (delay > 0)
                    {
                        try
                        {
                            await Task.Delay((int)Math.Min(delay, 1000), ct);
                        }
                        catch (OperationCanceledException)
                        {
                            // 取消是正常情况，安全地退出
                            break;
                        }
                    }
                }
            }
        }

        public void StopPlayback()
        {
            if (!IsPlaying) return;
            playbackCts?.Cancel();
        }

        public void ClearActions()
        {
            if (IsRecording || IsPlaying) return;
            RecordedActions.Clear();
            ActionCount = 0;
            if (ActiveScript != null)
            {
                ActiveScript.Actions.Clear();
                DataStorageService.SaveScript(ActiveScript);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
