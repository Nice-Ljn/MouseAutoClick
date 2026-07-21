using System.Windows;

namespace MouseRecorderWpf.Views
{
    public partial class InputDialog : Window
    {
        public string InputValue { get; private set; } = "";

        public InputDialog(string title, string defaultValue = "")
        {
            InitializeComponent();
            TitleText.Text = title;
            InputTextBox.Text = defaultValue;
            Loaded += (s, e) =>
            {
                InputTextBox.Focus();
                InputTextBox.SelectAll();
            };
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            InputValue = InputTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnOk_Click(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                BtnCancel_Click(sender, e);
            }
        }
    }
}
