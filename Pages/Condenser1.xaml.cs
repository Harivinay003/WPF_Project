using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using VirtualEMS.DataServices;
using VirtualEMS.Library;
using WPFSCADA.Configuration;
using WPFSCADA.Controls.Cards;
using WPFSCADA.Helpers;
using WPFSCADA.PopUps;
using WPFSCADA.Services;

namespace WPFSCADA.Pages
{
    public partial class Condenser1 : System.Windows.Controls.Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private double _dischargePressure;
        public double DischargePressure { get => _dischargePressure; set { if (_dischargePressure == value) return; _dischargePressure = value; OnPropertyChanged(nameof(DischargePressure)); } }
        private double _loadingSetPoint;
        public double LoadingSetPoint { get => _loadingSetPoint; set { if (_loadingSetPoint == value) return; _loadingSetPoint = value; OnPropertyChanged(nameof(LoadingSetPoint)); } }
        private double _unloadingSetPoint;
        public double UnloadingSetPoint { get => _unloadingSetPoint; set { if (_unloadingSetPoint == value) return; _unloadingSetPoint = value; OnPropertyChanged(nameof(UnloadingSetPoint)); } }
        private double _alarmSetPoint;
        public double AlarmSetPoint{ get => _alarmSetPoint; set { if (_alarmSetPoint == value)  return; _alarmSetPoint = value; OnPropertyChanged(nameof(AlarmSetPoint)); } }
        private double _safetySetPoint;
        public double SafetySetPoint { get => _safetySetPoint; set { if (_safetySetPoint == value)  return; _safetySetPoint = value; OnPropertyChanged(nameof(SafetySetPoint));  } }
        private bool _c1AutoManual;
        public bool C1AutoManual { get => _c1AutoManual;  set { if (_c1AutoManual != value) {  _c1AutoManual = value; OnPropertyChanged(nameof(C1AutoManual)); } } }
        private bool _c2AutoManual;
        public bool C2AutoManual { get => _c2AutoManual; set { if (_c2AutoManual != value) { _c2AutoManual = value; OnPropertyChanged(nameof(C2AutoManual)); } } }
        private bool _c3AutoManual;
        public bool C3AutoManual { get => _c3AutoManual; set { if (_c3AutoManual != value) { _c3AutoManual = value; OnPropertyChanged(nameof(C3AutoManual)); } } }
        private double _c1FlowMeter;
        public double C1FlowMeter { get => _c1FlowMeter; set { if (_c1FlowMeter == value) return; _c1FlowMeter = value; OnPropertyChanged(nameof(C1FlowMeter)); } }
        private double _c2FlowMeter;
        public double C2FlowMeter { get => _c2FlowMeter; set { if (_c2FlowMeter == value) return; _c2FlowMeter = value; OnPropertyChanged(nameof(C2FlowMeter)); } }
        private double _c3FlowMeter;
        public double C3FlowMeter { get => _c3FlowMeter; set { if (_c3FlowMeter == value) return; _c3FlowMeter = value; OnPropertyChanged(nameof(C3FlowMeter)); } }
        public iDbRepository Repository { get; private set; }
        private PLCService _plcService;
        private ConfigurationService _configurationService;
        private int _mainPageId;
        private bool _configurationLoaded;
        private DispatcherTimer _liveValuesTimer;
        private readonly Dictionary<string, int> ControlTagIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, double> TagUpdateActions = new Dictionary<int, double>();
        private readonly Dictionary<int, Tag> _condenserTags = new Dictionary<int, Tag>();
        private readonly Dictionary<string, Tag> _condenserTagsByConfigKey = new Dictionary<string, Tag>();
        private readonly Dictionary<int, double> _tagValues = new Dictionary<int, double>();
        private bool _isConfigurationMode;
        public bool IsConfigurationMode
        {
            get => _isConfigurationMode;
            set
            {
                if (_isConfigurationMode == value)
                    return;
                _isConfigurationMode = value;

                if (ConfigurationButton != null)
                {
                    ConfigurationButton.Content = value ? "Normal Mode" : "Configuration Mode";
                }
                if (!value)
                {
                    ClearConfigurationTooltips();
                }
            }
        }
        private DispatcherTimer _fanAnimationTimer;
        private int _fanFrame = 1;
        private readonly BitmapImage[] _fanFrames;
        private bool _c1Fan1Running;
        private bool _c1Fan2Running;
        private bool _c2Fan1Running;
        private bool _c2Fan2Running;
        private bool _c3Fan1Running;
        private bool _c3Fan2Running;
        private bool _c1PumpRunning;
        private bool _c2PumpRunning;
        private bool _c3PumpRunning;
        private bool _c1Fan1Trip;
        private bool _c1Fan2Trip;
        private bool _c1PumpTrip;
        private bool _c2Fan1Trip;
        private bool _c2Fan2Trip;
        private bool _c2PumpTrip;
        private bool _c3Fan1Trip;
        private bool _c3Fan2Trip;
        private bool _c3PumpTrip;
        private bool _c1Remote;
        private bool _c2Remote;
        private bool _c3Remote;
        private bool _c1IsMaster;
        private bool _c1IsSlave1;
        private bool _c1IsSlave2;
        private bool _c2IsMaster;
        private bool _c2IsSlave1;
        private bool _c2IsSlave2;
        private bool _c3IsMaster;
        private bool _c3IsSlave1;
        private bool _c3IsSlave2;
        private bool _lastC1PumpRunning;
        private bool _lastC2PumpRunning;
        private bool _lastC3PumpRunning;
        private bool _pipeAnimationStateInitialized;
        private readonly List<FlowParticle> _flowParticles = new();
        private bool _flowAnimationRunning = false;
        private DateTime _lastFlowUpdate = DateTime.Now;
        private readonly Dictionary<string, CondenserDetailsWindow> _condenserPopups = new Dictionary<string, CondenserDetailsWindow>(StringComparer.OrdinalIgnoreCase);
        public Condenser1()
        {
            InitializeComponent();
            _fanFrames = Enumerable.Range(1, 4)
                .Select(i =>
                {
                    var image = new BitmapImage(new Uri($"pack://application:,,,/WPFSCADA;component/Assets/Condenser_Fan{i}.png", UriKind.Absolute));
                    image.Freeze();
                    return image;
                }).ToArray();
            _fanAnimationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _fanAnimationTimer.Tick += FanAnimationTimer_Tick;
            DataContext = this;
            Repository = GetRepository();
            if (Repository != null)
            {
                _plcService = new PLCService(Repository);
                _configurationService = new ConfigurationService(Repository);
            }
            AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(ConfigurationMouseDown), true);
            AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(ConfigurationMouseMove), true);
            Loaded += (s, e) =>
            {
                CreateCondenserPipeAnimations();
            };
            Loaded += Condenser1_Loaded;
            Unloaded += Condenser1_Unloaded;
        }
        private BitmapImage GetFanFrame(int frame)
        {
            return _fanFrames[frame - 1];
        }
        private void Condenser1_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCondenserPLC();
            if (!_fanAnimationTimer.IsEnabled)
                _fanAnimationTimer.Start();
        }
        private void FanAnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (_c1Fan1Trip)
            {
                Fan1Image.Source = GetAssetImage("Condenser_Fan_trip.png");
            }
            else if (_c1Fan1Running)
            {
                Fan1Image.Source = GetFanFrame(_fanFrame);
            }
            else
            {
                Fan1Image.Source = GetAssetImage("Condenser_Fan_Red.png");
            }

            if (_c1Fan2Trip)
            {
                C1Fan1Image.Source = GetAssetImage("Condenser_Fan_trip.png");
            }
            else if (_c1Fan2Running)
            {
                C1Fan1Image.Source = GetFanFrame(_fanFrame);
            }
            else
            {
                C1Fan1Image.Source = GetAssetImage("Condenser_Fan_Red.png");
            }
            if (_c2Fan1Trip)
            {
                Condenser2Fan1Image.Source = GetAssetImage("Condenser_Fan_trip.png");
            }
            else if (_c2Fan1Running)
            {
                Condenser2Fan1Image.Source = GetFanFrame(_fanFrame);
            }
            else
            {
                Condenser2Fan1Image.Source = GetAssetImage("Condenser_Fan_Red.png");
            }
            if (_c2Fan2Trip)
            {
                C2Fan2Image.Source = GetAssetImage("Condenser_Fan_trip.png");
            }
            else if (_c2Fan2Running)
            {
                C2Fan2Image.Source = GetFanFrame(_fanFrame);
            }
            else
            {
                C2Fan2Image.Source = GetAssetImage("Condenser_Fan_Red.png");
            }
            if (_c3Fan1Trip)
            {
                Condenser3Fan1Image.Source = GetAssetImage("Condenser_Fan_trip.png");
            }
            else if (_c3Fan1Running)
            {
                Condenser3Fan1Image.Source = GetFanFrame(_fanFrame);
            }
            else
            {
                Condenser3Fan1Image.Source = GetAssetImage("Condenser_Fan_Red.png");
            }
            if (_c3Fan2Trip)
            {
                C3Fan2Image.Source = GetAssetImage("Condenser_Fan_trip.png");
            }
            else if (_c3Fan2Running)
            {
                C3Fan2Image.Source = GetFanFrame(_fanFrame);
            }
            else
            {
                C3Fan2Image.Source = GetAssetImage("Condenser_Fan_Red.png");
            }
            _fanFrame++;
            if (_fanFrame > 4)
                _fanFrame = 1;
        }
        private void Condenser1_Unloaded(object sender, RoutedEventArgs e)
        {
            StopLivePolling();
            StopFlowRendering();
            if (_fanAnimationTimer != null)
            {
                _fanAnimationTimer.Stop();
            }
        }
        private void InitializeCondenserPLC()
        {
            try
            {
                if (Repository == null)
                {
                    Debug.WriteLine("CONDENSER: Repository is null.");
                    return;
                }
                if (_configurationService == null)
                {
                    _configurationService = new ConfigurationService(Repository);
                }
                var page = Repository.GetPageByName("Condenser-1");
                if (page == null)
                {
                    Debug.WriteLine("CONDENSER: Page 'Condensers' not found.");
                    return;
                }
                _mainPageId = page.Id;
                Debug.WriteLine($"CONDENSER: PageId={_mainPageId}");
                // Configure ALL condenser PLC tags in one place.
                ConfigureCondenserTags();

                _configurationLoaded = true;
                StartLivePolling();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CONDENSER initialization error: {ex}");
            }
        }
        private void ConfigureCondenserTags()
        {
            if (_mainPageId <= 0 || Repository == null)
                return;
            var definitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["HighDischargePressure"] = "EC1_HS_DPT",
                ["LoadingSetPoint"] = "EC1_LSP",
                ["UnloadingSetPoint"] = "EC1_UNLSP",

                ["AlarmSetPoint"] = "EC1_Alarm_Set_Point",
                ["SafetySetPoint"] = "EC1_Safety_Set_Point",
                ["C1AutoManual"] = "EC1_I_AM",
                ["C2AutoManual"] = "EC2_I_AM",
                ["C3AutoManual"] = "EC3_I_AM",
                // Flow meters
                ["C1FlowMeter"] = "EC1_WATER_FM",
                ["C2FlowMeter"] = "EC2_WATER_FM",
                ["C3FlowMeter"] = "EC3_WATER_FM",
                // C1
                ["C1Fan1Run"] = "EC1_FAN1_RFB",
                ["C1Fan2Run"] = "EC1_FAN2_RFB",
                ["C1PumpRun"] = "EC1_PUMP_RFB",
                ["C1Fan1Trip"] = "EC1_FAN1_TFB",
                ["C1Fan2Trip"] = "EC1_FAN2_TFB",
                ["C1PumpTrip"] = "EC1_PUMP_TFB",
                ["C1RemoteFb"] = "EC1_REMOTE_FB",
                ["C1MasterSelection"] = "EC1_I_MASTER",
                ["C1Slave1"] = "EC1_SLAVE_1",
                ["C1Slave2"] = "EC1_SLAVE_2",
                // C2
                ["C2Fan1Run"] = "EC2_FAN1_RFB",
                ["C2Fan2Run"] = "EC2_FAN2_RFB",
                ["C2PumpRun"] = "EC2_PUMP_RFB",
                ["C2Fan1Trip"] = "EC2_FAN1_TFB",
                ["C2Fan2Trip"] = "EC2_FAN2_TFB",
                ["C2PumpTrip"] = "EC2_PUMP_TFB",
                ["C2RemoteFb"] = "EC2_REMOTE_FB",
                ["C2MasterSelection"] = "EC2_I_MASTER",
                ["C2Slave1"] = "EC2_SLAVE_1",
                ["C2Slave2"] = "EC2_SLAVE_2",
                // C3
                ["C3Fan1Run"] = "EC3_FAN1_RFB",
                ["C3Fan2Run"] = "EC3_FAN2_RFB",
                ["C3PumpRun"] = "EC3_PUMP_RFB",
                ["C3Fan1Trip"] = "EC3_FAN1_TFB",
                ["C3Fan2Trip"] = "EC3_FAN2_TFB",
                ["C3PumpTrip"] = "EC3_PUMP_TFB",
                ["C3RemoteFb"] = "EC3_REMOTE_FB",
                ["C3MasterSelection"] = "EC3_I_MASTER",
                ["C3Slave1"] = "EC3_SLAVE_1",
                ["C3Slave2"] = "EC3_SLAVE_2"
            };

            var allTags = Repository.GetTags()?.ToList() ?? new List<Tag>();

            _condenserTags.Clear();
            _condenserTagsByConfigKey.Clear();
            ControlTagIds.Clear();

            foreach (var definition in definitions)
            {
                string configKey = definition.Key;
                string plcTagName = definition.Value;

                var tag = allTags.FirstOrDefault(t => string.Equals( t.Name?.Trim(),  plcTagName, StringComparison.OrdinalIgnoreCase));
                if (tag == null)
                {
                    Debug.WriteLine($"CONDENSER TAG NOT FOUND: {configKey} -> {plcTagName}");
                    continue;
                }
                ControlTagIds[configKey] = tag.Id;
                _condenserTags[tag.Id] = tag;
                _condenserTagsByConfigKey[configKey] = tag;
            }
        }  
        private void StartLivePolling()
        {
            if (_liveValuesTimer != null)
                return;

            _liveValuesTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _liveValuesTimer.Tick += LiveValuesTimer_Tick;
            _liveValuesTimer.Start();
        }
        private void StopLivePolling()
        {
            if (_liveValuesTimer == null)
                return;
            _liveValuesTimer.Stop();
            _liveValuesTimer.Tick -= LiveValuesTimer_Tick;
            _liveValuesTimer = null;
        }
        private async void LiveValuesTimer_Tick(object sender, EventArgs e)
        {
            await ReadCondenserValuesAsync();
        }
        private void UpdateCondenserModeIndicators()
        {
            C1ModeText.Text = _c1Remote ? "R" : "L";           
            C2ModeText.Text = _c2Remote ? "R" : "L";            
            C3ModeText.Text = _c3Remote ? "R": "L";           
            C1OperatingModeText.Text = _c1AutoManual ? "A" : "M";
            C2OperatingModeText.Text = _c2AutoManual ? "A" : "M";
            C3OperatingModeText.Text = _c3AutoManual ? "A" : "M";

        }
        private async Task ReadCondenserValuesAsync()
        {
            if (ControlTagIds.Count == 0 && _condenserTags.Count == 0)
            {
                return;
            }
            try
            {
                var tagIds = ControlTagIds.Values
                    .Concat(_condenserTags.Keys)
                    .Distinct()
                    .ToList();
                var values = await _plcService.ReadTagsAsync(tagIds);
                foreach (var item in values)
                {
                    _tagValues[item.Key] = item.Value;
                    TagUpdateActions[item.Key] = item.Value;
                }
                UpdateCondenserUI();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CONDENSER PLC READ ERROR: {ex}");
            }
        }
      

        private void UpdateCondenserUI()
        {
            DischargePressure = GetTagValue("HighDischargePressure");
            LoadingSetPoint = GetTagValue("LoadingSetPoint");
            UnloadingSetPoint = GetTagValue("UnloadingSetPoint");
            AlarmSetPoint = GetTagValue("AlarmSetPoint");
            SafetySetPoint = GetTagValue("SafetySetPoint");
            C1FlowMeter = GetTagValue("C1FlowMeter");
            C2FlowMeter = GetTagValue("C2FlowMeter");
            C3FlowMeter = GetTagValue("C3FlowMeter");
            _c1Fan1Running = GetTagValue("C1Fan1Run") > 0.5;
            _c1Fan2Running = GetTagValue("C1Fan2Run") > 0.5;
            _c1Fan1Trip = GetTagValue("C1Fan1Trip") > 0.5;
            _c1Fan2Trip = GetTagValue("C1Fan2Trip") > 0.5;
            _c2Fan1Running = GetTagValue("C2Fan1Run") > 0.5;
            _c2Fan2Running = GetTagValue("C2Fan2Run") > 0.5;
            _c2Fan1Trip = GetTagValue("C2Fan1Trip") > 0.5;
            _c2Fan2Trip = GetTagValue("C2Fan2Trip") > 0.5;
            _c3Fan1Running = GetTagValue("C3Fan1Run") > 0.5;
            _c3Fan2Running = GetTagValue("C3Fan2Run") > 0.5;
            _c3Fan1Trip = GetTagValue("C3Fan1Trip") > 0.5;
            _c3Fan2Trip = GetTagValue("C3Fan2Trip") > 0.5;
            _c1PumpRunning = GetCondenserBool("C1PumpRun");
            _c1PumpTrip = GetCondenserBool("C1PumpTrip");
            _c2PumpRunning = GetCondenserBool("C2PumpRun");
            _c2PumpTrip = GetCondenserBool("C2PumpTrip");
            _c3PumpRunning = GetCondenserBool("C3PumpRun");
            _c3PumpTrip = GetCondenserBool("C3PumpTrip");
            bool pumpStateChanged = !_pipeAnimationStateInitialized || _lastC1PumpRunning != _c1PumpRunning || _lastC2PumpRunning != _c2PumpRunning || _lastC3PumpRunning != _c3PumpRunning;

            if (pumpStateChanged)
            {
                _lastC1PumpRunning = _c1PumpRunning;
                _lastC2PumpRunning = _c2PumpRunning;
                _lastC3PumpRunning = _c3PumpRunning;
                _pipeAnimationStateInitialized = true;
                CreateCondenserPipeAnimations();
            }
            C1PumpImage.Source = GetPumpImage(_c1PumpRunning, _c1PumpTrip);
            C2PumpImage.Source = GetPumpImage(_c2PumpRunning, _c2PumpTrip);
            C3PumpImage.Source = GetPumpImage(_c3PumpRunning, _c3PumpTrip);
            Condenser1Image.Source = GetCondenserImage(_c1PumpRunning);
            Condenser2Image.Source = GetCondenserImage(_c2PumpRunning);
            Condenser3Image.Source = GetCondenserImage(_c3PumpRunning);
            _c1Remote = GetCondenserBool("C1RemoteFb");
            _c1IsMaster = GetCondenserBool("C1MasterSelection");
            _c1IsSlave1 = GetCondenserBool("C1Slave1");
            _c1IsSlave2 = GetCondenserBool("C1Slave2");
            _c2Remote = GetCondenserBool("C2RemoteFb");
            _c2IsMaster = GetCondenserBool("C2MasterSelection");
            _c2IsSlave1 = GetCondenserBool("C2Slave1");
            _c2IsSlave2 = GetCondenserBool("C2Slave2");
            _c3Remote = GetCondenserBool("C3RemoteFb");
            _c3IsMaster = GetCondenserBool("C3MasterSelection");
            _c3IsSlave1 = GetCondenserBool("C3Slave1");
            _c3IsSlave2 = GetCondenserBool("C3Slave2");
            C1AutoManual = GetCondenserBool("C1AutoManual");
            C2AutoManual = GetCondenserBool("C2AutoManual");
            C3AutoManual = GetCondenserBool("C3AutoManual");

            UpdateCondenserModeIndicators();
            UpdateMainCondenserRoles();
        }
        private void UpdateMainCondenserRoles()
        {
            SetRoleButton(C1MasterButton, _c1IsMaster);
            SetRoleText(C1Slave1Text, _c1IsSlave1);
            SetRoleText(C1Slave2Text, _c1IsSlave2);
            SetRoleButton(C2MasterButton, _c2IsMaster);
            SetRoleText(C2Slave1Text, _c2IsSlave1);
            SetRoleText(C2Slave2Text, _c2IsSlave2);
            SetRoleButton(C3MasterButton, _c3IsMaster);
            SetRoleText(C3Slave1Text, _c3IsSlave1);
            SetRoleText(C3Slave2Text, _c3IsSlave2);
            C1MasterButton.IsEnabled = _c1Remote;
            C2MasterButton.IsEnabled = _c2Remote;
            C3MasterButton.IsEnabled = _c3Remote;
        }
        private void SetRoleButton(Button button, bool active)
        {
            if (active)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                button.Foreground = Brushes.White;
            }
            else
            {
                button.Background = Brushes.Transparent;
                button.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
            }
        }
        private void SetRoleText(TextBlock textBlock, bool active)
        {
            if (active)
            {
                textBlock.Background = new SolidColorBrush(Color.FromRgb(30, 115, 232));
                textBlock.Foreground = Brushes.White;
            }
            else
            {
                textBlock.Background = new SolidColorBrush(Color.FromRgb(232, 238, 247));
                textBlock.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            }
        }
        private async void C1MasterButton_Click(object sender, RoutedEventArgs e)
        {
            await SelectMasterAsync("C1", "EVAP. COND - 1");
        }
        private async void C2MasterButton_Click(object sender, RoutedEventArgs e)
        {
            await SelectMasterAsync("C2", "EVAP. COND - 2");
        }
        private async void C3MasterButton_Click(object sender, RoutedEventArgs e)
        {
            await SelectMasterAsync("C3", "EVAP. COND - 3");
        }       
        private async Task SelectMasterAsync(string unit,string condenserName)
        {
            if (IsConfigurationMode)
                return;
            string remoteKey = $"{unit}RemoteFb";
            string masterKey = $"{unit}MasterSelection";

            bool isRemote = GetCondenserBool(remoteKey);

            if (!isRemote)
            {
                ShowConfirmation("MASTER SELECTION",$"{condenserName} is in LOCAL mode.\n" +"Master selection is available only in REMOTE mode.");
                return;
            }
            bool alreadyMaster = GetCondenserBool(masterKey);
            if (alreadyMaster)
            {
                ShowConfirmation("MASTER SELECTION", $"{condenserName} is already selected as MASTER.");
                return;
            }
            bool confirmed = ShowConfirmation( "Confirm Master Selection", $"Are you sure you want to select " + $"{condenserName} as MASTER?");
            if (!confirmed)
                return;
            bool success = await WriteMasterCommandAsync(masterKey);
            if (!success)
            {
                ShowConfirmation( "MASTER SELECTION",$"Failed to select {condenserName} as MASTER.");
                return;
            }
        }
        private async Task<bool> WriteMasterCommandAsync(string configKey)
        {
            if (_plcService == null)
                return false;

            if (!ControlTagIds.TryGetValue(configKey, out int tagId))
            {
                Debug.WriteLine($"MASTER WRITE: Tag not assigned for {configKey}");
                return false;
            }
            try
            {
                return await _plcService.WriteBoolAsync(tagId, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MASTER WRITE ERROR: {ex}");
                return false;
            }
        }
        private BitmapImage GetPumpImage(bool isRunning, bool isTrip)
        {
            if (isTrip)
                return GetAssetImage("Condenser_Pump_Trip.png");
            if (isRunning)
                return GetAssetImage("Condenser_Pump_Green.png");
            return GetAssetImage("Condenser_Pump_Red.png");
        }
        private BitmapImage GetAssetImage(string imageName)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri($"pack://application:,,,/WPFSCADA;component/Assets/{imageName}", UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ASSET LOAD ERROR: {imageName} -> {ex.Message}");
                return null;
            }
        }
        private BitmapImage GetCondenserImage(bool isRunning)
        {
            string imageName = isRunning ? "Condenser.png" : "Condenser_WithoutWater.png";
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri($"pack://application:,,,/WPFSCADA;component/Assets/{imageName}", UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        private double GetTagValue(string configKey)
        {
            if (!ControlTagIds.TryGetValue(configKey, out int tagId))
            {
                Debug.WriteLine($"CONDENSER VALUE TAG NOT ASSIGNED: {configKey}");
                return 0;
            }
            if (!_tagValues.TryGetValue(tagId, out double value))
            {
                return 0;
            }

            return value;
        }
        private async Task<bool> WriteRealValueAsync(string configKey, double value)
        {
            if (_plcService == null)
                return false;
            if (!ControlTagIds.TryGetValue(configKey, out int tagId))
            {
                Debug.WriteLine($"CONDENSER WRITE: Tag not assigned for {configKey}");
                return false;
            }
            try
            {
                return await _plcService.WriteRealAsync(tagId, (float)value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CONDENSER WRITE ERROR: {ex}");
                return false;
            }
        }
        private async Task EditSetPointAsync(string configKey, string displayName)
        {
            if (IsConfigurationMode)
                return;
            if (!ControlTagIds.TryGetValue(configKey, out int tagId))
            {
                ShowConfirmation("Tag Not Assigned",$"No PLC tag is assigned for: {displayName}");
                return;
            }
            double currentValue = _tagValues.TryGetValue(tagId, out double value) ? value : 0;
            var dialog = new NumericValueDialog("EVAP CONDENSER", displayName, currentValue, "bar");
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() != true)
                return;
            double newValue = dialog.SelectedValue;
            bool confirmed = ShowConfirmation("Confirm Set Point", $"Are you sure you want to change " + $"{displayName} " +
                    $"from {currentValue:0.00} bar " + $"to {newValue:0.00} bar?");
            if (!confirmed)
                return;

            bool success = await WriteRealValueAsync(configKey, newValue);
            if (!success)
            {
                ShowConfirmation("PLC Write Error", $"Failed to write " + $"{displayName} to the PLC.");
                return;
            }
            if (configKey == "LoadingSetPoint")
                LoadingSetPoint = newValue;
            else if (configKey == "UnloadingSetPoint")
                UnloadingSetPoint = newValue;
            else if (configKey == "AlarmSetPoint")
                AlarmSetPoint = newValue;
            else if (configKey == "SafetySetPoint")
                SafetySetPoint = newValue;
        }
        private bool GetCondenserBool(string configKey)
        {
            if (!ControlTagIds.TryGetValue(configKey, out int tagId))
            {
                Debug.WriteLine($"CONDENSER BOOL TAG NOT FOUND: {configKey}");
                return false;
            }
            if (!_tagValues.TryGetValue(tagId, out double value))
            {
                Debug.WriteLine($"CONDENSER BOOL VALUE NOT FOUND: {configKey}, TagId={tagId}");
                return false;
            }
            return value > 0.5;
        }
        private async void LoadingSetPointEdit_Click(object sender, RoutedEventArgs e)
        {
            await EditSetPointAsync("LoadingSetPoint", "LOADING SET POINT");
        }
        private async void UnLoadingSetPointEdit_Click(object sender, RoutedEventArgs e)
        {
            await EditSetPointAsync("UnloadingSetPoint", "UNLOADING SET POINT");
        }
        private async void DischargePressureCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            await EditSetPointAsync("HighDischargePressure", "HIGH DISCHARGE PRESSURE");
        }
        private async void AlarmSetPointEdit_Click(object sender, RoutedEventArgs e)
        {
            await EditSetPointAsync("AlarmSetPoint", "ALARM SET POINT");
        }
        private async void SafetySetPointEdit_Click( object sender, RoutedEventArgs e)
        {
            await EditSetPointAsync( "SafetySetPoint", "SAFETY SET POINT");
        }
        private void ConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfigurationMode = !IsConfigurationMode;
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
        private void ConfigurationMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsConfigurationMode)
                return;
            var source = e.OriginalSource as DependencyObject;
            if (source == null)
                return;
            var element = FindConfiguredElement(source);
            if (element == null)
                return;
            ConfigureControl(element);
            e.Handled = true;
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
        private string GetTagSource(string configKey)
        {
            switch (configKey)
            {
                case "C1FlowMeter":
                    return "EC1";
                case "C2FlowMeter":
                    return "EC2";
                case "C3FlowMeter":
                    return "EC3";
                case "HighDischargePressure":
                case "LoadingSetPoint":
                case "UnloadingSetPoint":
                    return "EC1";
                default:
                    return "EC1";
            }
        }
        private void ConfigureControl(DependencyObject element)
        {
            if (!IsConfigurationMode)
                return;
            string configKey = TagProperties.GetConfigKey(element);

            if (string.IsNullOrWhiteSpace(configKey))
                return;
            string controlName = $"Condensers.{configKey}";
            var page = Repository.GetPageByName("Condenser-1");
            if (page == null)
            {
                ShowConfirmation("Configuration", "Condenser-1 page was not found.");
                return;
            }
            var pageItem = Repository.GetPageItem(_mainPageId, controlName);
            if (pageItem == null)
            {
                ShowConfirmation("Configuration", $"No PageItem found for:{controlName}");
                return;
            }
            string tagSource = GetTagSource(configKey);
            var window = new TagSelectionWindow(Repository, pageItem, tagSource);
            window.Owner = Window.GetWindow(this);
            bool? result = window.ShowDialog();
            if (result == true)
            {
                ConfigureCondenserTags();
                ShowConfirmation("Configuration", $"Tag configured successfully for:" + $"{controlName}");
            }
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
            if (element is not FrameworkElement frameworkElement)
            {
                return;
            }
            string controlName = $"Condensers.{configKey}";
            var page = Repository.GetPageByName("Condenser-1");
            if (page == null)
                return;
            var pageItem = Repository.GetPageItem(page.Id, controlName);

            if (pageItem == null)
                return;

            var pageItemTag = Repository.GetPageItemTag(pageItem.Id);
            string tooltipText;
            if (pageItemTag?.Tag != null)
            {
                tooltipText = $"Control : {controlName}\n" + $"Tag  : {pageItemTag.Tag.Name}";
            }
            else
            {
                tooltipText = $"Control : {controlName}\n" + $"Tag     : Not Assigned";
            }
            ToolTipService.SetToolTip(frameworkElement, tooltipText);
            ToolTipService.SetInitialShowDelay(frameworkElement, 100);
            ToolTipService.SetShowDuration(frameworkElement, 10000);
        }
        private void ClearConfigurationTooltips()
        {
            foreach (var element in GetConfiguredControls())
            {
                if (element is FrameworkElement fe)
                    fe.ToolTip = null;
            }
        }
        private IEnumerable<DependencyObject> GetConfiguredControls()
        {
            foreach (var child in GetAllChildren(this))
            {
                string configKey = TagProperties.GetConfigKey(child);
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
                var child = VisualTreeHelper.GetChild(parent, i);
                yield return child;
                foreach (var descendant in GetAllChildren(child))
                {
                    yield return descendant;
                }
            }
        }
        private void Condenser1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenCondenserPopup("EVAP CONDENSER-1", "Condenser-1");
            e.Handled = true;
        }
        private void Condenser2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenCondenserPopup("EVAP CONDENSER-2", "Condenser-2");
            e.Handled = true;
        }
        private void Condenser3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenCondenserPopup("EVAP CONDENSER-3", "Condenser-3");
            e.Handled = true;
        }
        private void OpenCondenserPopup(string unitName, string pageName)
        {
            if (_condenserPopups.TryGetValue(pageName, out CondenserDetailsWindow existingPopup))
            {
                if (existingPopup != null && existingPopup.IsVisible)
                {
                    if (existingPopup.WindowState == WindowState.Minimized)
                    {
                        existingPopup.WindowState = WindowState.Normal;
                    }
                    existingPopup.Activate();
                    existingPopup.Topmost = true;
                    existingPopup.Topmost = false;
                    return;
                }
                _condenserPopups.Remove(pageName);
            }
            var popup = new CondenserDetailsWindow(unitName, pageName)
            {
                Owner = Window.GetWindow(this)
            };
            popup.SetConfigurationMode(IsConfigurationMode);

            popup.Closed += (s, args) =>
            {
                _condenserPopups.Remove(pageName);
            };
            _condenserPopups[pageName] = popup;
            popup.Show();
        }
        private readonly Random _gasRandom = new Random();
        private void CreateCondenserPipeAnimations()
        {
            if (PipeAnimationCanvas == null)
                return;
            _flowParticles.Clear();
            PipeAnimationCanvas.Children.Clear();
            // MAIN RED GAS HEADER
            CreateGasPuffFlow(PipeAnimationCanvas, CreateLinePathGeometry(0, 160, 1800, 155), count: 28, baseDurationSeconds: 2.8);
            // C1 RED GAS BRANCH
            CreateGasPuffFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(620, 160), new Point(620, 443), new Point(460, 443)), count: 28, baseDurationSeconds: 2.8);
            // C2 RED GAS BRANCH
            CreateGasPuffFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(1220, 160), new Point(1220, 443), new Point(1070, 443)), count: 28, baseDurationSeconds: 2.8);
            // C3 RED GAS BRANCH
            CreateGasPuffFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(1780, 160), new Point(1780, 443), new Point(1660, 443)), count: 28, baseDurationSeconds: 2.8);
            // MAIN YELLOW GAS HEADER
            CreateGasPuffFlow(PipeAnimationCanvas, CreateLinePathGeometry(0, 900, 1800, 900), count: 28, baseDurationSeconds: 2.8);
            // C1 YELLOW GAS BRANCH
            CreateGasPuffFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(570, 900), new Point(570, 520), new Point(460, 520)), count: 28, baseDurationSeconds: 2.8);
            // C2 YELLOW GAS BRANCH
            CreateGasPuffFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(1180, 900), new Point(1180, 520), new Point(1070, 520)), count: 28, baseDurationSeconds: 2.8);
            // C3 YELLOW GAS BRANCH
            CreateGasPuffFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(1750, 900), new Point(1750, 520), new Point(1660, 520)), count: 28, baseDurationSeconds: 2.8);

            Color liquidFlowColor = Color.FromRgb(0xA5, 0xB4, 0xFC);
            // MAIN BLUE LIQUID HEADER
            CreateLiquidFlow(PipeAnimationCanvas, CreateLinePathGeometry(0, 800, 1800, 800), liquidFlowColor, 1.8);
            // C1 LOWER BLUE LIQUID BRANCH
            CreateLiquidFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(530, 800), new Point(530, 590), new Point(470, 590)), liquidFlowColor, 1.5);
            // C1 UPPER BLUE LIQUID BRANCH
            if (_c1PumpRunning)
            {
                CreateLiquidFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(55, 602), new Point(55, 400), new Point(90, 400)), liquidFlowColor, 1.5);
            }
            // C2 LOWER BLUE LIQUID BRANCH          
            CreateLiquidFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(1140, 800), new Point(1140, 590), new Point(1080, 590)), liquidFlowColor, 1.5);
            // C2 UPPER BLUE LIQUID BRANCH
            if (_c2PumpRunning)
            {
                CreateLiquidFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(662, 602), new Point(662, 400), new Point(690, 400)), liquidFlowColor, 1.5);
            }
            // C3 LOWER BLUE LIQUID BRANCH
            CreateLiquidFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(1730, 800), new Point(1730, 590), new Point(1670, 590)), liquidFlowColor, 1.5);
            // C3 UPPER BLUE LIQUID BRANCH
            if (_c3PumpRunning)
            {
                CreateLiquidFlow(PipeAnimationCanvas, CreatePathGeometry(new Point(1252, 602), new Point(1252, 400), new Point(1290, 400)), liquidFlowColor, 1.5);
            }
        }
        private PathGeometry CreateLinePathGeometry(double x1, double y1, double x2, double y2)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(x1, y1),
                IsClosed = false,
                IsFilled = false
            };
            figure.Segments.Add(new LineSegment(new Point(x2, y2), true));
            geometry.Figures.Add(figure);
            return geometry;
        }
        private PathGeometry CreatePathGeometry(params Point[] points)
        {
            var geometry = new PathGeometry();
            if (points == null || points.Length == 0)
                return geometry;
            var figure = new PathFigure
            {
                StartPoint = points[0],
                IsClosed = false,
                IsFilled = false
            };
            for (int i = 1; i < points.Length; i++)
            {
                figure.Segments.Add(new LineSegment(points[i], true));
            }
            geometry.Figures.Add(figure);
            return geometry;
        }
        private void CreateGasPuffFlow(Canvas canvas, PathGeometry geometry, int count = 28, double baseDurationSeconds = 2.8, Color? flowColor = null)
        {
            if (canvas == null || geometry == null || geometry.Figures.Count == 0)
                return;
            Color coreColor = flowColor ?? Colors.White;
            for (int i = 0; i < count; i++)
            {
                //double w = 5.0 + _gasRandom.NextDouble() * 5.0;
                //double h = w * (0.50 + _gasRandom.NextDouble() * 0.5);
                double w = 10 + _gasRandom.NextDouble() * 5;
                double h = w * (0.5 + _gasRandom.NextDouble() * 0.5);
                var puff = new Ellipse
                {
                    Width = w,
                    Height = h,
                    Opacity = 0,
                    IsHitTestVisible = false
                };
                var brush = new RadialGradientBrush();
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(220, coreColor.R, coreColor.G, coreColor.B), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(110, coreColor.R, coreColor.G, coreColor.B), 0.45));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, coreColor.R, coreColor.G, coreColor.B), 1.0));
                puff.Fill = brush;
                puff.Effect = new BlurEffect
                {
                    Radius = 4
                };
                var scaleTransform = new ScaleTransform(1, 1, w / 2, h / 2);
                puff.RenderTransform = scaleTransform;
                canvas.Children.Add(puff);
                double initialProgress = (double)i / count;
                FlowParticle particle = new FlowParticle
                {
                    Element = puff,
                    Geometry = geometry,
                    Progress = initialProgress,
                    Speed = 0.105 + _gasRandom.NextDouble() * 0.035,
                    IsLiquid = false
                };
                _flowParticles.Add(particle);
                double durationSeconds = 1.8 + _gasRandom.NextDouble() * 0.7;
                var opacityAnimation = new DoubleAnimationUsingKeyFrames
                {
                    Duration = TimeSpan.FromSeconds(durationSeconds),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromPercent(0)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.85, KeyTime.FromPercent(0.08)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.85, KeyTime.FromPercent(0.88)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromPercent(1)));
                puff.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);
                var scaleAnimation = new DoubleAnimation
                {
                    From = 0.85,
                    To = 1.25,
                    Duration = TimeSpan.FromSeconds(1.2 + _gasRandom.NextDouble() * 1.0),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }
            StartFlowRendering();
        }
        private void FlowAnimationRendering(object? sender, EventArgs e)
        {
            if (!_flowAnimationRunning)
                return;
            DateTime now = DateTime.Now;
            double deltaTime = (now - _lastFlowUpdate).TotalSeconds;
            _lastFlowUpdate = now;
            foreach (FlowParticle particle in _flowParticles)
            {
                particle.Progress += particle.Speed * deltaTime;
                if (particle.Progress > 1.0)
                    particle.Progress -= 1.0;
                Point point;
                Point tangent;
                particle.Geometry.GetPointAtFractionLength(particle.Progress, out point, out tangent);
                double width = particle.Element.Width;
                double height = particle.Element.Height;
                Canvas.SetLeft(particle.Element, point.X - width / 2);
                Canvas.SetTop(particle.Element, point.Y - height / 2);
                if (particle.IsLiquid && particle.Element.RenderTransform is RotateTransform rotate)
                {
                    double angle = Math.Atan2(tangent.Y, tangent.X) * 180 / Math.PI;
                    rotate.Angle = angle;
                }
            }
        }
        private void StartFlowRendering()
        {
            if (_flowAnimationRunning)
                return;
            _flowAnimationRunning = true;
            _lastFlowUpdate = DateTime.Now;
            CompositionTarget.Rendering += FlowAnimationRendering;
        }
        private void CreateLiquidFlow(Canvas canvas, PathGeometry geometry, Color flowColor, double durationSeconds)
        {
            if (canvas == null)
                return;
            if (geometry == null)
                return;
            if (geometry.Figures.Count == 0)
                return;
            var flow = new Path
            {
                Data = geometry,
                Stroke = new SolidColorBrush(flowColor),
                StrokeThickness = 3,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashArray = new DoubleCollection { 10, 3 },
                IsHitTestVisible = false
            };
            flow.Effect = new DropShadowEffect
            {
                Color = flowColor,
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.90
            };
            canvas.Children.Add(flow);
            var animation = new DoubleAnimation
            {
                From = 0,
                To = -26,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                RepeatBehavior = RepeatBehavior.Forever
            };
            flow.BeginAnimation(Shape.StrokeDashOffsetProperty, animation);
        }
        private bool ShowConfirmation(string title, string message)
        {
            var dialog = new ConfirmationDialog(title, message);
            dialog.Owner = Window.GetWindow(this);
            bool? result = dialog.ShowDialog();
            return result == true;
        }
        private iDbRepository GetRepository()
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConfigDBConnString"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Error: ConfigDBConnString not found in configuration.");
                    return null;
                }
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                var context = new AppDbContext(options);
                return new dbRepository(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating repository: {ex.Message}");
                return null;
            }
        }
        private void StopFlowRendering()
        {
            if (!_flowAnimationRunning)
                return;
            _flowAnimationRunning = false;
            CompositionTarget.Rendering -= FlowAnimationRendering;
        }
        private class FlowParticle
        {
            public FrameworkElement Element { get; set; }
            public PathGeometry Geometry { get; set; }
            public double Progress { get; set; }
            public double Speed { get; set; }
            public bool IsLiquid { get; set; }
        }
    }
}