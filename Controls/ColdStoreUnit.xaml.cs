using FluentModbus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VirtualEMS.DataServices;
using VirtualEMS.Library;
using WPFSCADA.PopUps;

namespace WPFSCADA.Controls
{
    /// <summary>
    /// Interaction logic for ColdStoreUnit.xaml
    /// </summary>
    public partial class ColdStoreUnit : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        private string _name;
        public string UnitName { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(nameof(UnitName)); } } }
        private bool _isRemote = false;
        public bool IsRemote { get => _isRemote; set { if (_isRemote != value) { _isRemote = value; OnPropertyChanged(nameof(IsRemote)); } } }

        private bool _isAutomated = false;
        public bool IsAutomated { get => _isAutomated; set { if (_isAutomated != value) { _isAutomated = value; OnPropertyChanged(nameof(IsAutomated));  } } }

        private bool manualDefrostCommand = false;
        public bool ManualDefrostCommand { get => manualDefrostCommand; set { if (manualDefrostCommand != value) { manualDefrostCommand = value; OnPropertyChanged(nameof(ManualDefrostCommand)); } } }

        private bool fansStartCommand = false;
        public bool FansStartCommand { get => fansStartCommand; set { if (fansStartCommand != value) { fansStartCommand = value; OnPropertyChanged(nameof(FansStartCommand)); } } }
        private bool fansStopCommand = false;
        public bool FansStopCommand { get => fansStopCommand; set { if (fansStopCommand != value) { fansStopCommand = value; OnPropertyChanged(nameof(FansStopCommand)); } } }

        private bool _fan1RunFB = false;
        public bool Fan1RunFB { get => _fan1RunFB; set { if (_fan1RunFB != value) { _fan1RunFB = value; OnPropertyChanged(nameof(Fan1RunFB)); } } }

        private bool _fan2RunFB = false;
        public bool Fan2RunFB { get => _fan2RunFB; set { if (_fan2RunFB != value) { _fan2RunFB = value; OnPropertyChanged(nameof(Fan2RunFB)); } } }

        private bool _fan3RunFB = false;
        public bool Fan3RunFB { get => _fan3RunFB; set { if (_fan3RunFB != value) { _fan3RunFB = value; OnPropertyChanged(nameof(Fan3RunFB)); } } }

        private bool _fan4RunFB = false;
        public bool Fan4RunFB { get => _fan4RunFB; set { if (_fan4RunFB != value) { _fan4RunFB = value; OnPropertyChanged(nameof(Fan4RunFB)); } } }

        private bool _unitRunFB = false;
        public bool UnitRunFB { get => _unitRunFB; set { if (_unitRunFB != value) { _unitRunFB = value; OnPropertyChanged(nameof(UnitRunFB)); } } }

        private double _evaporatorTemp;
        public double EvaporatorTemp { get => _evaporatorTemp; set { if (_evaporatorTemp != value) { _evaporatorTemp = value; OnPropertyChanged(nameof(EvaporatorTemp)); } } }

        private int _liquidDrainMinsSetpoint, _defrostMinsSetpoint;
        public int LiquidDrainMinsSetpoint { get => _liquidDrainMinsSetpoint; set { if (_liquidDrainMinsSetpoint != value) { _liquidDrainMinsSetpoint = value; OnPropertyChanged(nameof(LiquidDrainMinsSetpoint)); } } }
        public int DefrostMinsSetpoint { get => _defrostMinsSetpoint; set { if (_defrostMinsSetpoint != value) { _defrostMinsSetpoint = value; OnPropertyChanged(nameof(DefrostMinsSetpoint)); } } }
        //public int FreezeHrsSetpoint { get => _freezeHrsSetpoint; set { if (_freezeHrsSetpoint != value) { _freezeHrsSetpoint = value; OnPropertyChanged(nameof(FreezeHrsSetpoint)); } } }
        //public int FreezeMinsSetpoint { get => _freezeMinsSetpoint; set { if (_freezeMinsSetpoint != value) { _freezeMinsSetpoint = value; OnPropertyChanged(nameof(FreezeMinsSetpoint)); } } }

        private int _liquidDrainMins = 0, _defrostMins = 0, _freezeHrs = 0, _freezeMins = 0;
        public int LiquidDrainMins { get => _liquidDrainMins; set { if (_liquidDrainMins != value) { _liquidDrainMins = value; OnPropertyChanged(nameof(LiquidDrainMins)); } } }
        public int DefrostMins { get => _defrostMins; set { if (_defrostMins != value) { _defrostMins = value; OnPropertyChanged(nameof(DefrostMins)); } } }
        public int FreezeHrs { get => _freezeHrs; set { if (_freezeHrs != value) { _freezeHrs = value; OnPropertyChanged(nameof(FreezeHrs)); } } }
        public int FreezeMins { get => _freezeMins; set { if (_freezeMins != value) { _freezeMins = value; OnPropertyChanged(nameof(FreezeMins)); } } }

        private bool _lsvFb = false, _ssvFb = false, _hgsvFb = false, _hgrsvFb = false;
        public bool LsvFb { get => _lsvFb; set { if (_lsvFb != value) { _lsvFb = value; OnPropertyChanged(nameof(LsvFb)); UpdatePipeAnimations(); } } }
        public bool SsvFb { get => _ssvFb; set { if (_ssvFb != value) { _ssvFb = value; OnPropertyChanged(nameof(SsvFb)); UpdatePipeAnimations(); } } }
        public bool HgsvFb { get => _hgsvFb; set { if (_hgsvFb != value) { _hgsvFb = value; OnPropertyChanged(nameof(HgsvFb)); UpdatePipeAnimations(); } } }
        public bool HgrsvFb { get => _hgrsvFb; set { if (_hgrsvFb != value) { _hgrsvFb = value; OnPropertyChanged(nameof(HgrsvFb)); UpdatePipeAnimations(); } } }

        //private string _mode = "FREEZING";
        //public string Mode { get => _mode; set { if (_mode != value) { _mode = value; OnPropertyChanged(nameof(Mode)); } } }
        private MODE _mode = MODE.IDEAL;
        public MODE Mode { get => _mode; set { if (_mode != value) { _mode = value; OnPropertyChanged(nameof(Mode)); } } }
        private bool _suppressPlcWrite = false;
        public iDbRepository Repository { get; private set; }
        public enum MODE
        {
            IDEAL,
            FREEZING,
            DEFROSTING,
        }
        // command TagIds
        int autoManualTagId, manualDefrostTagId, fansStartTagId, fansStopTagId;
        public int AutoManualTagId { get => autoManualTagId; set { if (autoManualTagId != value) { autoManualTagId = value; OnPropertyChanged(nameof(autoManualTagId)); } } }
        public int ManualDefrostTagId { get => manualDefrostTagId; set { if (manualDefrostTagId != value) { manualDefrostTagId = value; OnPropertyChanged(nameof(manualDefrostTagId)); } } }
        private readonly List<FlowParticle> _flowParticles = new();

        private bool _flowAnimationRunning = false;

        private DateTime _lastFlowUpdate = DateTime.Now;

        public ColdStoreUnit()
        {
            InitializeComponent();
            // ensure runtime DataContext is also the control itself
            this.DataContext = this;

            // Initialize the repository
            Repository = GetRepository();

            // set initial enabled state based on RemoteLocalToggle (false = REMOTE)
            UpdateControlsForRemoteLocal(RemoteLocalToggle?.IsChecked == true);
            // attach handlers (in case Toggle events aren't wired in XAML)
            //RemoteLocalToggle.Checked += RemoteLocalToggle_Checked;
            //RemoteLocalToggle.Unchecked += RemoteLocalToggle_Unchecked;
            //AutoManualToggle.Checked += AutoManualToggle_Checked;
            //AutoManualToggle.Unchecked += AutoManualToggle_Unchecked;
            Loaded += ColdStoreUnit_Loaded;
            Unloaded += ColdStoreUnit_Unloaded;

        }
        private void ColdStoreUnit_Loaded( object sender, RoutedEventArgs e)
        {
            UpdatePipeAnimations();
        }
        private void ColdStoreUnit_Unloaded(object sender, RoutedEventArgs e)
        {
            StopPipeAnimations();
        }       
        private readonly Random _random = new Random();
        private void StartGasPipeAnimation( Path pipe,  string particleColor)
        {
            if (pipe.Data is not PathGeometry geometry)
                return;
            // Keep your current gas density
            int particleCount = 28;

            Color coreColor = (Color)ColorConverter.ConvertFromString(particleColor);

            for (int i = 0; i < particleCount; i++)
            {
                // Same general size range as compressor gas
                double w = 5 + _random.NextDouble() * 3;
                double h = w * (0.5 + _random.NextDouble() * 0.25);

                var ellipse = new Ellipse
                {
                    Width = w,
                    Height = h,
                    Opacity = 0,
                    IsHitTestVisible = false
                };
                var brush = new RadialGradientBrush();
                brush.GradientStops.Add( new GradientStop(Color.FromArgb( 220,coreColor.R, coreColor.G, coreColor.B), 0.0));
                brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, coreColor.R, coreColor.G, coreColor.B),1.0));
                ellipse.Fill = brush;
                ellipse.Effect = new BlurEffect
                {
                    Radius = 4
                };
                double initialProgress = (double)i / particleCount;
                var scaleTransform =  new ScaleTransform( 1,  1,  w / 2,  h / 2);
                ellipse.RenderTransform =  scaleTransform;
                FlowAnimationCanvas.Children.Add(ellipse);
                FlowParticle particle = new FlowParticle
                {
                    Element = ellipse,
                    Geometry = geometry,
                    Progress = initialProgress,
                    Speed =  0.105 + _random.NextDouble() * 0.035,
                    IsLiquid = false
                };

                _flowParticles.Add(particle);

                double durationSeconds =  1.8 +  _random.NextDouble() * 0.7;
                var opacityAnimation =  new DoubleAnimationUsingKeyFrames
                    {
                        Duration = TimeSpan.FromSeconds( durationSeconds),
                        RepeatBehavior =  RepeatBehavior.Forever
                    };
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.85, KeyTime.FromPercent(0.08)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame( 0.85,  KeyTime.FromPercent(0.88)));
                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));

                ellipse.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);

                var scaleAnimation = new DoubleAnimation
                    {
                        From = 0.85,
                        To = 1.25,
                        Duration = TimeSpan.FromSeconds(1.2 + _random.NextDouble() * 1.0),
                        AutoReverse = true,
                        RepeatBehavior =  RepeatBehavior.Forever
                    };

                scaleTransform.BeginAnimation( ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }
            StartFlowRendering();
        }
        private void FlowAnimationRendering( object? sender, EventArgs e)
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
                Canvas.SetLeft( particle.Element, point.X - width / 2);
                Canvas.SetTop(particle.Element, point.Y - height / 2);
                if (particle.IsLiquid &&  particle.Element.RenderTransform  is RotateTransform rotate)
                {
                    double angle = Math.Atan2( tangent.Y, tangent.X) * 180 / Math.PI;
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
        private void StartLiquidPipeAnimation( Path pipe, string liquidColor)
        {
            if (pipe.Data is not PathGeometry geometry)
                return;
            var liquidFlow = new Path
            {
                Data = geometry,
                Stroke = new SolidColorBrush( (Color)ColorConverter.ConvertFromString(liquidColor)),

                StrokeThickness = 2,

                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,

                // Same compressor-style liquid flow
                StrokeDashArray = new DoubleCollection  {  10,  2  },
                IsHitTestVisible = false,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = (Color)ColorConverter.ConvertFromString(liquidColor),
                    BlurRadius = 4,
                    ShadowDepth = 0,
                    Opacity = 0.7
                }
            };
            FlowAnimationCanvas.Children.Add(liquidFlow);
            var dashAnimation = new DoubleAnimation
            {
                From = 0,
                To = -24,
                Duration = TimeSpan.FromSeconds(1.8),
                RepeatBehavior = RepeatBehavior.Forever
            };
            liquidFlow.BeginAnimation( Shape.StrokeDashOffsetProperty,  dashAnimation);
        }

        private void UpdatePipeAnimations()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(UpdatePipeAnimations));

                return;
            }
            StopPipeAnimations();
            // BLUE GAS - SSV
            if (SsvFb)
            {
                StartGasPipeAnimation(mainBluePath, "#2196F3");
            }

            // PURPLE GAS - HGSV
            if (HgsvFb)
            {
                StartGasPipeAnimation( mainPurplePath1,"MediumPurple");
            }

            // RED GAS - HGRSV
            if (HgrsvFb)
            {
                StartGasPipeAnimation( mainRedPath,"#B71C1C");
            }
            // YELLOW LIQUID - LSV
            if (LsvFb)
            {
                StartLiquidPipeAnimation( mainOrangePath, "#FFD54F");
            }
        }       
        private void StopPipeAnimations()
        {
            _flowParticles.Clear();
            FlowAnimationCanvas.Children.Clear();

            if (_flowAnimationRunning)
            {
                CompositionTarget.Rendering -= FlowAnimationRendering;
                _flowAnimationRunning = false;
            }
        }
        private void FansGrid_MouseLeftButtonUp( object sender,  MouseButtonEventArgs e)
        {
            var window = new ColdStoreDetailsWindow(this)
            {
                Owner = Window.GetWindow(this)
            };

            window.ShowDialog();
        }
        private bool ShowConfirmation(string title, string message)
        {
            var dialog = new ConfirmationDialog(title,message)
            {
                Owner = Window.GetWindow(this)
            };
            return dialog.ShowDialog() == true;
        }
        private void FanStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAutomated)
                return;
            bool confirmed = ShowConfirmation("Confirm Fan Start", $"Are you sure you want to START the fans ?");
            if (!confirmed)
                return;
            if (_suppressPlcWrite)
                return;

            WriteBoolToPLCAsync(fansStartTagId, true);
        }
        private void FanStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAutomated)
                return;
            bool confirmed = ShowConfirmation( "Confirm Fan Stop", $"Are you sure you want to STOP the fans ?");
            if (!confirmed)
                return;
            if (_suppressPlcWrite)
                return;
            WriteBoolToPLCAsync(fansStopTagId, true);
        }
        private iDbRepository GetRepository()
        {
            try
            {
                // If your app uses App.xaml.cs to store the repository, access it like this:
                // var app = Application.Current as App;
                // return app?.Repository;

                // Create a new context using the connection string from config
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
        private void defrostButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRemote || IsAutomated)
                return;
            bool confirmed = ShowConfirmation("Confirm Manual Defrost", $"Are you sure you want to start manual defrost for {UnitName}?");
            if (!confirmed)
                return;
            WriteBoolToPLCAsync(manualDefrostTagId, true);
        }     
        private void RemoteLocalToggle_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e)
        {
            if (!RemoteLocalToggle.IsEnabled)
                return;

            // Prevent the ToggleButton from changing itself.
            e.Handled = true;

            bool newRemoteState = !RemoteLocalToggle.IsChecked.GetValueOrDefault();
            string mode = newRemoteState ? "REMOTE" : "LOCAL";
            bool confirmed = ShowConfirmation( $"Confirm {mode} Mode", $"Are you sure you want to switch {UnitName} to {mode} mode?");

            if (!confirmed)
            {
                // Do absolutely nothing.
                // The toggle never changed.
                return;
            }

            _suppressPlcWrite = true;

            IsRemote = newRemoteState;
            RemoteLocalToggle.IsChecked = newRemoteState;

            _suppressPlcWrite = false;

            UpdateControlsForRemoteLocal(newRemoteState);
        }
        private void AutoManualToggle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AutoManualToggle.IsEnabled)
                return;

            // Prevent normal ToggleButton behavior.
            e.Handled = true;

            bool newAutoState = !AutoManualToggle.IsChecked.GetValueOrDefault();

            string mode = newAutoState ? "AUTO" : "MANUAL";

            bool confirmed = ShowConfirmation(
                $"Confirm {mode} Mode",
                $"Are you sure you want to switch {UnitName} to {mode} mode?");

            if (!confirmed)
            {
                // Keep the current state.
                return;
            }

            _suppressPlcWrite = true;

            IsAutomated = newAutoState;
            AutoManualToggle.IsChecked = newAutoState;

            _suppressPlcWrite = false;

            UpdateControlsForAutoManual(newAutoState);

            // Send the command only after confirmation.
            WriteBoolToPLCAsync(autoManualTagId, newAutoState);
        }

        private void UpdateControlsForRemoteLocal(bool isRemote)
        {
            if (AutoManualToggle != null)
                AutoManualToggle.IsEnabled = isRemote;

            if (FanStartButton != null)
                FanStartButton.IsEnabled = isRemote;

            if (FanStopButton != null)
                FanStopButton.IsEnabled = isRemote;

            if (defrostButton != null)
                defrostButton.IsEnabled = isRemote;
        }

        private void UpdateControlsForAutoManual(bool isAuto)
        {
            if (FanStartButton != null)
                FanStartButton.IsEnabled = !isAuto;
            if (FanStopButton != null)
                FanStopButton.IsEnabled = !isAuto;
            if (defrostButton != null)
                defrostButton.IsEnabled = !isAuto;
        }
        private void WriteBoolToPLCAsync(int TagId, bool value)
        {
            try
            {
                var allTags = Repository?.GetTags().ToList() ?? new List<Tag>();
                var TargetTag = allTags.FirstOrDefault(t => t.Id == TagId); // your tag id field

                if (TargetTag == null)
                {
                    System.Diagnostics.Debug.WriteLine("Auto/Manual tag not found");
                    return;
                }

                var devices = Repository?.GetIODevices().ToList();
                var plcDevice = devices?.FirstOrDefault(d =>
                    d.Id == TargetTag.DeviceId);

                if (plcDevice == null || string.IsNullOrEmpty(plcDevice.IpAddress))
                {
                    System.Diagnostics.Debug.WriteLine("PLC device not found for Auto/Manual write");
                    return;
                }

                //await Task.Run(() =>
                {
                    using var modbusClient = new ModbusTcpClient();
                    try
                    {
                        modbusClient.Connect(new IPEndPoint(IPAddress.Parse(plcDevice.IpAddress), 502));
                        if (!modbusClient.IsConnected)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to connect to PLC at {plcDevice.IpAddress}");
                            return;
                        }

                        const int unitIdentifier = 1;

                        if (TargetTag.Type == DataType.BOOL)
                        {
                            // Coil write
                            modbusClient.WriteSingleCoil(unitIdentifier, TargetTag.Address, value);
                        }
                        else
                        {
                            WriteWBoolBit(modbusClient, unitIdentifier, TargetTag.Address, TargetTag.Bit, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error writing Auto/Manual to PLC: {ex.Message}");
                    }
                    finally
                    {
                        if (modbusClient.IsConnected)
                            modbusClient.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in WriteAutoManualToPLCAsync: {ex.Message}");
            }
        }
        private void WriteWBoolBit(ModbusTcpClient modbusClient, int unitIdentifier, int registerAddress, int bitIndex, bool value)
        {
            if (bitIndex < 0 || bitIndex > 15)
                throw new ArgumentOutOfRangeException(nameof(bitIndex), "Bit index must be 0-15 for a 16-bit register");

            // Read as ushort (FluentModbus decodes standard order), then swap bytes
            // because this device sends registers byte-swapped
            ushort rawValue = modbusClient.ReadHoldingRegisters<ushort>(unitIdentifier, registerAddress, 1).ToArray()[0];
            ushort currentValue = (ushort)(((rawValue & 0xFF) << 8) | (rawValue >> 8));

            ushort newValue = value
                ? (ushort)(currentValue | (1 << bitIndex))
                : (ushort)(currentValue & ~(1 << bitIndex));

            if (newValue != currentValue)
            {
                // Swap back to device's byte order before writing
                ushort writeValue = (ushort)(((newValue & 0xFF) << 8) | (newValue >> 8));
                modbusClient.WriteSingleRegister(unitIdentifier, registerAddress, writeValue);

            }
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
