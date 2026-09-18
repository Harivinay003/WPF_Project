using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFSCADA.PopUps
{
    public partial class CompressorDetailsWindow : Window
    {
        private readonly string _unitName;

        public CompressorDetailsWindow(string unitName, bool showMasterSlave = true)
        {
            InitializeComponent();
            _unitName = string.IsNullOrWhiteSpace(unitName) ? "COMPRESSOR": unitName;
            UnitNameText.Text = _unitName;
            if (!showMasterSlave)
            {
                MasterSlavePanel.Visibility = Visibility.Collapsed;
            }
            ShowTab(1);
        }
        private void ControlsTab_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(1);
        }
        private void EventsTab_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(2);
        }
        private void MaintenanceTab_Click(object sender, RoutedEventArgs e)
        {
            ShowTab(3);
        }
        private void ShowTab(int tab)
        {
            ControlsTab.Visibility = Visibility.Collapsed;
            EventsTab.Visibility = Visibility.Collapsed;
            MaintenanceTab.Visibility = Visibility.Collapsed;
            ResetTabButtons();
            switch (tab)
            {
                case 1:
                    ControlsTab.Visibility = Visibility.Visible;
                    ActivateTabButton(ControlsTabButton);
                    break;

                case 2:
                    EventsTab.Visibility =Visibility.Visible;
                    ActivateTabButton(EventsTabButton);
                    break;

                case 3:
                    MaintenanceTab.Visibility = Visibility.Visible;
                    ActivateTabButton(MaintenanceTabButton);
                    break;
            }
        }
        private void ResetTabButtons()
        {
            ControlsTabButton.Background = Brushes.Transparent;
            ControlsTabButton.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            EventsTabButton.Background = Brushes.Transparent;
            EventsTabButton.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            MaintenanceTabButton.Background = Brushes.Transparent;
            MaintenanceTabButton.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
        }
        private void ActivateTabButton(Button button)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(55, 65, 81)); 
            button.Foreground = Brushes.White;
        }
        private void ManualModeButton_Click(object sender,RoutedEventArgs e)
        {
            if (!ShowConfirmation("Confirm Operating Mode", $"Are you sure you want to switch " + $"{_unitName} to MANUAL mode?"))
            {
                return;
            }
            // TODO:
            // Write MANUAL operating mode to compressor PLC.
        }
        private void AutoModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowConfirmation("Confirm Operating Mode", $"Are you sure you want to switch " + $"{_unitName} to AUTO mode?"))
            {
                return;
            }
                        // TODO:
            // Write AUTO operating mode to compressor PLC.
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowConfirmation( "Confirm Start", $"Are you sure you want to START " + $"{_unitName}?"))
            {
                return;
            }
            // TODO:
            // Write START command to compressor PLC.
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowConfirmation("Confirm Stop", $"Are you sure you want to STOP " + $"{_unitName}?"))
            {
                return;
            }
            // TODO:
            // Write STOP command to compressor PLC.
        }
        private void AlarmResetButton_Click( object sender,RoutedEventArgs e)
        {
            if (!ShowConfirmation("Confirm Alarm Reset", $"Are you sure you want to RESET " + $"the alarm on {_unitName}?"))
            {
                return;
            }
            // TODO:
            // Write ALARM RESET command to compressor PLC.
        }
        private void MasterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowConfirmation( "Confirm Compressor Role", $"Are you sure you want to make " +  $"{_unitName} MASTER?"))
            {
                return;
            }
            // TODO:
            // Write MASTER selection to PLC.

            // TODO:
            // If this page contains another compressor,
            // that compressor must become SLAVE.
        }
        private void SlaveButton_Click(object sender,RoutedEventArgs e)
        {
            if (!ShowConfirmation("Confirm Compressor Role",$"Are you sure you want to make " + $"{_unitName} SLAVE?"))
            {
                return;
            }
            // TODO:
            // Write SLAVE selection to PLC.
        }
        private bool ShowConfirmation(string title, string message)
        {
            var dialog = new ConfirmationDialog(title, message);
            dialog.Owner = this;
            return dialog.ShowDialog() == true;
        }
        private void CloseButton_Click( object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}