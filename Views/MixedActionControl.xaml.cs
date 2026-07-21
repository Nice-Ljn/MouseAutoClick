using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MouseRecorderWpf.Models;
using MouseRecorderWpf.ViewModels;

namespace MouseRecorderWpf.Views
{
    public partial class MixedActionControl : System.Windows.Controls.UserControl
    {
        private MixedActionViewModel ViewModel => (MixedActionViewModel)DataContext;

        public MixedActionControl()
        {
            InitializeComponent();
            DataContext = new MixedActionViewModel();
        }

        public void BtnAddMouseLeft_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddMouseAction(MixedActionType.MouseLeftClick);
        }

        public void BtnAddMouseRight_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddMouseAction(MixedActionType.MouseRightClick);
        }

        public void BtnAddMouseDouble_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddMouseAction(MixedActionType.MouseDoubleClick);
        }

        public void BtnAddMouseMiddle_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddMouseAction(MixedActionType.MouseMiddleClick);
        }

        public void BtnAddKeyPress_Click(object sender, RoutedEventArgs e)
        {
        }

        public void BtnAddKeyDown_Click(object sender, RoutedEventArgs e)
        {
        }

        public void BtnAddKeyUp_Click(object sender, RoutedEventArgs e)
        {
        }

        public void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveUp();
        }

        public void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveDown();
        }

        public void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = new List<MouseRecorderWpf.Models.MixedAction>();
            foreach (var item in ActionGrid.SelectedItems)
            {
                if (item is MouseRecorderWpf.Models.MixedAction action)
                {
                    selectedItems.Add(action);
                }
            }
            ViewModel.RemoveSelectedItems(selectedItems);
        }

        public void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearAll();
        }

        public void BtnToggleRecording_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsRecording)
            {
                ViewModel.StopRecording();
            }
            else
            {
                ViewModel.StartRecording();
            }
        }

        public void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Start();
        }

        public void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Stop();
        }

        private void PlanComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var planName = e.AddedItems[0] as string;
                if (!string.IsNullOrEmpty(planName))
                {
                    ViewModel.SwitchPlan(planName);
                }
            }
        }

        private void BtnAddPlan_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddPlan();
        }

        private void BtnDeletePlan_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.PlanNames.Count <= 1)
            {
                System.Windows.MessageBox.Show("至少需要保留一个方案", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ViewModel.DeletePlan();
        }

        private void BtnRenamePlan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("请输入新的方案名称", ViewModel.CurrentPlanName);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                var newName = dialog.InputValue;
                if (string.IsNullOrEmpty(newName))
                {
                    System.Windows.MessageBox.Show("请输入方案名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                ViewModel.RenamePlan(newName);
            }
        }

        private void BtnExportPlan_Click(object sender, RoutedEventArgs e)
        {
            var json = ViewModel.ExportCurrentPlan();
            if (string.IsNullOrEmpty(json))
            {
                System.Windows.MessageBox.Show("导出失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json",
                FileName = $"{ViewModel.CurrentPlanName}.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, json);
                System.Windows.MessageBox.Show($"方案已导出到:\n{dialog.FileName}", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnImportPlan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                var json = File.ReadAllText(dialog.FileName);
                if (ViewModel.ImportPlan(json))
                {
                    System.Windows.MessageBox.Show("方案导入成功", "导入成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("导入失败，文件格式不正确", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}