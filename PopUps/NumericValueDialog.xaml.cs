using System;
using System.Windows;

namespace WPFSCADA.PopUps
{
    public partial class NumericValueDialog : Window
    {
        public double SelectedValue { get; private set; }

        private readonly double _currentValue;
        public NumericValueDialog(
    string unitName,
    string title,
    double currentValue,
    string unit)
        {
            InitializeComponent();

            // Header - only unit name
            TitleText.Text = unitName;

            // Title beside input
            ParameterTitleText.Text = title;

            // Existing value
            ValueTextBox.Text = currentValue.ToString("0.00");

            // Unit
            InputUnitText.Text = unit;

            ValueTextBox.Focus();
            ValueTextBox.SelectAll();
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(
                    ValueTextBox.Text,
                    out double value))
            {
                MessageBox.Show(
                    "Please enter a valid numeric value.",
                    "Invalid Value",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ValueTextBox.Focus();
                ValueTextBox.SelectAll();

                return;
            }

            SelectedValue = value;

            DialogResult = true;
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}