using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EOL.Views
{
    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        public string Password { get; private set; }

        // Make these configurable if you like
        public int MaxAttempts { get; set; } = 5;
        private int _attemptsLeft;

        // Optional: inject your own validator from caller
        public Func<string, bool> Validate { get; set; }

        public PasswordWindow()
        {
            InitializeComponent();

            // Default validator (replace or inject from caller)
            Validate = p => p == "2512";

            Loaded += (_, __) =>
            {
                _attemptsLeft = MaxAttempts;
                UpdateAttemptsText();
                PasswordBox.Focus();
            };
        }
        private void ShowInline(string text, bool isError = true)
        {
            InlineMessage.Text = text ?? "";
            InlineMessage.Foreground = isError ? Brushes.IndianRed : Brushes.Gray;
            InlineMessage.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateAttemptsText()
        {
            if (MaxAttempts > 0)
            {
                AttemptsText.Text = $"Attempts left: {_attemptsLeft}/{MaxAttempts}";
                AttemptsText.Visibility = Visibility.Visible;
            }
            else
            {
                AttemptsText.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            // enable OK only when there is input; clear inline message while typing
            OkButton.IsEnabled = !string.IsNullOrWhiteSpace(PasswordBox.Password);
            if (InlineMessage.Visibility == Visibility.Visible)
                ShowInline(null);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var entered = PasswordBox.Password;

            // Block empty/whitespace (no attempt consumed)
            if (string.IsNullOrWhiteSpace(entered))
            {
                ShowInline("Please enter a password.");
                PasswordBox.Focus();
                return;
            }

            if (Validate?.Invoke(entered) == true)
            {
                Password = entered;
                DialogResult = true;   // closes dialog with success
                return;
            }

            // Wrong password: show inline and decrement attempts
            _attemptsLeft--;
            if (_attemptsLeft <= 0 && MaxAttempts > 0)
            {
                ShowInline("Too many incorrect attempts.");
                DialogResult = false;  // closes dialog with failure
                return;
            }

            ShowInline($"Incorrect password. {_attemptsLeft} attempt{(_attemptsLeft == 1 ? "" : "s")} remaining.");
            PasswordBox.Clear();
            PasswordBox.Focus();
            UpdateAttemptsText();
            OkButton.IsEnabled = false;
        }
    }
}

