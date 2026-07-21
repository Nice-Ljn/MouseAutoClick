using System;
using System.Windows;
using MouseRecorderWpf.ViewModels;
using MouseRecorderWpf.Services;

namespace MouseRecorderWpf.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;
        private Action _emptyAction = () => { };

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            HotkeyRegistry.Register(112, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 0)
                {
                    if (ViewModel.CurrentView is RecorderControl recorder)
                    {
                        recorder.BtnRecord_Click(null, null);
                    }
                }
            }));
            HotkeyRegistry.Register(113, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 0)
                {
                    if (ViewModel.CurrentView is RecorderControl recorder)
                    {
                        recorder.BtnPlay_Click(null, null);
                    }
                }
            }));
            HotkeyRegistry.Register(114, () => Dispatcher.Invoke(() => {
                if (ViewModel.CurrentView is RecorderControl recorder)
                {
                    recorder.BtnStop_Click(null, null);
                }
                if (ViewModel.CurrentView is MultiClickControl multiClick)
                {
                    multiClick.BtnStop_Click(null, null);
                }
            }));
            HotkeyRegistry.Register(115, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 1)
                {
                    if (ViewModel.CurrentView is MultiClickControl multiClick)
                    {
                        multiClick.BtnAdd_Click(null, null);
                    }
                }
            }));
            HotkeyRegistry.Register(116, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 1)
                {
                    if (ViewModel.CurrentView is MultiClickControl multiClick)
                    {
                        multiClick.BtnStart_Click(null, null);
                    }
                }
            }));
            HotkeyRegistry.Register(117, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 1)
                {
                    if (ViewModel.CurrentView is MultiClickControl multiClick)
                    {
                        multiClick.BtnStop_Click(null, null);
                    }
                }
                if (ViewModel.ActiveTab == 2)
                {
                    if (ViewModel.CurrentView is MixedActionControl mixedAction)
                    {
                        mixedAction.BtnStop_Click(null, null);
                    }
                }
            }));
            HotkeyRegistry.Register(116, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 2)
                {
                    if (ViewModel.CurrentView is MixedActionControl mixedAction)
                    {
                        mixedAction.BtnStart_Click(null, null);
                    }
                }
            }));
            HotkeyRegistry.Register(118, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 2)
                {
                    if (ViewModel.CurrentView is MixedActionControl mixedAction)
                    {
                        mixedAction.BtnToggleRecording_Click(null, null);
                    }
                }
            }));
            HotkeyRegistry.Register(115, () => Dispatcher.Invoke(() => {
                if (ViewModel.ActiveTab == 2)
                {
                    if (ViewModel.CurrentView is MixedActionControl mixedAction)
                    {
                        mixedAction.BtnAddMouseLeft_Click(null, null);
                    }
                }
            }));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            HotkeyRegistry.Unregister(112, _emptyAction);
            HotkeyRegistry.Unregister(113, _emptyAction);
            HotkeyRegistry.Unregister(114, _emptyAction);
            HotkeyRegistry.Unregister(115, _emptyAction);
            HotkeyRegistry.Unregister(116, _emptyAction);
            HotkeyRegistry.Unregister(117, _emptyAction);
            HotkeyRegistry.Unregister(118, _emptyAction);
        }

        private void BtnRecorder_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchToRecorder();
            UpdateButtonStyles();
        }

        private void BtnMultiClick_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchToMultiClick();
            UpdateButtonStyles();
        }

        private void BtnMixedAction_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchToMixedAction();
            UpdateButtonStyles();
        }

        private void UpdateButtonStyles()
        {
            BtnRecorder.Style = ViewModel.ActiveTab == 0 
                ? (Style)FindResource("SidebarActiveButton") 
                : (Style)FindResource("SidebarButton");
            BtnMultiClick.Style = ViewModel.ActiveTab == 1 
                ? (Style)FindResource("SidebarActiveButton") 
                : (Style)FindResource("SidebarButton");
            BtnMixedAction.Style = ViewModel.ActiveTab == 2 
                ? (Style)FindResource("SidebarActiveButton") 
                : (Style)FindResource("SidebarButton");
        }
    }
}
