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
    }
}
