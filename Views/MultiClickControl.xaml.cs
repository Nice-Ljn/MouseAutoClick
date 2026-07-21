using System.Windows;
using System.Windows.Controls;
using MouseRecorderWpf.ViewModels;

namespace MouseRecorderWpf.Views
{
    public partial class MultiClickControl : System.Windows.Controls.UserControl
    {
        private MultiClickViewModel ViewModel => (MultiClickViewModel)DataContext;

        public MultiClickControl()
        {
            InitializeComponent();
            DataContext = new MultiClickViewModel();
        }

        public void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddCurrentPosition();
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
            ViewModel.RemoveSelected();
        }

        public void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearAll();
        }

        public void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Start();
        }

        public void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Stop();
        }

        private void TargetGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                ViewModel.SyncTargetFromGrid(ViewModel.SelectedIndex);
            }
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
    }
}
