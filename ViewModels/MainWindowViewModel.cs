using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using MouseRecorderWpf.Views;
using UserControl = System.Windows.Controls.UserControl;

namespace MouseRecorderWpf.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private UserControl? _currentView;
        private int _activeTab;

        public UserControl? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public int ActiveTab
        {
            get => _activeTab;
            set
            {
                _activeTab = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            SwitchToRecorder();
        }

        public void SwitchToRecorder()
        {
            CurrentView = new RecorderControl();
            ActiveTab = 0;
        }

        public void SwitchToMultiClick()
        {
            CurrentView = new MultiClickControl();
            ActiveTab = 1;
        }

        public void SwitchToMixedAction()
        {
            CurrentView = new MixedActionControl();
            ActiveTab = 2;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
