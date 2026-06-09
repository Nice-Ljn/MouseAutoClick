using System.Windows.Controls;
using System.Windows;
using MouseRecorderWpf.ViewModels;
using MouseRecorderWpf.Models;
using UserControl = System.Windows.Controls.UserControl;
using TextBox = System.Windows.Controls.TextBox;
using Button = System.Windows.Controls.Button;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = System.Windows.Controls.TextBlock;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Thickness = System.Windows.Thickness;

namespace MouseRecorderWpf.Views
{
    public partial class RecorderControl : UserControl
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public RecorderControl()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        public void BtnRecord_Click(object sender, RoutedEventArgs e)
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

        public async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsPlaying)
            {
                ViewModel.StopPlayback();
            }
            else
            {
                await ViewModel.StartPlayback();
            }
        }

        public void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsRecording)
            {
                ViewModel.StopRecording();
            }
            if (ViewModel.IsPlaying)
            {
                ViewModel.StopPlayback();
            }
        }

        public void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearActions();
        }

        public void BtnRenameScript_Click(object sender, RoutedEventArgs e)
        {
            ShowRenameDialog();
        }

        public void ScriptItem_DoubleClick(object sender, RoutedEventArgs e)
        {
            ShowRenameDialog();
        }

        private void ShowRenameDialog()
        {
            if (ViewModel.ActiveScript != null)
            {
                var inputWindow = new System.Windows.Window
                {
                    Title = "重命名脚本",
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = System.Windows.Application.Current.MainWindow,
                    ResizeMode = ResizeMode.NoResize,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xFF, 0xFF))
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                var label = new TextBlock
                {
                    Text = "请输入新的脚本名称:",
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x60, 0x62, 0x66))
                };

                var textBox = new TextBox
                {
                    Text = ViewModel.ActiveScript.Name,
                    Height = 35,
                    Padding = new Thickness(10, 0, 10, 0),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                textBox.SelectAll();

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var cancelButton = new Button
                {
                    Content = "取消",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(0, 0, 10, 0),
                    Style = (Style)System.Windows.Application.Current.Resources["DefaultButton"]
                };
                cancelButton.Click += (s, args) => inputWindow.Close();

                var confirmButton = new Button
                {
                    Content = "确定",
                    Width = 80,
                    Height = 30,
                    Style = (Style)System.Windows.Application.Current.Resources["PrimaryButton"]
                };

                confirmButton.Click += (s, args) =>
                {
                    var newName = textBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        ViewModel.RenameScript(ViewModel.ActiveScript, newName);
                        inputWindow.Close();
                    }
                };

                buttonPanel.Children.Add(cancelButton);
                buttonPanel.Children.Add(confirmButton);
                stackPanel.Children.Add(label);
                stackPanel.Children.Add(textBox);
                stackPanel.Children.Add(buttonPanel);
                inputWindow.Content = stackPanel;
                inputWindow.Loaded += (s, args) => textBox.Focus();

                inputWindow.ShowDialog();
            }
        }

        public void BtnDeleteScript_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ActiveScript != null)
            {
                var result = System.Windows.MessageBox.Show(
                    $"确定要删除脚本 \"{ViewModel.ActiveScript.Name}\"吗?",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    ViewModel.DeleteScript(ViewModel.ActiveScript);
                }
            }
        }

        public void BtnExportScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "脚本文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    FileName = ViewModel.ActiveScript?.Name ?? "脚本",
                    DefaultExt = ".json",
                    Title = "导出脚本"
                };
                if (dialog.ShowDialog() == true)
                {
                    ViewModel.ExportScript(dialog.FileName);
                    System.Windows.MessageBox.Show("导出成功!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void BtnImportScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "脚本文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                    DefaultExt = ".json",
                    Title = "导入脚本"
                };
                if (dialog.ShowDialog() == true)
                {
                    ViewModel.ImportScript(dialog.FileName);
                    System.Windows.MessageBox.Show("导入成功!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
