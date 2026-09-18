using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VirtualEMS.DataServices;
using VirtualEMS.Library;
using WPFSCADA.Configuration;
using WPFSCADA.Helpers;
using WPFSCADA.Services;

namespace WPFSCADA.PopUps
{
    public partial class CondenserDetailsWindow : Window, INotifyPropertyChanged
    {
        private readonly Dictionary<int, double> TagUpdateActions = new Dictionary<int, double>();
        private DispatcherTimer _liveValuesTimer;
        private readonly SemaphoreSlim _plcLock =  new SemaphoreSlim(1, 1);
        private IODevice _plcDevice;
        private readonly Dictionary<int, Tag> _popupTags = new Dictionary<int, Tag>();
        private readonly Dictionary<string, Tag> _popupTagsByConfigKey =  new Dictionary<string, Tag>();
        private Dictionary<string, string> AssignedTags = new Dictionary<string, string>();
        private bool _configurationLoaded = false;
        private bool _isUpdatingUI = false;
        private bool _isWriting = false;
        private readonly Dictionary<UIElement, bool> _originalEnabledStates = new Dictionary<UIElement, bool>();
        private bool _requestedConfigurationMode;
        private bool _isConfigurationMode;
        private readonly string _unitName;
        private readonly string _pageName;
        private int _pageId;
        public iDbRepository Repository { get; private set; }
        private PLCService _plcService;
        private ConfigurationService _configurationService;
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConfigurationMode
        {
            get => _isConfigurationMode;
            set
            {
                if (_isConfigurationMode == value)
                    return;
                _isConfigurationMode = value;
                OnPropertyChanged(nameof(IsConfigurationMode));
                if (!_isConfigurationMode)
                {
                    ClearConfigurationTooltips();
                }
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public CondenserDetailsWindow(string unitName, string pageName,bool showMasterSlave = true)
        {
            InitializeComponent();
            AddHandler( UIElement.PreviewMouseDownEvent,new MouseButtonEventHandler(ConfigurationMouseDown),true);
            AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(ConfigurationMouseMove),true);
            Repository = GetRepository();
            _plcService = new PLCService(Repository);
            _configurationService = new ConfigurationService(Repository);
            _unitName = string.IsNullOrWhiteSpace(unitName) ? " CONDENSER": unitName;
            UnitNameText.Text = _unitName;
            _pageName = pageName;
            if (!LoadPLCConfiguration())
            {
                MessageBox.Show("Unable to load PLC configuration.","Configuration Error", MessageBoxButton.OK,MessageBoxImage.Warning);
            }
            ShowTab(1);
            Loaded += CondenserDetailsWindow_Loaded;
        }
        private void CondenserDetailsWindow_Loaded(object sender,RoutedEventArgs e)
        {
            var page = _configurationService.GetPage(_pageName);
            if (page == null)
            {
                MessageBox.Show($"Page not found: {_pageName}");
                return;
            }
            _pageId = page.Id;
            ConfigurePopupTags(_unitName);
            LoadPopupAssignedTags();
            ApplyConfigurationMode(_requestedConfigurationMode);
            StartLivePolling();
        }    
        private void ConfigurePopupTags(string unitName)
        {
            _popupTags.Clear();
            _popupTagsByConfigKey.Clear();
            AssignedTags.Clear();
            var controls = GetConfiguredControls().ToList();
            var controlNames = new List<string>();

            foreach (var control in controls)
            {
                string configKey = TagProperties.GetConfigKey(control);

                if (string.IsNullOrWhiteSpace(configKey))
                {
                    continue;
                }
                string controlName = $"{unitName}.{configKey}";
                controlNames.Add(controlName);
                _configurationService.EnsurePageItem(_pageId, controlName, configKey);
            }
            AssignedTags = _configurationService.LoadAssignedTags(_pageId, controlNames);

            var controlTagMappings = _configurationService.LoadControlTagMappings(_pageId);
            var allTags = Repository.GetTags();
            string prefix = GetTagSourcePrefix();

            foreach (var mapping in GetCondenserTagMap())
            {
                string configKey = mapping.Key;
                string tagName = mapping.Value;

                var tag = allTags.FirstOrDefault(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));
                if (tag == null)
                {
                    Debug.WriteLine($"CONDENSER TAG NOT FOUND: {configKey} -> {tagName}");
                    continue;
                }
                _popupTags[tag.Id] = tag;
                _popupTagsByConfigKey[configKey] = tag;
            }
        }
        public IEnumerable<DependencyObject> GetConfiguredControls()
        {
            foreach (var child in GetAllChildren(this))
            {
                string configKey =TagProperties.GetConfigKey(child);
                if (!string.IsNullOrWhiteSpace(configKey))
                {
                    yield return child;
                }
            }
        }
        private static IEnumerable<DependencyObject> GetAllChildren(DependencyObject parent)
        {
            if (parent == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent,i);
                yield return child;
                foreach (var descendant in GetAllChildren(child))
                {
                    yield return descendant;
                }
            }
        }
        private void StartLivePolling()
        {
            _liveValuesTimer = new DispatcherTimer();
            _liveValuesTimer.Interval = TimeSpan.FromMilliseconds(500);
            _liveValuesTimer.Tick += LiveValuesTimer_Tick;
            _liveValuesTimer.Start();
        }
        private async void LiveValuesTimer_Tick(object sender,EventArgs e)
        {
            if (_isWriting)
                return;
            await GetLiveValuesFromPLC();
        }
        private Dictionary<string, string> GetCondenserTagMap()
        {
            string prefix = GetTagSourcePrefix();
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["RemoteFb"] = $"{prefix}_REMOTE_FB",
                ["Fan1RunFb"] = $"{prefix}_FAN1_RFB",
                ["Fan1TripFb"] = $"{prefix}_FAN1_TFB",
                ["Fan2RunFb"] = $"{prefix}_FAN2_RFB",
                ["Fan2TripFb"] = $"{prefix}_FAN2_TFB",
                ["PumpRunFb"] = $"{prefix}_PUMP_RFB",
                ["PumpTripFb"] = $"{prefix}_PUMP_TFB",
                ["AutoManual"] = $"{prefix}_I_AM",
                ["MasterSelection"] = $"{prefix}_I_MASTER",
                ["Slave1"] = $"{prefix}_SLAVE_1",
                ["Slave2"] = $"{prefix}_SLAVE_2",
                ["PumpStart"] = $"{prefix}_I_PUMP_START",
                ["PumpStop"] = $"{prefix}_I_PUMP_STOP",
                ["Fan1Start"] = $"{prefix}_I_FAN1_START",
                ["Fan1Stop"] = $"{prefix}_I_FAN1_STOP",
                ["Fan2Start"] = $"{prefix}_I_FAN2_START",
                ["Fan2Stop"] = $"{prefix}_I_FAN2_STOP",
                ["WaterFlow"] = $"{prefix}_WATER_FM",
                ["HighDischargePressure"] = $"{prefix}_HS_DPT",
                ["LoadingSetPoint"] = $"{prefix}_LSP",
                ["UnloadingSetPoint"] = $"{prefix}_UNLSP",
                ["RpmOut"] = $"{prefix}_RPM_OUT",
                ["PumpRunHours"] = $"{prefix}_PUMP_RUN_HRS",
                ["Fan1RunHours"] = $"{prefix}_FAN1_RUN_HRS",
                ["Fan2RunHours"] = $"{prefix}_FAN2_RUN_HRS",
                ["Fan1Rpm"] = $"{prefix}_FAN1_RPM_SP",
                ["Fan2Rpm"] = $"{prefix}_FAN2_RPM_SP",
            };
        }
        private async Task GetLiveValuesFromPLC()
        {
            if (!_configurationLoaded || _plcDevice == null || _popupTags.Count == 0)
            {
                Debug.WriteLine("CONDENSER LIVE: READ SKIPPED");
                return;
            }
            if (_isWriting)
                return;
            await _plcLock.WaitAsync();
            try
            {
                var tagIds = _popupTags.Keys.ToList();
                var values = await _plcService.ReadTagsAsync(tagIds);
                foreach (var item in values)
                {
                    TagUpdateActions[item.Key] = item.Value;
                }
                Dispatcher.Invoke(UpdatePopupFromPLC);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CONDENSER PLC read error: {ex}");
            }
            finally
            {
                _plcLock.Release();
            }
        }
        private void UpdatePopupFromPLC()
        {
            if (_isUpdatingUI)
                return;
            try
            {
                _isUpdatingUI = true;

                bool isRemote = GetBool("RemoteFb");

                if (isRemote)
                {
                    ControlModeText.Text = "REMOTE";
                    ControlModeText.Foreground = new SolidColorBrush(Color.FromRgb(22, 156, 74));
                }
                else
                {
                    ControlModeText.Text ="LOCAL";
                    ControlModeText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
                }

                bool isAuto = GetBool("AutoManual");
                UpdateOperatingModeButtons(isAuto);
                UpdateMasterSlaveButtons();
                UpdateEquipmentStatus(Fan1FeedbackText,Fan1RpmText, "Fan1RunFb", "Fan1TripFb","Fan1Rpm");
                UpdateEquipmentStatus(Fan2FeedbackText,Fan2RpmText,"Fan2RunFb", "Fan2TripFb","Fan2Rpm");
                UpdateEquipmentStatus(PumpFeedbackText, null,"PumpRunFb","PumpTripFb", null);
                //UpdateEquipmentStatus(PumpFeedbackText, PumpRpmText,"PumpRunFb","PumpTripFb","PumpRpm");
                UpdateRunTime(Fan1RunHoursText,Fan1RunMinutesText, "Fan1RunHours");
                UpdateRunTime(Fan2RunHoursText,Fan2RunMinutesText,"Fan2RunHours");
                UpdateRunTime(PumpRunHoursText, PumpRunMinutesText,"PumpRunHours");
                UpdateControlAvailability(isRemote,isAuto);
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }
        private void UpdateOperatingModeButtons(bool isAuto)
        {
            if (isAuto)
            {
                AutoModeButton.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                AutoModeButton.Foreground = Brushes.White;
                ManualModeButton.Background =Brushes.Transparent;
                ManualModeButton.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            }
            else
            {
                ManualModeButton.Background = new SolidColorBrush(Color.FromRgb( 30, 115,232));
                ManualModeButton.Foreground = Brushes.White;
                AutoModeButton.Background =Brushes.Transparent;
                AutoModeButton.Foreground = new SolidColorBrush( Color.FromRgb( 75, 85,99));
            }
        }
        private void UpdateMasterSlaveButtons()
        {
            bool isMaster = GetBool("MasterSelection");
            bool isSlave1 = GetBool("Slave1");
            bool isSlave2 = GetBool("Slave2");

            if (isMaster)
            {
                MasterButton.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                MasterButton.Foreground = Brushes.White;
            }
            else
            {
                MasterButton.Background = Brushes.Transparent;
                MasterButton.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            }
            if (isSlave1)
            {
                Slave1Text.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                Slave1Text.Foreground = Brushes.White;
            }
            else
            {
                Slave1Text.Background = new SolidColorBrush(Color.FromRgb(232, 238, 247));
                Slave1Text.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            }
            if (isSlave2)
            {
                Slave2Text.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                Slave2Text.Foreground = Brushes.White;
            }
            else
            {
                Slave2Text.Background = new SolidColorBrush(Color.FromRgb(232, 238, 247));
                Slave2Text.Foreground =new SolidColorBrush(Color.FromRgb(100, 116, 139));
            }
        }
        private void UpdateControlAvailability(bool isRemote,bool isAuto)
        {
            if (IsConfigurationMode)
            {
                foreach (var control in GetConfiguredControls())
                {
                    if (control is UIElement uiElement)
                    {
                        uiElement.IsEnabled = true;
                    }
                }
                return;
            }
            AutoModeButton.IsEnabled = isRemote;
            ManualModeButton.IsEnabled =isRemote;
            MasterButton.IsEnabled = isRemote;
            bool manualAvailable =isRemote && !isAuto;
            Fan1StartButton.IsEnabled = manualAvailable;
            Fan1StopButton.IsEnabled = manualAvailable;
            Fan2StartButton.IsEnabled = manualAvailable;
            Fan2StopButton.IsEnabled =manualAvailable;
            PumpStartButton.IsEnabled = manualAvailable;
            PumpStopButton.IsEnabled = manualAvailable;
        }      
        private Tag GetPopupTag(string configKey)
        {
            if (string.IsNullOrWhiteSpace(configKey))
            {
                return null;
            }
            return _popupTagsByConfigKey.TryGetValue(configKey, out Tag tag) ? tag : null;
        }
        private bool GetBool(string configKey)
        {
            var tag = GetPopupTag(configKey);
            if (tag == null)
                return false;
            return TagUpdateActions.TryGetValue(tag.Id, out double value)&& value != 0;
        }
        private bool TryGetDouble(string configKey, out double value)
        {
            value = 0;
            var tag = GetPopupTag(configKey);
            if (tag == null)
                return false;
            return TagUpdateActions.TryGetValue(tag.Id, out value);
        }
        private void UpdateEquipmentStatus(TextBlock feedbackText,TextBlock rpmText,  string runConfigKey, string tripConfigKey, string rpmConfigKey)
        {
            bool run = GetBool(runConfigKey);
            bool trip = GetBool(tripConfigKey);

            if (trip)
            {
                feedbackText.Text = "TRIPPED";
                feedbackText.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11));
            }
            else if (run)
            {
                feedbackText.Text = "RUNNING";
                feedbackText.Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74));
            }
            else
            {
                feedbackText.Text = "STOPPED";
                feedbackText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            }
            if (TryGetDouble(rpmConfigKey, out double rpm))
            {
                rpmText.Text = rpm.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            }
            //else
            //{
            //    //rpmText.Text = "--";
            //}
        }
        private void UpdateRunTime(TextBlock hoursText, TextBlock minutesText, string hoursConfigKey, string minutesConfigKey = null)
        {
            Debug.Write(hoursText.Text);
            if (TryGetDouble(hoursConfigKey, out double decimalHours))
            {
                // 2.25 -> 2 Hours 15 Minutes
                // 7.75 -> 7 Hours 45 Minutes
                int totalMinutes = (int)Math.Round(decimalHours * 60);
                int hours = totalMinutes / 60;
                int minutes = totalMinutes % 60;
                hoursText.Text = hours.ToString(System.Globalization.CultureInfo.InvariantCulture);
                minutesText.Text = minutes.ToString("00", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                hoursText.Text = "--";
                minutesText.Text = "--";
            }
        }      
        private async void AutoModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            bool remote = GetBool("RemoteFb");
            if (!remote)
                return;
            if (GetBool("AutoManual"))
                return;
            bool confirmed = ShowConfirmation( "Confirm Operating Mode", $"Are you sure you want to switch " + $"{_unitName} to AUTO mode?");
            if (!confirmed)
                return;
            bool success = await WriteTagBitAsync("AutoManual",true);

            if (!success)
            {
                ShowWriteError("AUTO mode");
                return;
            }
        }
        private async void ManualModeButton_Click( object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            bool remote = GetBool("RemoteFb");
            if (!remote)
                return;
            if (!GetBool("AutoManual"))
                return;
            bool confirmed = ShowConfirmation("Confirm Operating Mode", $"Are you sure you want to switch " + $"{_unitName} to MANUAL mode?");
            if (!confirmed)
                return;
            bool success = await WriteTagBitAsync("AutoManual",false);

            if (!success)
            {
                ShowWriteError("MANUAL mode");
                return;
            }
        }      
        private async void MasterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            if (IsConfigurationMode)
                return;
            bool remote = GetBool("RemoteFb");
            if (!remote)
            {
                MessageBox.Show($"{_unitName} is in LOCAL mode.\n" + "Master selection is available only in REMOTE mode.", "MASTER SELECTION", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            bool alreadyMaster = GetBool("MasterSelection");

            if (alreadyMaster)
            {
                MessageBox.Show( $"{_unitName} is already selected as MASTER.", "MASTER SELECTION", MessageBoxButton.OK,  MessageBoxImage.Information);

                return;
            }
            bool confirmed =  ShowConfirmation("Confirm Master Selection", $"Are you sure you want to select" + $"{_unitName}" + "as MASTER?" + "The other two condensers will become " + "SLAVE 1 and SLAVE 2.");

            if (!confirmed)
                return;

            bool success = await WriteTagBitAsync("MasterSelection", true);
            if (!success)
            {
                ShowWriteError("MASTER SELECTION");
                return;
            }
        }
        private async void Fan1StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            if (!GetBool("RemoteFb"))
                return;

            if (GetBool("AutoManual"))
            {
                MessageBox.Show( $"{_unitName} is in AUTO mode." + "Please switch to MANUAL mode before " + "starting FAN-1.", "FAN-1 START",  MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool confirmed = ShowConfirmation("Confirm FAN-1 Start", $"Are you sure you want to START " + $"FAN-1 on {_unitName}?");
            if (!confirmed)
                return;
            bool success = await PulseTagBitAsync("Fan1Start");
            if (!success)
            {
                ShowWriteError("FAN-1 START");
            }
        }
        private async void Fan1StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            if (!GetBool("RemoteFb"))
                return;
            if (GetBool("AutoManual"))
            {
                MessageBox.Show($"{_unitName} is in AUTO mode." + "Please switch to MANUAL mode before " + "stopping FAN-1.","FAN-1 STOP",MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            bool confirmed = ShowConfirmation("Confirm FAN-1 Stop",$"Are you sure you want to STOP " + $"FAN-1 on {_unitName}?");
            if (!confirmed)
                return;
            bool success = await PulseTagBitAsync("Fan1Stop");
            if (!success)
            {
                ShowWriteError("FAN-1 STOP");
            }
        }
        private async void Fan2StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            if (!GetBool("RemoteFb"))
                return;
            if (GetBool("AutoManual"))
            {
                MessageBox.Show(
                    $"{_unitName} is in AUTO mode." + "Please switch to MANUAL mode before " + "starting FAN-2.", "FAN-2 START", MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            bool confirmed =  ShowConfirmation("Confirm FAN-2 Start", $"Are you sure you want to START " + $"FAN-2 on {_unitName}?");
            if (!confirmed)
                return;
            bool success = await PulseTagBitAsync("Fan2Start");
            if (!success)
            {
                ShowWriteError("FAN-2 START");
            }
        }
        private async void Fan2StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            if (!GetBool("RemoteFb"))
                return;
            if (GetBool("AutoManual"))
            {
                MessageBox.Show( $"{_unitName} is in AUTO mode." +"Please switch to MANUAL mode before " + "stopping FAN-2.", "FAN-2 STOP",MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            bool confirmed = ShowConfirmation("Confirm FAN-2 Stop", $"Are you sure you want to STOP " + $"FAN-2 on {_unitName}?");
            if (!confirmed)
                return;
            bool success = await PulseTagBitAsync("Fan2Stop");
            if (!success)
            {
                ShowWriteError("FAN-2 STOP");
            }
        }
        private async void PumpStartButton_Click( object sender,RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;
            if (!GetBool("RemoteFb"))
                return;
            if (GetBool("AutoManual"))
            {
                MessageBox.Show($"{_unitName} is in AUTO mode." +"Please switch to MANUAL mode before " + "starting the PUMP.","PUMP START",  MessageBoxButton.OK,   MessageBoxImage.Warning);
                return;
            }
            bool confirmed = ShowConfirmation("Confirm Pump Start", $"Are you sure you want to START " + $"the PUMP on {_unitName}?");
            if (!confirmed)
                return;
            bool success = await PulseTagBitAsync("PumpStart");
            if (!success)
            {
                ShowWriteError("PUMP START");
            }
        }
        private async void PumpStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingUI)
                return;

            if (!GetBool("RemoteFb"))
                return;

            if (GetBool("AutoManual"))
            {
                MessageBox.Show( $"{_unitName} is in AUTO mode." + "Please switch to MANUAL mode before " + "stopping the PUMP.", "PUMP STOP", MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            bool confirmed = ShowConfirmation("Confirm Pump Stop", $"Are you sure you want to STOP " + $"the PUMP on {_unitName}?");
            if (!confirmed)
                return;
            bool success =await PulseTagBitAsync("PumpStop");
            if (!success)
            {
                ShowWriteError("PUMP STOP");
            }
        }
        private async void Fan1RpmEditButton_Click(object sender, RoutedEventArgs e)
        {
            await EditRpmAsync("Fan1Rpm", "FAN-1 RPM");
        }

        private async void Fan2RpmEditButton_Click(object sender, RoutedEventArgs e)
        {
            await EditRpmAsync("Fan2Rpm", "FAN-2 RPM");
        }

        private async void PumpRpmEditButton_Click(object sender, RoutedEventArgs e)
        {
            await EditRpmAsync("PumpRpm", "PUMP RPM");
        }

        private async Task EditRpmAsync(string configKey, string displayName)
        {
            if (IsConfigurationMode || _isUpdatingUI || _isWriting)
                return;

            if (!GetBool("RemoteFb"))
            {
                MessageBox.Show( $"{_unitName} is in LOCAL mode." + "RPM cannot be changed while the condenser is in LOCAL mode.", "RPM Edit", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Tag tag = GetPopupTag(configKey);
            if (tag == null)
            {
                MessageBox.Show($"No PLC tag is assigned for:\n{displayName}", "Tag Not Assigned",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            double currentValue = 0;
            TryGetDouble(configKey, out currentValue);
            var dialog = new NumericValueDialog( _unitName, displayName, currentValue, "RPM");
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() != true)
                return;
            double newValue = dialog.SelectedValue;
            if (newValue < 0)
            {
                MessageBox.Show("RPM cannot be negative.",  "Invalid RPM", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool confirmed = ShowConfirmation( "Confirm RPM Change", $"Are you sure you want to change" +$"{displayName}" + $"from {currentValue:0.##} RPM to {newValue:0.##} RPM?");
            if (!confirmed)
                return;
            bool success = await WriteNumericTagAsync(tag, newValue, configKey);
            if (!success)
            {
                MessageBox.Show($"Failed to write {displayName} to the PLC.","PLC Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task<bool> WriteNumericTagAsync(Tag tag, double value,string configKey)
        {
            if (!_configurationLoaded || tag == null ||  _plcDevice == null ||string.IsNullOrWhiteSpace(_plcDevice.IpAddress) || _isWriting)
            {
                return false;
            }
            _isWriting = true;
            await _plcLock.WaitAsync();
            try
            {
                if (tag.Type != DataType.REAL)
                {
                    Console.WriteLine($"CONDENSER RPM WRITE: Unsupported type " + $"{tag.Type} for {tag.Name}");
                    return false;
                }
                bool success = await _plcService.WriteRealAsync(tag.Id,(float)value);              
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine( $"CONDENSER RPM write error: {ex}");
                return false;
            }
            finally
            {
                _plcLock.Release();
                _isWriting = false;
            }
        }
        private async Task<bool> PulseTagBitAsync(string configKey)
        {
            if (!_configurationLoaded || _plcDevice == null || _isWriting)
            {
                return false;
            }
            Tag tag = GetPopupTag(configKey);
            if (tag == null)
            {
                Debug.WriteLine($"Pulse tag not found: {configKey}");
                return false;
            }
            _isWriting = true;
            await _plcLock.WaitAsync();

            try
            {
                bool onSuccess = await _plcService.WriteBoolAsync(tag.Id,true);
                if (!onSuccess)
                    return false;
                await Task.Delay(500);
                bool offSuccess = await _plcService.WriteBoolAsync(tag.Id,false);              

                return offSuccess;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CONDENSER pulse error: {ex}");
                return false;
            }
            finally
            {
                _plcLock.Release();
                _isWriting = false;
            }
        }
        private async Task<bool> WriteTagBitAsync(string configKey, bool value)
        {
            if (!_configurationLoaded)
                return false;
            Tag tag = GetPopupTag(configKey);
            if (tag == null)
            {
                Debug.WriteLine($"Tag not found for ConfigKey: {configKey}");
                return false;
            }
            if (_plcDevice == null || string.IsNullOrWhiteSpace(_plcDevice.IpAddress))
            {
                return false;
            }
            if (_isWriting)
                return false;
            _isWriting = true;
            await _plcLock.WaitAsync();
            try
            {
                bool success = await _plcService.WriteBoolAsync(tag.Id, value);            

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine( $"CONDENSER bit write error: {ex}");
                return false;
            }
            finally
            {
                _plcLock.Release();
                _isWriting = false;
            }
        }
        private iDbRepository GetRepository()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConfigDBConnString"]?.ConnectionString;

                if (string.IsNullOrWhiteSpace(
                    connectionString))
                {
                    return null;
                }
                var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
                var context = new AppDbContext(options);
                return new dbRepository(context);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Repository error: {ex}");
                return null;
            }
        }
        private bool LoadPLCConfiguration()
        {
            try
            {
                var repository =Repository;
                if (repository == null)
                    return false;

                _plcDevice = repository.GetIODevices().ToList()
                        .FirstOrDefault(d => d.DeviceType == IODeviceType.PLC || d.DeviceType == IODeviceType.EthernetDevice);

                if (_plcDevice == null)
                {
                    Debug.WriteLine("PLC device not found.");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(_plcDevice.IpAddress))
                {
                    Debug.WriteLine("PLC IP address not configured.");
                    return false;
                }
                _configurationLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PLC configuration error: " + $"{ex}");
                return false;
            }
        }
        private bool ShowConfirmation(string title, string message)
        {
            var dialog = new ConfirmationDialog(title, message)
                {
                    Owner = Window.GetWindow(this)
                };
            return dialog.ShowDialog() == true;
        }
        private void ShowWriteError(string operation)
        {
            MessageBox.Show($"Unable to send '{operation}' " + $"command to the PLC.\n\n" + "Please check the PLC connection.", "PLC Communication Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void ConfigurationMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsConfigurationMode)
                return;
            var source =e.OriginalSource as DependencyObject;
            if (source == null)
                return;

            var element = FindConfiguredElement(source);
            if (element == null)
                return;
            string configKey =TagProperties.GetConfigKey(element);
            if (string.IsNullOrWhiteSpace(configKey))
            {
                return;
            }
            ConfigureControl(element);
            e.Handled = true;
        }
        private void ConfigureControl(DependencyObject element)
        {
            if (!IsConfigurationMode)
                return;
            string configKey = TagProperties.GetConfigKey(element);
            if (string.IsNullOrWhiteSpace(configKey))
            {
                return;
            }
            string controlName = $"{_unitName}.{configKey}";
            var page =_configurationService.GetPage(_pageName);
            if (page == null)
            {
                MessageBox.Show( $"Page was not found for:\n" + $"{_pageName}");
                return;
            }
            var pageItem = _configurationService.GetPageItem( page.Id,controlName);
            if (pageItem == null)
            {
                MessageBox.Show( $"No PageItem found for:\n" + $"{controlName}");
                return;
            }
            var window = new TagSelectionWindow(Repository, pageItem, GetTagSourcePrefix());
            window.Owner = Window.GetWindow(this);
            bool? result = window.ShowDialog();
            if (result == true)
            {
                ConfigurePopupTags(_unitName);
                LoadPopupAssignedTags();
            }
        }
        private string GetTagSourcePrefix()
        {
            if (string.Equals(_pageName, "Condenser-1", StringComparison.OrdinalIgnoreCase))
                return "EC1";
            if (string.Equals(_pageName, "Condenser-2", StringComparison.OrdinalIgnoreCase))
                return "EC2";
            if (string.Equals(_pageName, "Condenser-3", StringComparison.OrdinalIgnoreCase))
                return "EC3";
            return string.Empty;
        }
        private void LoadPopupAssignedTags()
        {
            var controls =GetConfiguredControls().ToList();
            var controlNames = controls.Select(control =>
                    {
                        string configKey =TagProperties.GetConfigKey(control);
                        return
                            string.IsNullOrWhiteSpace(configKey)? null: $"{_unitName}.{configKey}";
                    })
                    .Where(name => !string.IsNullOrWhiteSpace(name)).ToList();

            AssignedTags = _configurationService.LoadAssignedTags( _pageId, controlNames);
        }
        private void ConfigurationMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsConfigurationMode)
                return;

            var source = e.OriginalSource as DependencyObject;
            if (source == null)
                return;
            var element = FindConfiguredElement(source);
            if (element == null)
                return;
            ShowAssignedTagTooltip(element);
        }
        private void ShowAssignedTagTooltip(DependencyObject element)
        {
            if (!IsConfigurationMode)
                return;
            string configKey = TagProperties.GetConfigKey(element);
            if (string.IsNullOrWhiteSpace(configKey))
            {
                return;
            }
            string controlName = $"{_unitName}.{configKey}";
            if (element is not FrameworkElement frameworkElement)
            {
                return;
            }
            string tooltipText;
            if (AssignedTags.TryGetValue(controlName,out string tagName))
            {
                tooltipText = $"Control : {controlName}\n" + $"Tag     : {tagName}";
            }
            else
            {
                tooltipText = $"Control : {controlName}\n" + $"Tag     : Not Assigned";
            }
            ToolTipService.SetToolTip(frameworkElement,tooltipText);
            ToolTipService.SetInitialShowDelay(frameworkElement, 100);
            ToolTipService.SetShowDuration(frameworkElement,10000);
        }
        private DependencyObject FindConfiguredElement(DependencyObject source)
        {
            DependencyObject current = source;
            while (current != null)
            {
                string configKey = TagProperties.GetConfigKey(current);
                if (!string.IsNullOrWhiteSpace(configKey))
                {
                    return current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
        private void ClearConfigurationTooltips()
        {
            foreach (var control in GetConfiguredControls())
            {
                if (control is FrameworkElement fe)
                {
                    fe.ToolTip = null;
                }
            }
        }
        public void SetConfigurationMode(bool enabled)
        {
            _requestedConfigurationMode =enabled;
            if (IsLoaded)
            {
                ApplyConfigurationMode(enabled);
            }
        }
        private void ApplyConfigurationMode(bool enabled)
        {
            IsConfigurationMode =enabled;
            var controls = GetConfiguredControls().OfType<UIElement>().ToList();          

            foreach (var control in controls)
            {
                if (enabled)
                {
                    _originalEnabledStates[control] = control.IsEnabled;
                    control.IsEnabled = true;
                }
            }
            if (!enabled)
            {
                foreach (var item in _originalEnabledStates)
                {
                    item.Key.IsEnabled = item.Value;
                }
                _originalEnabledStates.Clear();

                if (TagUpdateActions.Count > 0)
                {
                    UpdatePopupFromPLC();
                }
            }
        }
        private void ControlsTab_Click(object sender,RoutedEventArgs e)
        {
            ShowTab(1);
        }
        private void EventsTab_Click(object sender,RoutedEventArgs e)
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
            MaintenanceTab.Visibility =Visibility.Collapsed;
            ResetTabButtons();
            switch (tab)
            {
                case 1:
                    ControlsTab.Visibility =Visibility.Visible;
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
            if (IsConfigurationMode)
            {
                Dispatcher.BeginInvoke(new Action(() => {ConfigurePopupTags( _unitName); }), DispatcherPriority.Loaded);
            }
        }       
        private void ResetTabButtons()
        {
            ControlsTabButton.Background = Brushes.Transparent;
            ControlsTabButton.Foreground =new SolidColorBrush(Color.FromRgb( 75, 85,99));
            EventsTabButton.Background = Brushes.Transparent;
            EventsTabButton.Foreground =new SolidColorBrush(Color.FromRgb( 75, 85, 99));
            MaintenanceTabButton.Background = Brushes.Transparent;
            MaintenanceTabButton.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
        }
        private void ActivateTabButton(Button button)
        {
            button.Background =new SolidColorBrush(Color.FromRgb(55, 65, 81));
            button.Foreground = Brushes.White;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            StopLivePolling();
            base.OnClosed(e);
        }
        private void StopLivePolling()
        {
            if (_liveValuesTimer == null)
                return;
            _liveValuesTimer.Stop();
            _liveValuesTimer.Tick -= LiveValuesTimer_Tick;
            _liveValuesTimer = null;
        }
        private void Window_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}