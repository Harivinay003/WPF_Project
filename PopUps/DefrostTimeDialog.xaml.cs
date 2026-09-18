using System;
using System.Windows;

namespace WPFSCADA.PopUps
{
    public partial class DefrostTimeDialog : Window
    {
        public string SelectedTime { get; private set; }

        public DefrostTimeDialog(string unitName, string title, string currentTime)
        {
            InitializeComponent();

            CycleTitleText.Text = $"{unitName} - {title}";

            if (!string.IsNullOrWhiteSpace(currentTime))
            {
                string[] parts = currentTime.Split(
                    new[] { ':', ' ' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    HoursTextBox.Text = parts[0];
                    MinutesTextBox.Text = parts[1];
                }
            }
        }
        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(HoursTextBox.Text, out int hours))
            {
                MessageBox.Show(
                    "Please enter a valid hour.",
                    "Invalid Time",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            if (!int.TryParse(MinutesTextBox.Text, out int minutes))
            {
                MessageBox.Show(
                    "Please enter valid minutes.",
                    "Invalid Time",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            if (hours < 0 || hours > 23)
            {
                MessageBox.Show(
                    "Hours must be between 0 and 23.",
                    "Invalid Time",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            if (minutes < 0 || minutes > 59)
            {
                MessageBox.Show(
                    "Minutes must be between 0 and 59.",
                    "Invalid Time",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            SelectedTime = $"{hours:00} : {minutes:00}";
            DialogResult = true;
        }
        private void CancelButton_Click( object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void CloseButton_Click(object sender,RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}